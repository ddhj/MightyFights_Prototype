#!/usr/bin/env python3
"""MightyFights sprite stitcher.

Rebuilds the lost animation-encoding step of the original toolchain (see
docs/DESIGN_DIRECTION.md "Art pipeline"): takes per-frame PNG folders (the layout of
"C:\\dev\\art assets raw\\wodyn\\<unit>") and/or animated GIFs, packs a single sprite
sheet, and emits the game's AnimationData JSON -- the exact hybrid format
`AnimationDataLoader.cs` parses (TexturePacker-style "frames" plus the hand-authored
"ActionTypes" taxonomy with per-action iIncrement timing).

Key game-compat rules baked in (derived from the loader + trooper AI, not guessed):
  * Frame filenames are `<action><NN>.png` with exactly 2-digit numbering; the loader
    derives the action by stripping the last 2 chars before the extension.
  * Frames of one action must be contiguous and in order in the "frames" array.
  * The trooper AI requests animations BY NAME (idle, low, stab, stick, chop, chopb,
    bigchop, lunge, parry, stance, ready, pant, victory, death, deathb, walk, run,
    flee), so configs alias/duplicate source animations onto those names rather than
    changing game code.
  * The Halberd canvas is 100x64 and AnimationDataLoader's flip math assumes width
    100; inputs are validated against the canvas.

Usage:
  python stitch.py <config.json> --out <outdir>

Config shape (see peasant.config.json next to this script):
  {
    "name": "Peasant",
    "canvas": [100, 64],
    "source": "C:/dev/art assets raw/wodyn/peasant_pngs",   # folder-of-action-folders
    "gifs": { "actionname": "path/to/anim.gif", ... },       # optional gif sources
    "recenter": "idle",     # optional: shift ALL frames by one constant dx so this
                            # action's first-frame body center sits on the canvas
                            # midline -- see the flip-jump note in main()
    "flipX": true,          # optional: mirror every frame horizontally. The game's
                            # convention is art faces LEFT (bDir flips it right);
                            # set this when a source (e.g. AI-generated) faces right
    "actions": [
      { "main": "Attack", "sub": "Basic", "name": "chop", "src": "pea_1knife", "increment": 100 },
      ...
    ]
  }

Grid mode (see lpc_character.config.json next to this script): instead of "source"/"gifs",
give a "grid" block and slice ONE fixed-cell sheet -- the top-down 4-facing layouts, where
an animation is a block of cells with one FACING AXIS and one FRAME AXIS. Which axis is
which is the "axis" option; both layouts are first-class:

  "axis": "rows-are-directions"  (DEFAULT) one row per facing, frames run left-to-right.
                                 LPC "Universal Spritesheet" art -- lpc_character.config.json
  "axis": "cols-are-directions"  one COLUMN per facing, frames run top-to-bottom. The
                                 transposed convention -- ninja_adventure_character.config.json

  {
    "name": "LPCCharacter",
    "grid": {
      "sheet": "path/to/character.png",
      "cell": [64, 64],                               # per-asset; see the cell note below
      "axis": "rows-are-directions",                  # optional, this is the default
      "directions": ["Up", "Left", "Down", "Right"],  # axis order; rename these freely
      "mirror": { "Right": "Left" }                   # optional, see below
    },
    "actions": [
      { "main": "Move", "sub": "{dir}", "name": "walk{dir}", "row": 8, "frames": 9,
        "col": 0, "increment": 100 },                 # "col"/"dirs"/"mirror" optional
      { "main": "Death", "sub": "Normal", "name": "death", "row": 20, "frames": 6,
        "dirs": ["Down"] }                            # single lane, non-directional
    ]
  }
  * "row"/"col" are the top-left cell of the action's block and "frames" is its length
    along the FRAME axis. The action then consumes one consecutive lane along the FACING
    axis per entry in its direction list (per-action "dirs", else the sheet's
    "directions") -- consecutive rows under "rows-are-directions", consecutive columns
    under "cols-are-directions". "{dir}" in main/sub/name is substituted with the
    direction name, so the facing can sit at ANY of the engine's three taxonomy levels.
  * "{dir}" MUST appear in "name" for a directional action: AnimationDataLoader keys
    frames by action name GLOBALLY (it recovers the name from the frame filename), so
    two facings cannot both be called "walk" even under different main/sub.
  * "variants" replaces the facing axis for one action, for the sheets that reuse it as a
    chooser instead of a direction (e.g. the transposed pack's last row, whose 4 columns
    are dead/item/ability/ability2, not facings). One lane per entry, in order, each with
    its own full taxonomy key and no "{dir}" substitution:
      { "row": 6, "frames": 1, "variants": [
          { "main": "Death",   "sub": "Normal", "name": "death" },
          { "main": "Ability", "sub": "Normal", "name": "ability", "increment": 120 } ] }
    It is mutually exclusive with "dirs"/"mirror" (they address the same axis).
  * "mirror": {"Right": "Left"} builds Right by mirroring Left's lane and consumes no lane
    of its own -- the legitimate space saving for sheets that ship only 3 facings. It is
    grid-only and unrelated to "flipX"/"recenter".
  * "cell" is per-config, not per-pack: point a config at a bigger sprite (the boss sheets)
    and just set its own cell size. Guess wrong and load_grid_sheet refuses the sheet and
    lists the square cell sizes that DO divide it evenly -- that is how you find the real
    cell size of an unmeasured sheet.
  * The cell IS the canvas here (omit "canvas", or set it equal to "cell"): top-down
    frames are anchored by their cell, so fit_to_canvas's bottom-center anchoring must
    never run. "recenter" is rejected in grid mode for the same reason -- its single
    constant dx exists to fix the side-view bDir flip-jump (see main()) and is meaningless
    when every facing needs its own anchor.
"""

import argparse
import json
import os
import sys

from PIL import Image, ImageSequence


def load_frames_from_folder(folder):
    """Frames from a folder of numbered PNGs, sorted by filename."""
    names = sorted(f for f in os.listdir(folder) if f.lower().endswith(".png"))
    return [Image.open(os.path.join(folder, f)).convert("RGBA") for f in names]


def load_frames_from_gif(path):
    """Frames from an animated GIF, composited to RGBA (handles palette/disposal)."""
    im = Image.open(path)
    return [frame.convert("RGBA") for frame in ImageSequence.Iterator(im)]


def load_grid_sheet(path, cell):
    """Open a grid-mode sheet and check it divides evenly into cell-sized cells.
    Returns (image, columns, rows)."""
    cw, ch = cell
    if cw < 1 or ch < 1:
        raise SystemExit(f"grid cell must be positive, got {list(cell)}")
    if not os.path.isfile(path):
        raise SystemExit(f"grid sheet not found: {path}")
    sheet = Image.open(path).convert("RGBA")
    if sheet.width % cw or sheet.height % ch:
        # the "fits" hint is the intended way to discover an unmeasured sheet's real cell
        # size (the boss sprites nobody has downloaded yet) -- guess, read, correct
        fits = [n for n in range(8, 257) if not (sheet.width % n or sheet.height % n)]
        raise SystemExit(
            f"grid sheet {path} is {sheet.width}x{sheet.height}, not a whole number of "
            f"{cw}x{ch} cells ({sheet.width % cw}px left over across, "
            f"{sheet.height % ch}px down)"
            + (f"; square cell sizes that DO divide it evenly: {fits}" if fits else ""))
    return sheet, sheet.width // cw, sheet.height // ch


def load_frames_from_grid(sheet, cell, row, col, count, down=False):
    """Frames from one lane of a grid sheet: `count` cells starting at (row, col), walking
    down rows when `down` ("cols-are-directions"), else across columns (the default)."""
    cw, ch = cell
    dr, dc = (1, 0) if down else (0, 1)
    return [sheet.crop(((col + dc * i) * cw, (row + dr * i) * ch,
                        (col + dc * i + 1) * cw, (row + dr * i + 1) * ch))
            for i in range(count)]


def expand_grid_actions(cfg):
    """Grid mode frame acquisition: slice one fixed-cell sheet into the same action
    dicts the folder/GIF path feeds the main loop, with the frames already loaded (key
    "frames"). Everything downstream -- canvas fit, flip, trim, pack, JSON -- is shared.
    See the module docstring for the config shape."""
    grid = cfg["grid"]
    cell = tuple(grid["cell"])
    sheet, cols, rows = load_grid_sheet(grid["sheet"], cell)
    axis = grid.get("axis", "rows-are-directions")
    if axis not in ("rows-are-directions", "cols-are-directions"):
        raise SystemExit(f"grid \"axis\" must be \"rows-are-directions\" (default) or"
                         f" \"cols-are-directions\", got \"{axis}\"")
    down = axis == "cols-are-directions"    # facings across columns, frames down rows
    def_dirs = grid.get("directions", ["Up", "Left", "Down", "Right"])
    def_mirror = grid.get("mirror", {})

    out = []
    for act in cfg["actions"]:
        row0, count, col0 = act.get("row", 0), act["frames"], act.get("col", 0)
        variants = act.get("variants")
        label = act.get("name", f"@row{row0},col{col0}")
        if count < 1 or row0 < 0 or col0 < 0:
            raise SystemExit(f"grid action '{label}': bad row/col/frames "
                             f"({row0}/{col0}/{count})")

        # An action's lanes along the facing axis are normally facings; "variants" swaps
        # them for named one-off animations, for the sheets that reuse that axis as a
        # chooser (dead/item/ability) rather than a direction.
        if variants is not None:
            if "dirs" in act or "mirror" in act:
                raise SystemExit(f"grid action '{label}': \"variants\" replaces the facing axis"
                                 f" and cannot be combined with \"dirs\"/\"mirror\"")
            if not variants:
                raise SystemExit(f"grid action '{label}': empty variant list")
            lanes = [v["name"] for v in variants]
            if len(set(lanes)) != len(lanes):
                raise SystemExit(f"grid action '{label}': variant names must be unique, got {lanes}")
            mirror = {}
        else:
            lanes, mirror = act.get("dirs", def_dirs), act.get("mirror", def_mirror)
            if not lanes:
                raise SystemExit(f"grid action '{label}': empty direction list")
            if len(lanes) > 1 and "{dir}" not in act["name"]:
                raise SystemExit(f"grid action '{label}': directional actions need "
                                 f"\"{{dir}}\" in \"name\" (the loader keys actions by name globally)")

        by_lane, k = {}, 0
        for d in lanes:                         # lanes are consumed in order along the facing
            if d in mirror:                     # axis, mirrored facings taking none of their own
                continue
            r, c = (row0, col0 + k) if down else (row0 + k, col0)
            r1, c1 = (r + count - 1, c) if down else (r, c + count - 1)
            if r1 >= rows or c1 >= cols:
                raise SystemExit(
                    f"grid action '{label}' lane '{d}' wants rows {r}-{r1} x columns {c}-{c1},"
                    f" but {grid['sheet']} is only {cols} columns x {rows} rows of cells")
            by_lane[d] = load_frames_from_grid(sheet, cell, r, c, count, down)
            k += 1
        for d in lanes:
            if d in mirror:
                if mirror[d] not in by_lane:
                    raise SystemExit(f"grid action '{label}': mirror '{d}' <- '{mirror[d]}',"
                                     f" but '{mirror[d]}' is not a sliced facing of this action")
                by_lane[d] = [fr.transpose(Image.FLIP_LEFT_RIGHT) for fr in by_lane[mirror[d]]]

        if variants is not None:
            for v in variants:
                out.append({"main": v["main"], "sub": v["sub"], "name": v["name"],
                            "increment": v.get("increment", act.get("increment", 100)),
                            "frames": by_lane[v["name"]]})
        else:
            for d in lanes:
                out.append({"main": act["main"].replace("{dir}", d),
                            "sub": act["sub"].replace("{dir}", d),
                            "name": act["name"].replace("{dir}", d),
                            "increment": act.get("increment", 100),
                            "frames": by_lane[d]})
    return out


def fit_to_canvas(frame, canvas):
    """Return the frame on the canvas size: exact-size frames pass through; smaller
    frames (gif sources) are anchored bottom-center, matching how the units stand."""
    cw, ch = canvas
    if frame.size == (cw, ch):
        return frame
    if frame.width > cw or frame.height > ch:
        raise SystemExit(f"frame {frame.size} exceeds canvas {canvas}")
    out = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    out.paste(frame, ((cw - frame.width) // 2, ch - frame.height))
    return out


def shift_x(frame, dx):
    """Shift frame content horizontally by dx on the same canvas (see 'recenter')."""
    if dx == 0:
        return frame
    bbox = frame.getbbox()
    if bbox and (bbox[0] + dx < 0 or bbox[2] + dx > frame.width):
        raise SystemExit(f"recenter shift dx={dx} pushes content off the canvas")
    out = Image.new("RGBA", frame.size, (0, 0, 0, 0))
    out.paste(frame, (dx, 0))
    return out


def trim(frame):
    """Trim to the alpha bounding box. Returns (cropped, x, y, w, h)."""
    bbox = frame.getbbox()
    if bbox is None:                     # fully transparent frame: keep a 1px stub
        return frame.crop((0, 0, 1, 1)), 0, 0, 1, 1
    x0, y0, x1, y1 = bbox
    return frame.crop(bbox), x0, y0, x1 - x0, y1 - y0


def shelf_pack(entries, max_width=1024, pad=1):
    """Simple shelf packer. entries: list of dicts with 'img'. Sets x/y, returns sheet size."""
    x = y = shelf_h = 0
    for e in entries:
        w, h = e["img"].size
        if x + w + pad > max_width:
            x = 0
            y += shelf_h + pad
            shelf_h = 0
        e["x"], e["y"] = x, y
        x += w + pad
        shelf_h = max(shelf_h, h)
    return max_width, y + shelf_h


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("config")
    ap.add_argument("--out", required=True)
    args = ap.parse_args()

    with open(args.config, "r", encoding="utf-8") as f:
        cfg = json.load(f)

    name = cfg["name"]
    canvas = tuple(cfg.get("canvas", [100, 64]))
    source = cfg.get("source")
    gifs = cfg.get("gifs", {})
    grid = cfg.get("grid")
    if grid:
        # In grid mode the cell IS the canvas: top-down frames are anchored by their own
        # cell, so neither fit_to_canvas's bottom-center anchoring nor the side-view
        # "recenter" dx may run (both would move every facing by the same amount).
        cell = tuple(grid["cell"])
        canvas = tuple(cfg["canvas"]) if "canvas" in cfg else cell
        if canvas != cell:
            raise SystemExit(f"grid mode: canvas {list(canvas)} must equal cell {list(cell)}"
                             f" (or just omit \"canvas\")")
        if cfg.get("recenter"):
            raise SystemExit("grid mode: \"recenter\" is side-view-only; grid cells are already"
                             " anchored (use per-facing art, not one constant dx)")
    actions = expand_grid_actions(cfg) if grid else cfg["actions"]
    os.makedirs(args.out, exist_ok=True)

    # ---- optional horizontal recentering ----
    # The game's flip math mirrors a frame across its source canvas, and
    # Trooper.UpdateRefPoints derives tCenter from the same flip offsets, so a unit
    # whose body sits off-center in the canvas teleports sideways by 2x the offset --
    # and oscillates bDir -- every time it turns. One constant dx for ALL frames (so
    # in-animation motion like lunges is preserved), anchored on the named action's
    # first frame.
    flip_x = bool(cfg.get("flipX", False))

    dx = 0
    ref_name = cfg.get("recenter")
    if ref_name:
        ref_act = next((a for a in cfg["actions"] if a["name"] == ref_name), None)
        if ref_act is None:
            raise SystemExit(f"recenter action '{ref_name}' not in actions list")
        ref_src = ref_act["src"]
        ref_frames = (load_frames_from_gif(gifs[ref_src]) if ref_src in gifs
                      else load_frames_from_folder(os.path.join(source, ref_src)))
        ref_fit = fit_to_canvas(ref_frames[0], canvas)
        if flip_x:
            ref_fit = ref_fit.transpose(Image.FLIP_LEFT_RIGHT)
        bbox = ref_fit.getbbox()
        if bbox is None:
            raise SystemExit(f"recenter action '{ref_name}' first frame is empty")
        dx = round(canvas[0] / 2 - (bbox[0] + bbox[2]) / 2)
        print(f"recenter: shifting all frames by dx={dx} (anchor '{ref_name}')")

    # ---- gather frames per output action, in taxonomy order ----
    entries = []            # one dict per packed frame
    taxonomy = {}           # MainType -> SubType -> [ {sAction, iIncrement} ]
    seen_names = set()

    for act in actions:
        out_name, src = act["name"], act.get("src")
        if out_name in seen_names:
            raise SystemExit(f"duplicate output action name: {out_name}")
        seen_names.add(out_name)

        if "frames" in act:                 # grid mode: rows were sliced up front
            frames = act["frames"]
        elif src in gifs:
            frames = load_frames_from_gif(gifs[src])
        else:
            folder = os.path.join(source, src)
            if not os.path.isdir(folder):
                raise SystemExit(f"missing source folder for '{out_name}': {folder}")
            frames = load_frames_from_folder(folder)
        if not frames:
            raise SystemExit(f"no frames for action '{out_name}' from '{src}'")
        if len(frames) > 100:
            raise SystemExit(f"action '{out_name}' has {len(frames)} frames; 2-digit numbering caps at 100")

        for i, fr in enumerate(frames):
            fr = fit_to_canvas(fr, canvas)
            if flip_x:
                fr = fr.transpose(Image.FLIP_LEFT_RIGHT)
            fr = shift_x(fr, dx)
            cropped, tx, ty, tw, th = trim(fr)
            entries.append({
                "filename": f"{out_name}{i:02d}.png",
                "img": cropped,
                "sss": (tx, ty, tw, th),
                "trimmed": (tw, th) != canvas,
            })

        taxonomy.setdefault(act["main"], {}).setdefault(act["sub"], []).append(
            {"sAction": out_name, "iIncrement": act.get("increment", 100)})

    # ---- pack ----
    sheet_w, sheet_h = shelf_pack(entries)
    sheet = Image.new("RGBA", (sheet_w, sheet_h), (0, 0, 0, 0))
    for e in entries:
        sheet.paste(e["img"], (e["x"], e["y"]))

    # ---- emit the hybrid JSON the game loader parses ----
    action_types = [
        {"MainType": main_t,
         "SubTypes": [{"Type": sub_t, "Actions": acts} for sub_t, acts in subs.items()]}
        for main_t, subs in taxonomy.items()
    ]
    frames_json = [
        {"filename": e["filename"],
         "frame": {"x": e["x"], "y": e["y"], "w": e["img"].width, "h": e["img"].height},
         "rotated": False,
         "trimmed": e["trimmed"],
         "spriteSourceSize": {"x": e["sss"][0], "y": e["sss"][1], "w": e["sss"][2], "h": e["sss"][3]},
         "sourceSize": {"w": canvas[0], "h": canvas[1]}}
        for e in entries
    ]
    doc = {
        "ActionTypes": action_types,
        "frames": frames_json,
        "meta": {"app": "MightyFights sprite_stitcher", "version": "1.0",
                 "image": f"{name}.png", "size": {"w": sheet_w, "h": sheet_h}, "scale": "1"},
    }

    png_path = os.path.join(args.out, f"{name}.png")
    json_path = os.path.join(args.out, f"{name}Array.json")
    sheet.save(png_path)
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(doc, f, indent=1)

    # ---- report + trooper-compat check ----
    required = ["idle", "low", "stab", "stick", "chop", "chopb", "bigchop", "lunge",
                "parry", "stance", "ready", "pant", "victory", "death", "deathb",
                "walk", "run", "flee"]
    missing = [r for r in required if r not in seen_names]
    print(f"{name}: {len(seen_names)} actions, {len(entries)} frames, sheet {sheet_w}x{sheet_h}")
    print(f"wrote {png_path}\nwrote {json_path}")
    if missing:
        print(f"WARNING: trooper AI requests these by name and they are MISSING: {missing}")
    else:
        print("trooper-compat: all 18 explicitly-requested action names present")

    # flip-jump check: bDir flips mirror the frame across the canvas AND move tCenter
    # by the same amount (Trooper.UpdateRefPoints), so movement frames with an
    # off-center body make the unit teleport sideways and flip-flop bDir every frame
    # it walks near a destination (this is exactly what broke the first Bandit stitch).
    move_jump = max((abs(canvas[0] - 2 * (e["sss"][0] + e["sss"][2] / 2))
                     for e in entries if e["filename"][:-6] in ("walk", "run", "flee")),
                    default=0)
    print(f"flip-jump on movement frames: {move_jump:.0f}px (sprite/tCenter shift when bDir flips)")
    if move_jump > 12:
        print("WARNING: body is off-center in the canvas -> turning will teleport/oscillate;"
              " add \"recenter\" to the config")


if __name__ == "__main__":
    main()
