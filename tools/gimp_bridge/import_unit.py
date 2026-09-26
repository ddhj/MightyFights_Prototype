#!/usr/bin/env python3
"""Import the artist's GIMP files back into MightyFights unit sheets.

Reads .ora directly and .xcf through GIMP's own batch mode (gimp-console, found automatically or via
GIMP_CONSOLE). The file's "#guides unit=..." layer says which unit it is (the file name is the
fallback). Every visible layer not starting with '#' is merged into the art, cut back into cells,
and compared with the unit's CURRENT sheet:

    a row's frame count  = up to its last non-empty cell (paint into a spare cell to add a frame,
                           erase the last ones to drop them); trailing frames that were already
                           blank in the sheet and are still blank are kept
    unchanged frame      -> keeps its exact crop and trim
    changed / new frame  -> trimmed with stitch.py's own trim()
    hit frames           -> read off the "#hit" layer (a mark in a cell = Collision keyframe)
    everything else      -> ActionTypes, iIncrement timings, other KeyFrames (SelfHeal ...) are
                            carried from the current sheet, never re-derived

and a unit that changed is repacked with stitch.py's own packer into
<out>/<its content folder>/<Unit>.png + <Unit>Array.json, plus preview GIFs in <out>/_preview/<Unit>/.

A NEW unit (made with `export_unit.py --new`) takes its actions and timings from the unit named in
its guides (`like=`), and every row must have at least one frame.

SAFE BY DEFAULT: output goes to tools/gimp_bridge/out/. Only --write-content writes into
content/Sprite Data, and it refuses a file exported from an older sheet (the guides' hash no
longer matches) unless --force -- untouched frames would otherwise revert the newer art.

Examples:
    python tools/gimp_bridge/import_unit.py tools/gimp_bridge/work/Bandit.ora --dry-run
    python tools/gimp_bridge/import_unit.py returned_from_artist/
    python tools/gimp_bridge/import_unit.py returned_from_artist/Bandit.xcf --write-content
"""

import argparse
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unit_sheet as us  # noqa: E402

DEFAULT_OUT = os.path.join(us.HERE, "out")
PREVIEW_SCALE = 3


def merge_art(w, h, layers, fname, warnings):
    """Every visible non-# layer, bottom to top, at its own offset, with its opacity."""
    canvas = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    used = 0
    for L in reversed(layers):                    # read_ora lists top first
        name = L["name"]
        if name.startswith(us.GUIDE_PREFIX):
            continue
        if not L["visible"]:
            warnings.append(f"{fname}: layer '{name}' is hidden -- left out")
            continue
        if L["op"] != "svg:src-over":
            warnings.append(f"{fname}: layer '{name}' uses blend mode {L['op']} -- merged as Normal")
        img = L["img"]
        if L["opacity"] < 1.0:
            a = np.asarray(img, dtype=np.float32).copy()
            a[..., 3] *= L["opacity"]
            img = Image.fromarray(a.round().astype(np.uint8), "RGBA")
        layer = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        layer.paste(img, (L["x"], L["y"]))
        canvas = Image.alpha_composite(canvas, layer)
        used += 1
    return canvas, used


def hit_cells(w, h, layers, cell):
    """{(row, col)} with any visible pixel on a #hit layer; None if there is no #hit layer at all."""
    found = None
    cw, ch = cell
    for L in layers:
        if not L["name"].lower().startswith(us.HIT_TAG):
            continue
        found = found or set()
        canvas = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        canvas.paste(L["img"], (L["x"], L["y"]))
        a = np.asarray(canvas.getchannel("A"))
        for r in range(h // ch):
            for c in range(w // cw):
                if a[r * ch:(r + 1) * ch, c * cw:(c + 1) * cw].any():
                    found.add((r, c))
    return found


def which_unit(path, layers):
    for L in layers:
        if L["name"].startswith(us.GUIDES_TAG):
            tags = us.parse_guides(L["name"])
            if "unit" in tags:
                return tags
    stem = os.path.splitext(os.path.basename(path))[0]
    return {"unit": stem}


def import_file(path, root, force, problems, warnings):
    """Returns a plan dict for one file, or None when it cannot be imported (problems says why)."""
    fname = os.path.basename(path)
    w, h, layers = us.open_layers(path)
    tags = which_unit(path, layers)
    name = tags["unit"]
    new = tags.get("new") == "1"

    if new:
        if name in us.find_units(root):
            problems.append(f"{fname}: a new unit '{name}', but '{name}' already exists in the game -- "
                            f"export it again with --unit {name} to edit it")
            return None
        like = us.UnitSheet(tags.get("like", "Peasant"), root)
        cw, ch = (int(v) for v in tags.get("canvas", "%dx%d" % like.cell).split("x"))
        template = None
        rows = [(a, []) for a, _ in like.actions]
        taxonomy, tax_doc, meta = like.taxonomy, like.doc["ActionTypes"], \
            {"app": "MightyFights gimp_bridge", "version": "1.0", "scale": "1"}
        rel = os.path.join(os.path.dirname(like.rel), name) if os.path.dirname(like.rel) else name
    else:
        template = us.UnitSheet(name, root)
        cw, ch = template.cell
        rows = template.actions
        taxonomy, tax_doc, meta, rel = template.taxonomy, template.doc["ActionTypes"], template.doc.get("meta", {}), template.rel
        stamp, now = tags.get("hash"), us.fingerprint(name, root)
        if stamp and stamp != now:
            msg = f"{fname}: {name}'s sheet changed after this file was exported (#{stamp} vs #{now})"
            (warnings if force else problems).append(
                msg + ("" if force else " -- its untouched frames would revert the newer art; --force to accept"))

    if w % cw or h != len(rows) * ch:
        problems.append(f"{fname}: canvas is {w}x{h}; {name} needs {len(rows)} rows of {cw}x{ch} cells "
                        f"({len(rows) * ch}px high, a multiple of {cw}px wide) -- was the canvas resized or cropped?")
        return None
    cols = w // cw

    art, used = merge_art(w, h, layers, fname, warnings)
    if not used:
        problems.append(f"{fname}: no art layer -- every layer is hidden or starts with '#'")
        return None
    hits = hit_cells(w, h, layers, (cw, ch))
    if hits is None:
        warnings.append(f"{fname}: no '#hit' layer -- keeping the hit frames the sheet already had")

    frames, report, changed = [], [], False
    for r, (act, ix) in enumerate(rows):
        cells = [art.crop((c * cw, r * ch, (c + 1) * cw, (r + 1) * ch)) for c in range(cols)]
        n = max((c + 1 for c, img in enumerate(cells) if not us.is_empty(img)), default=0)
        #### A frame that was BLANK in the sheet and is still blank is kept, not dropped: some sheets
        #### (Drau) end actions on deliberate empty frames, and cutting them would change the timing.
        while template and n < len(ix) and us.is_empty(template.cell_image(ix[n])):
            n += 1
        main_t = taxonomy[act][0]
        if n == 0:
            problems.append(f"{fname}: row '{act}' is empty -- every action needs at least one frame")
            continue

        old_kf = template.keyframes(act) if template else {}
        if hits is None:
            row_hits = {k for k, kf in old_kf.items() if kf.get("Type") == us.HIT_KEY}
        else:
            row_hits = {c for (rr, c) in hits if rr == r}
        for c in sorted(row_hits):
            if c >= n:
                problems.append(f"{fname}: row '{act}' has a hit mark on frame {c:02d}, but its last frame is {n - 1:02d}")
        if row_hits and main_t != "Attack":
            warnings.append(f"{fname}: row '{act}' ({main_t}) has a hit mark -- only attacks use it")
        if main_t == "Attack" and not row_hits:
            warnings.append(f"{fname}: attack '{act}' has no hit frame -- it will never deal damage")

        edited = []
        for k in range(n):
            cell = cells[k]
            kf = None
            other = old_kf.get(k)
            if other and other.get("Type") != us.HIT_KEY:
                kf = other
                if k in row_hits:
                    problems.append(f"{fname}: {act}{k:02d} is marked as a hit but already carries a "
                                    f"{other.get('Type')} keyframe; a frame holds one keyframe")
            elif k in row_hits:
                kf = {"Type": us.HIT_KEY, "oData": ""}
            if template and k < len(ix) and us.same_pixels(cell, template.cell_image(ix[k])):
                s = template.frames[ix[k]]["spriteSourceSize"]
                crop, sss = template.crops[ix[k]], (s["x"], s["y"], s["w"], s["h"])
            else:
                crop, x, y, tw, th = us.trim(cell)
                sss = (x, y, tw, th)
                edited.append(k)
            frames.append({"filename": f"{act}{k:02d}.png", "img": crop, "sss": sss, "KeyFrame": kf,
                           "cell": cell})
        for k, kf in old_kf.items():
            if k >= n and kf.get("Type") != us.HIT_KEY:
                warnings.append(f"{fname}: {act}{k:02d} was removed and its {kf.get('Type')} keyframe with it")

        old_hits = {k for k, kf in old_kf.items() if kf.get("Type") == us.HIT_KEY}
        delta = []
        if template and n != len(ix):
            delta.append(f"{len(ix)} -> {n} frames")
        if edited and template:
            delta.append("repainted " + ", ".join(f"{k:02d}" for k in edited[:8]) + (" ..." if len(edited) > 8 else ""))
        if template and row_hits != old_hits:
            delta.append("hit " + (",".join(f"{k:02d}" for k in sorted(old_hits)) or "none") + " -> "
                         + (",".join(f"{k:02d}" for k in sorted(row_hits)) or "none"))
        if not template:
            delta.append(f"{n} frames" + (", hit " + ",".join(f"{k:02d}" for k in sorted(row_hits)) if row_hits else ""))
        if delta:
            changed = True
            report.append(f"  {act:<10} " + "; ".join(delta))

        #### the flip-jump lesson (docs/AI_ART_PIPELINE.md): off-centre movement frames make the unit
        #### teleport sideways every time it turns
        if act in us.MOVE_ACTIONS:
            jump = max(us.flip_jump(f["sss"], cw) for f in frames[-n:])
            if jump > us.FLIP_JUMP_WARN:
                warnings.append(f"{fname}: '{act}' body is off the canvas midline (turning jumps it {jump:.0f}px "
                                f"sideways) -- centre it on the dotted blue line"
                                + ("" if delta else " (already so in the current sheet)"))

    missing = [a for a in us.TROOPER_ACTIONS if a not in taxonomy]
    if missing and "Troopers" in rel:
        warnings.append(f"{fname}: {name} has no {missing} -- the trooper AI asks for those by name")
    return {"name": name, "new": new, "rel": rel, "cell": (cw, ch), "tax_doc": tax_doc, "meta": meta,
            "taxonomy": taxonomy, "frames": frames, "report": report, "changed": changed, "file": fname}


def write_previews(plan, out_dir):
    """One looping GIF per action at its game timing, scaled up, so the artist can see it move."""
    folder = os.path.join(out_dir, "_preview", plan["name"])
    os.makedirs(folder, exist_ok=True)
    by_act = {}
    for f in plan["frames"]:
        by_act.setdefault(us.action_of(f["filename"]), []).append(f)
    cw, ch = plan["cell"]
    for act, fs in by_act.items():
        imgs = []
        for f in fs:
            bg = Image.new("RGBA", (cw, ch), (96, 112, 80, 255))
            bg.alpha_composite(f["cell"])
            if f.get("KeyFrame", {}) and f["KeyFrame"].get("Type") == us.HIT_KEY:
                bg.paste(us.HIT_COLOUR, (cw - 6, 2, cw - 2, 6))
            imgs.append(bg.convert("RGB").resize((cw * PREVIEW_SCALE, ch * PREVIEW_SCALE), Image.NEAREST))
        inc = plan["taxonomy"][act][2]
        imgs[0].save(os.path.join(folder, f"{act}.gif"), save_all=True, append_images=imgs[1:],
                     duration=max(20, inc), loop=0, disposal=2)
    return folder


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("inputs", nargs="+", help=".ora / .xcf files, or folders of them")
    ap.add_argument("--out", default=DEFAULT_OUT, help=f"output folder (default {DEFAULT_OUT})")
    ap.add_argument("--root", default=us.SPRITE_DATA, help="where the units' current sheets live (the templates)")
    ap.add_argument("--write-content", action="store_true",
                    help="write straight into content/Sprite Data (the shipped sheets) instead of --out")
    ap.add_argument("--force", action="store_true", help="import even though the unit's sheet changed since the export")
    ap.add_argument("--dry-run", action="store_true", help="report what would change, write nothing")
    ap.add_argument("--all", action="store_true", help="write every unit seen, even unchanged ones")
    ap.add_argument("--no-preview", action="store_true", help="skip the preview GIFs")
    args = ap.parse_args()

    #### THE PACKAGED BRIDGE (UnitBridge.exe) WRITES INSIDE ITS OWN FOLDER, AND NOWHERE ELSE. Its content/
    #### is the template every import compares against; writing there would make the next import compare
    #### against the artist's own edit.
    if us.FROZEN:
        if args.write_content:
            raise SystemExit("the packaged bridge never writes into its own content/ -- the output goes to "
                             "out/ beside UnitBridge.exe")
        root = os.path.normcase(os.path.abspath(us.REPO))
        dest = os.path.normcase(os.path.abspath(args.out))
        content = os.path.join(root, "content")

        def inside(folder, path):
            try:
                return os.path.commonpath([folder, path]) == folder
            except ValueError:                      # another drive
                return False
        if not inside(root, dest) or inside(content, dest):
            raise SystemExit(f"--out must be a folder inside {us.REPO} (and not its content/); got {args.out}")

    files = []
    for p in args.inputs:
        if os.path.isdir(p):
            files += sorted(os.path.join(p, f) for f in os.listdir(p) if f.lower().endswith((".ora", ".xcf")))
        else:
            files.append(p)
    if not files:
        raise SystemExit("no .ora/.xcf files given")
    #### the same unit saved as both .xcf and .ora (what the README asks GIMP 3 users to send) is one edit:
    #### take the .ora, which needs no GIMP to read
    stems = {}
    for f in files:
        key = os.path.splitext(os.path.abspath(f))[0].lower()
        if key not in stems or f.lower().endswith(".ora"):
            stems[key] = f
    files = sorted(stems.values())

    problems, warnings, plans = [], [], {}
    for path in files:
        plan = import_file(path, args.root, args.force, problems, warnings)
        if plan is None:
            continue
        if plan["name"] in plans:
            problems.append(f"{plan['file']} and {plans[plan['name']]['file']} are both {plan['name']} -- "
                            f"import one file per unit")
            continue
        plans[plan["name"]] = plan

    for wmsg in warnings:
        print("warning:", wmsg)
    if problems:
        for p in problems:
            print("ERROR:", p)
        raise SystemExit(1)

    out_root = args.root if args.write_content else args.out
    wrote = []
    for name, plan in plans.items():
        n_frames = len(plan["frames"])
        if plan["new"]:
            print(f"{name}: NEW unit, {plan['cell'][0]}x{plan['cell'][1]}, {n_frames} frames")
        else:
            print(f"{name}: " + ("changed" if plan["changed"] else "unchanged") + f" ({n_frames} frames)")
        for line in plan["report"]:
            print(line)
        if args.dry_run or not (plan["changed"] or plan["new"] or args.all):
            continue
        png, js = us.write_unit(name, plan["tax_doc"], plan["meta"], plan["cell"], plan["frames"],
                                os.path.join(out_root, plan["rel"]))
        wrote.append(plan)
        print(f"  wrote {png}\n  wrote {js}")
        if not args.no_preview:
            print(f"  previews in {write_previews(plan, args.out)}")

    if args.dry_run:
        print("dry run: nothing written")
    elif not wrote:
        print("nothing changed; nothing written")
    elif args.write_content:
        print("\nwrote into content/Sprite Data.")
        for plan in wrote:
            key = (plan["rel"].replace("\\", "/") + "/" + plan["name"] + ".png")
            if plan["new"]:
                print(f"  {plan['name']} is NEW: add it to content/Content.mgcb, then spawn it from code:\n"
                      f"    #begin Sprite Data/{key}\n    /importer:TextureImporter\n"
                      f"    /processor:TextureProcessor\n    /build:Sprite Data/{key}\n")
        print("Next: `dotnet build MightyFights.sln` so MGCB rebuilds the sheets.")
    elif us.FROZEN:
        print(f"\nthe import accepted your files; the rebuilt sheets are in {out_root}, and animated previews "
              f"in {os.path.join(args.out, '_preview')}. Send the .ora/.xcf files you painted back to the developer.")
    else:
        print(f"\noutput is in {out_root}; nothing shipped was touched. Re-run with --write-content to ship it.")


if __name__ == "__main__":
    main()
