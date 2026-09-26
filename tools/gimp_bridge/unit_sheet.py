#!/usr/bin/env python3
"""Shared plumbing for the MightyFights GIMP bridge: unit sheets, ORA files, GIMP batch.

Nothing here is a CLI -- see export_unit.py, import_unit.py and unit_bridge.py.

Modelled on SettlementARPG's tools/gimp_bridge (the paperdoll bridge), cut down to what a
MightyFights unit is: ONE sheet + ONE AnimationData JSON per unit
(`content/Sprite Data/.../<Name>/<Name>.png` + `<Name>Array.json`), side-view, no facings --
the game mirrors the art for the other direction (bDir).

THE IDEA
========
The unit's current sheet is the TEMPLATE. The JSON already holds the ActionTypes taxonomy, the
per-action iIncrement timings and the KeyFrames (`Collision` = the frame an attack lands on,
`SelfHeal` on pant), so the import never re-derives any of that. It replaces pixels, lets an
action grow or shrink by whole frames, and moves hit frames to where the artist marked them:

    untouched frame -> keeps its exact crop and trim from the template
    changed frame   -> re-trimmed with stitch.py's own trim()
    whole sheet     -> repacked with stitch.py's own shelf_pack(), same as a fresh stitch

so an unchanged import of a stitched unit reproduces its JSON exactly.
"""

import hashlib
import io
import json
import os
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
import zipfile

import numpy as np
from PIL import Image

#### FROZEN: the packaged UnitBridge.exe is laid out as the repo in miniature -- `content/Sprite Data/...`
#### beside the exe -- so every path below means the same thing, and `work/` and `out/` land beside the exe.
FROZEN = bool(getattr(sys, "frozen", False))
HERE = os.path.dirname(os.path.abspath(sys.executable if FROZEN else __file__))
REPO = HERE if FROZEN else os.path.dirname(os.path.dirname(HERE))
SPRITE_DATA = os.path.join(REPO, "content", "Sprite Data")
CONTENT_MGCB = os.path.join(REPO, "content", "Content.mgcb")

#### The stitcher's own trim and packer, imported rather than copied, so a bridged sheet and a
#### stitched sheet can never disagree about the trim threshold or the packing.
sys.path.insert(0, os.path.join(REPO, "tools", "sprite_stitcher"))
from stitch import trim, shelf_pack   # noqa: E402

#### The names the trooper AI asks for (stitch.py's `required` list). A unit missing one of these
#### cannot play that state; the import says so.
TROOPER_ACTIONS = ["idle", "low", "stab", "stick", "chop", "chopb", "bigchop", "lunge",
                   "parry", "stance", "ready", "pant", "victory", "death", "deathb",
                   "walk", "run", "flee"]
MOVE_ACTIONS = ("walk", "run", "flee")
FLIP_JUMP_WARN = 12          # px -- stitch.py's threshold, see the flip-jump note there
MAX_FRAMES = 100             # frame names are <action><NN>: two digits

#### Layer names. Anything starting with '#' is a helper layer. Two of them carry meaning:
GUIDE_PREFIX = "#"
GUIDES_TAG = "#guides"       # "#guides unit=Bandit hash=1a2b3c4d cols=12 ..." -- what the file IS
HIT_TAG = "#hit"             # "#hit frames ..." -- a mark in a cell = that frame is a Collision keyframe
HIT_KEY = "Collision"
HIT_COLOUR = (255, 40, 40, 255)


# ---------------------------------------------------------------------------------------------
# finding units
# ---------------------------------------------------------------------------------------------

def find_units(root=SPRITE_DATA):
    """{name: folder relative to root} for every single-texture unit: a `<Name>Array.json` with an
    ActionTypes taxonomy and a `<Name>.png` beside it. The Halberd (one JSON shared by 14 palette-
    swapped textures) and the Halberd front-facing sheet (no taxonomy) do not qualify."""
    units = {}
    for d, _, files in os.walk(root):
        for f in files:
            if not f.endswith("Array.json"):
                continue
            name = f[:-len("Array.json")]
            if name + ".png" not in files:
                continue
            try:
                with open(os.path.join(d, f), "r", encoding="utf-8-sig") as fh:
                    if not json.load(fh).get("ActionTypes"):
                        continue
            except (OSError, ValueError):
                continue
            units[name] = os.path.relpath(d, root)
    return dict(sorted(units.items()))


def unit_paths(name, root=SPRITE_DATA, rel=None):
    rel = rel if rel is not None else find_units(root).get(name)
    if rel is None:
        raise SystemExit(f"no unit '{name}' under {root} (units: {', '.join(find_units(root)) or 'none'})")
    folder = os.path.join(root, rel)
    return os.path.join(folder, name + ".png"), os.path.join(folder, name + "Array.json"), rel


def fingerprint(name, root=SPRITE_DATA):
    png, js, _ = unit_paths(name, root)
    h = hashlib.md5()
    for p in (js, png):
        with open(p, "rb") as f:
            h.update(f.read())
    return h.hexdigest()[:8]


# ---------------------------------------------------------------------------------------------
# a unit sheet
# ---------------------------------------------------------------------------------------------

def action_of(filename):
    """What AnimationDataLoader does: strip the 2 digits before the extension."""
    return filename[:filename.index(".") - 2]


class UnitSheet:
    """One unit's sheet, decoded to per-frame crops on the unit's canvas."""

    def __init__(self, name, root=SPRITE_DATA, paths=None):
        """paths=(png, json) reads a sheet find_units() skips, e.g. one Halberd texture (the guide does)."""
        self.name = name
        if paths:
            self.png, self.json, self.rel = paths[0], paths[1], None
        else:
            self.png, self.json, self.rel = unit_paths(name, root)
        with open(self.json, "r", encoding="utf-8-sig") as f:
            self.doc = json.load(f)
        self.sheet = Image.open(self.png).convert("RGBA")
        self.frames = self.doc["frames"]
        sizes = {(fr["sourceSize"]["w"], fr["sourceSize"]["h"]) for fr in self.frames}
        if len(sizes) != 1:
            raise SystemExit(f"{name}: mixed canvas sizes {sizes}")
        self.cell = sizes.pop()
        self.crops = [self._crop(fr) for fr in self.frames]

        #### the taxonomy: action -> (main, sub, iIncrement)
        self.taxonomy = {}
        for m in self.doc["ActionTypes"]:
            for s in m["SubTypes"]:
                for a in s["Actions"]:
                    self.taxonomy[a["sAction"]] = (m["MainType"], s["Type"], a["iIncrement"])

        #### the actions in FRAME order (the order the loader walks), each one contiguous
        self.actions = []
        for i, fr in enumerate(self.frames):
            act = action_of(fr["filename"])
            if self.actions and self.actions[-1][0] == act:
                self.actions[-1][1].append(i)
            else:
                if any(a == act for a, _ in self.actions):
                    raise SystemExit(f"{name}: the frames of '{act}' are not contiguous")
                if act not in self.taxonomy:
                    raise SystemExit(f"{name}: frames of '{act}' but no such action in ActionTypes")
                self.actions.append((act, [i]))
        missing = [a for a in self.taxonomy if a not in dict(self.actions)]
        if missing:
            raise SystemExit(f"{name}: ActionTypes lists {missing} but the sheet has no frames for them")

    def _crop(self, fr):
        f = fr["frame"]
        if not fr.get("rotated"):
            return self.sheet.crop((f["x"], f["y"], f["x"] + f["w"], f["y"] + f["h"]))
        #### TexturePacker stores a rotated frame turned 90 degrees clockwise, taking frame.h x frame.w
        #### on the sheet (the loader's `new Rectangle(x, y, frame.h, frame.w)`); turn it back.
        region = self.sheet.crop((f["x"], f["y"], f["x"] + f["h"], f["y"] + f["w"]))
        return region.transpose(Image.ROTATE_90)

    def cell_image(self, i):
        """Frame i on its full untrimmed canvas."""
        fr = self.frames[i]
        out = Image.new("RGBA", self.cell, (0, 0, 0, 0))
        sss = fr["spriteSourceSize"]
        out.paste(self.crops[i], (sss["x"], sss["y"]))
        return out

    def keyframes(self, action):
        """{frame position within the action: KeyFrame dict}"""
        ix = dict(self.actions)[action]
        return {k: self.frames[i]["KeyFrame"] for k, i in enumerate(ix) if "KeyFrame" in self.frames[i]}


def same_pixels(a, b):
    """Pixel equality where a fully transparent pixel's RGB does not count -- GIMP is free to zero
    it, and it is invisible either way."""
    x = np.asarray(a.convert("RGBA"), dtype=np.uint8).copy()
    y = np.asarray(b.convert("RGBA"), dtype=np.uint8).copy()
    if x.shape != y.shape:
        return False
    x[x[..., 3] == 0] = 0
    y[y[..., 3] == 0] = 0
    return np.array_equal(x, y)


def is_empty(img):
    return img.getchannel("A").getbbox() is None


def flip_jump(sss, canvas_w):
    """How far the sprite (and tCenter) jumps sideways when bDir flips -- see stitch.py."""
    return abs(canvas_w - 2 * (sss[0] + sss[2] / 2))


def write_unit(name, taxonomy_doc, meta, cell, frames, out_dir):
    """Pack and write one unit exactly the way stitch.py main() does. frames = [dict(filename,
    img=trimmed crop, sss=(x, y, w, h), KeyFrame=optional)] in frame order."""
    entries = [{"img": f["img"]} for f in frames]
    sheet_w, sheet_h = shelf_pack(entries)
    img = Image.new("RGBA", (sheet_w, sheet_h), (0, 0, 0, 0))
    for e in entries:
        img.paste(e["img"], (e["x"], e["y"]))

    cw, ch = cell
    frames_json = []
    for f, e in zip(frames, entries):
        sss = f["sss"]
        fr = {"filename": f["filename"],
              "frame": {"x": e["x"], "y": e["y"], "w": e["img"].width, "h": e["img"].height},
              "rotated": False,
              "trimmed": (sss[2], sss[3]) != (cw, ch),
              "spriteSourceSize": {"x": sss[0], "y": sss[1], "w": sss[2], "h": sss[3]},
              "sourceSize": {"w": cw, "h": ch}}
        if f.get("KeyFrame"):
            fr["KeyFrame"] = f["KeyFrame"]
        frames_json.append(fr)

    meta = dict(meta)
    meta["image"] = f"{name}.png"
    meta["size"] = {"w": sheet_w, "h": sheet_h}
    doc = {"ActionTypes": taxonomy_doc, "frames": frames_json, "meta": meta}

    os.makedirs(out_dir, exist_ok=True)
    png_path = os.path.join(out_dir, f"{name}.png")
    json_path = os.path.join(out_dir, f"{name}Array.json")
    img.save(png_path)
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(doc, f, indent=1)
    return png_path, json_path


# ---------------------------------------------------------------------------------------------
# the #guides layer's tag: what the file is, readable after GIMP has had it
# ---------------------------------------------------------------------------------------------

def guides_name(**tags):
    return GUIDES_TAG + " " + " ".join(f"{k}={v}" for k, v in tags.items()) + " (ignored on import)"


def parse_guides(name):
    out = {}
    for tok in name[len(GUIDES_TAG):].split():
        if "=" in tok:
            k, v = tok.split("=", 1)
            out[k] = v
    return out


# ---------------------------------------------------------------------------------------------
# OpenRaster: a zip of PNG layers plus stack.xml (https://www.openraster.org/)
# ---------------------------------------------------------------------------------------------

def png_bytes(img):
    buf = io.BytesIO()
    img.save(buf, "PNG")
    return buf.getvalue()


def write_ora(path, size, layers, merged):
    """layers: [(name, Image, visible, opacity)] BOTTOM TO TOP (stack.xml lists them top first)."""
    w, h = size
    stack = ET.Element("image", {"version": "0.0.3", "w": str(w), "h": str(h)})
    root = ET.SubElement(stack, "stack")
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED) as z:
        #### the spec: "mimetype" first, STORED, no newline
        z.writestr(zipfile.ZipInfo("mimetype"), "image/openraster", compress_type=zipfile.ZIP_STORED)
        for n, (name, img, visible, opacity) in reversed(list(enumerate(layers))):
            src = f"data/layer{n:03d}.png"
            z.writestr(src, png_bytes(img))
            ET.SubElement(root, "layer", {"name": name, "src": src, "x": "0", "y": "0",
                                          "opacity": f"{opacity:.2f}",
                                          "visibility": "visible" if visible else "hidden",
                                          "composite-op": "svg:src-over"})
        z.writestr("stack.xml", ET.tostring(stack, encoding="utf-8", xml_declaration=True))
        z.writestr("mergedimage.png", png_bytes(merged))
        thumb = merged.copy()
        thumb.thumbnail((256, 256))
        z.writestr("Thumbnails/thumbnail.png", png_bytes(thumb))


def read_ora(path):
    """(w, h, layers) with layers = [dict(name, img, x, y, opacity, visible, op)] TOP FIRST,
    groups flattened in stacking order."""
    with zipfile.ZipFile(path, "r") as z:
        stack = ET.fromstring(z.read("stack.xml"))
        w, h = int(stack.get("w")), int(stack.get("h"))
        out = []

        def walk(node, visible):
            for child in node:
                vis = visible and child.get("visibility", "visible") != "hidden"
                if child.tag == "stack":
                    walk(child, vis)
                elif child.tag == "layer":
                    img = Image.open(io.BytesIO(z.read(child.get("src")))).convert("RGBA")
                    out.append({"name": child.get("name", ""), "img": img,
                                "x": int(float(child.get("x", "0"))),
                                "y": int(float(child.get("y", "0"))),
                                "opacity": float(child.get("opacity", "1.0")),
                                "visible": vis,
                                "op": child.get("composite-op", "svg:src-over")})

        top = stack.find("stack")
        walk(top if top is not None else stack, True)
    return w, h, out


# ---------------------------------------------------------------------------------------------
# GIMP's own batch mode, for .xcf (GIMP 2.10 ships Python-Fu and the file-openraster plug-in)
# ---------------------------------------------------------------------------------------------

def find_gimp():
    """gimp-console-*.exe, or None. GIMP_CONSOLE overrides."""
    env = os.environ.get("GIMP_CONSOLE")
    if env and os.path.isfile(env):
        return env
    for name in ("gimp-console-2.10", "gimp-console-3.0", "gimp-console"):
        p = shutil.which(name)
        if p:
            return p
    roots = [os.path.expandvars(r"%LOCALAPPDATA%\Programs"), r"C:\Program Files", r"C:\Program Files (x86)"]
    for r in roots:
        for sub in ("GIMP 2", "GIMP 3"):
            b = os.path.join(r, sub, "bin")
            if os.path.isdir(b):
                for f in sorted(os.listdir(b)):
                    if f.lower().startswith("gimp-console") and f.lower().endswith(".exe"):
                        return os.path.join(b, f)
    return None


def gimp_convert(src, dst):
    """Open `src` in GIMP headless and save it as `dst`; the extension picks the format
    (.xcf <-> .ora). Python-Fu under GIMP 2.10 is Python 2, so the snippet stays py2-clean.
    (Run from Python via subprocess, not through a PowerShell pipeline -- tools/gimp_xcf's
    README records GIMP batch mode hanging under PowerShell.)"""
    gimp = find_gimp()
    if gimp is None:
        raise SystemExit("GIMP is not installed here (no gimp-console found; set GIMP_CONSOLE), "
                         "so a .xcf cannot be read. Ask the artist to File > Export As... .ora instead.")
    s, d = src.replace("\\", "/"), dst.replace("\\", "/")
    code = ("img = pdb.gimp_file_load('%s', '%s'); "
            "pdb.gimp_file_save(img, img.layers[0], '%s', '%s'); "
            "pdb.gimp_image_delete(img)") % (s, s, d, d)
    if os.path.exists(dst):
        os.remove(dst)
    r = subprocess.run([gimp, "-i", "-d", "-f", "--batch-interpreter", "python-fu-eval",
                        "-b", code, "-b", "pdb.gimp_quit(1)"],
                       capture_output=True, text=True, timeout=600, stdin=subprocess.DEVNULL)
    if not os.path.isfile(dst):
        raise SystemExit(f"GIMP failed to convert {src} -> {dst}:\n{r.stdout}\n{r.stderr}")
    return dst


def open_layers(path):
    """Read a .ora directly, or a .xcf through GIMP."""
    ext = os.path.splitext(path)[1].lower()
    if ext == ".ora":
        return read_ora(path)
    if ext == ".xcf":
        tmp = tempfile.mkdtemp(prefix="gimp_bridge_")
        try:
            ora = gimp_convert(os.path.abspath(path), os.path.join(tmp, "converted.ora"))
            return read_ora(ora)
        finally:
            shutil.rmtree(tmp, ignore_errors=True)
    raise SystemExit(f"{path}: expected a .ora or .xcf")
