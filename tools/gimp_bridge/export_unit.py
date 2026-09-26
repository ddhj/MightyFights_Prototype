#!/usr/bin/env python3
"""Export a MightyFights unit as ONE layered GIMP file, or start a brand-new unit.

ONE FILE = ONE UNIT. The canvas is a grid: one ROW per action (idle, low, stab, ... in the order
the sheet stores them), one COLUMN per frame, each cell the unit's canvas (Peasant 100x64, Bandit
100x70, ...). Every row gets SPARE empty columns on the right: painting into them ADDS frames to
that action; erasing a row's last frames REMOVES them.

Layers, bottom to top:
    "#reference <Unit> (...)"    only for `--new`: a faint existing unit, for scale and baseline.
    "art: <Unit>"                the unit's pixels. Paint here (or on any new layer of your own --
                                 every visible layer whose name doesn't start with '#' is merged).
    "#hit frames (...)"          a red dot marks the frame an attack LANDS on (the game's Collision
                                 keyframe). Move the dot to move the hit. An attack row with no dot
                                 deals no damage.
    "#guides unit=... (...)"     cell borders, the canvas midline, row labels. Ignored, but it
                                 carries what the file is, so a renamed file still imports.

Examples:
    python tools/gimp_bridge/export_unit.py --list
    python tools/gimp_bridge/export_unit.py --unit Bandit
    python tools/gimp_bridge/export_unit.py --unit Peasant --xcf --spare 6
    python tools/gimp_bridge/export_unit.py --new Goblin --like Peasant
    python tools/gimp_bridge/export_unit.py --new Ogre --like Peasant --canvas 140x96
"""

import argparse
import os
import re
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unit_sheet as us  # noqa: E402

DEFAULT_OUT = os.path.join(us.HERE, "work")
DEFAULT_SPARE = 4
NEW_FRAMES = 8               # empty columns per row for a new unit


def guides(size, cell, rows):
    """rows = [(label, frame count)]"""
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cw, ch = cell
    w, h = size
    for x in range(0, w + 1, cw):
        d.line([(min(x, w - 1), 0), (min(x, w - 1), h - 1)], fill=(255, 0, 255, 90))
    for y in range(0, h + 1, ch):
        d.line([(0, min(y, h - 1)), (w - 1, min(y, h - 1))], fill=(255, 0, 255, 90))
    for r, (label, n) in enumerate(rows):
        for c in range(w // cw):
            #### the canvas midline: the game mirrors each frame across it when the unit turns, so a
            #### body that is not centred on it jumps sideways every turn (docs/AI_ART_PIPELINE.md)
            mx = c * cw + cw // 2
            for y in range(r * ch + 2, (r + 1) * ch - 2, 4):
                d.point((mx, y), fill=(0, 200, 255, 110) if c < n else (0, 200, 255, 50))
            d.text((c * cw + cw - 15, r * ch + ch - 11), f"{c:02d}",
                   fill=(255, 0, 255, 150) if c < n else (255, 0, 255, 60))
        d.text((3, r * ch + 2), label, fill=(255, 0, 255, 200))
    return img


def hit_marks(size, cell, hits):
    """hits = {(row, col)}: a dot in the cell's top-right corner."""
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cw, ch = cell
    for r, c in hits:
        x, y = c * cw + cw - 10, r * ch + 4
        d.ellipse([x, y, x + 6, y + 6], fill=us.HIT_COLOUR)
    return img


def export_unit(sheet, spare, stem, out, xcf):
    cw, ch = sheet.cell
    cols = max(len(ix) for _, ix in sheet.actions) + spare
    cols = min(cols, us.MAX_FRAMES)
    w, h = cols * cw, len(sheet.actions) * ch
    art = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    hits, labels = set(), []
    for r, (act, ix) in enumerate(sheet.actions):
        for c, i in enumerate(ix):
            art.paste(sheet.cell_image(i), (c * cw, r * ch))
        main, sub, inc = sheet.taxonomy[act]
        extra = []
        for k, kf in sheet.keyframes(act).items():
            if kf.get("Type") == us.HIT_KEY:
                hits.add((r, k))
            else:
                extra.append(f"{kf.get('Type')}@{k:02d}")
        labels.append((f"{act}  {main}/{sub}  {inc}ms" + (("  " + " ".join(extra)) if extra else ""), len(ix)))
    stack = [(f"art: {sheet.name}", art, True, 1.0),
             ("#hit frames (paint a dot in a cell = the attack lands on that frame)",
              hit_marks((w, h), sheet.cell, hits), True, 1.0),
             (us.guides_name(unit=sheet.name, hash=us.fingerprint(sheet.name), cols=cols),
              guides((w, h), sheet.cell, labels), True, 1.0)]
    return write(stem, out, (w, h), stack, art, xcf)


def export_new(name, like, canvas, frames, out, xcf):
    """A blank grid for a new unit, with the rows (and their taxonomy/timings) of `like`."""
    lw, lh = like.cell
    cw, ch = canvas or like.cell
    if cw < lw or ch < lh:
        #### the reference is pasted bottom-centre, where stitch.py anchors a smaller frame
        raise SystemExit(f"--canvas {cw}x{ch} is smaller than {like.name}'s {lw}x{lh}; the reference would not fit")
    cols = min(max(frames, max(len(ix) for _, ix in like.actions)), us.MAX_FRAMES)
    w, h = cols * cw, len(like.actions) * ch
    ref = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    labels = []
    ox, oy = (cw - lw) // 2, ch - lh
    for r, (act, ix) in enumerate(like.actions):
        for c, i in enumerate(ix):
            ref.paste(like.cell_image(i), (c * cw + ox, r * ch + oy))
        main, sub, inc = like.taxonomy[act]
        labels.append((f"{act}  {main}/{sub}  {inc}ms", cols))
    blank = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    stack = [(f"#reference {like.name} (for scale only, ignored on import)", ref, True, 0.30),
             (f"art: {name}", blank, True, 1.0),
             ("#hit frames (paint a dot in a cell = the attack lands on that frame)", blank.copy(), True, 1.0),
             (us.guides_name(unit=name, new=1, like=like.name, canvas=f"{cw}x{ch}", cols=cols),
              guides((w, h), (cw, ch), labels), True, 1.0)]
    return write(name, out, (w, h), stack, ref, xcf)


def write(stem, out, size, stack, merged, xcf):
    os.makedirs(out, exist_ok=True)
    path = os.path.join(out, f"{stem}.ora")
    us.write_ora(path, size, stack, merged)
    print(f"wrote {path}  ({size[0]}x{size[1]})")
    if xcf:
        x = os.path.splitext(path)[0] + ".xcf"
        us.gimp_convert(os.path.abspath(path), os.path.abspath(x))
        print(f"wrote {x}")
    return path


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--unit", help="an existing unit, e.g. Bandit (see --list)")
    ap.add_argument("--new", metavar="NAME", help="start a NEW unit called NAME")
    ap.add_argument("--like", default="Peasant",
                    help="with --new: the unit whose actions, timings and size the new one copies (default Peasant)")
    ap.add_argument("--canvas", help="with --new: WxH of one frame (default: the --like unit's)")
    ap.add_argument("--frames", type=int, default=NEW_FRAMES,
                    help=f"with --new: empty columns per row (default {NEW_FRAMES})")
    ap.add_argument("--spare", type=int, default=DEFAULT_SPARE,
                    help=f"empty columns after each row's last frame, for adding frames (default {DEFAULT_SPARE})")
    ap.add_argument("--out", default=DEFAULT_OUT, help=f"output folder (default {DEFAULT_OUT})")
    ap.add_argument("--root", default=us.SPRITE_DATA, help="where the units' sheets live")
    ap.add_argument("--xcf", action="store_true", help="also save a native .xcf beside the .ora (needs GIMP)")
    ap.add_argument("--list", action="store_true", help="list the units, then stop")
    args = ap.parse_args()

    if args.list:
        for name, rel in us.find_units(args.root).items():
            s = us.UnitSheet(name, args.root)
            hits = sum(1 for fr in s.frames if fr.get("KeyFrame", {}).get("Type") == us.HIT_KEY)
            print(f"  {name:<10} {s.cell[0]}x{s.cell[1]}  {len(s.actions):>2} actions  "
                  f"{len(s.frames):>3} frames  {hits:>2} hit frames   ({rel})")
        return

    if bool(args.unit) == bool(args.new):
        raise SystemExit("give --unit NAME (edit an existing unit) or --new NAME (start one); --list shows the units")

    if args.unit:
        export_unit(us.UnitSheet(args.unit, args.root), max(0, args.spare), args.unit, args.out, args.xcf)
        return

    if not re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*", args.new):
        raise SystemExit(f"a unit name is letters, digits and _ (it becomes a file and a content path): '{args.new}'")
    if args.new in us.find_units(args.root):
        raise SystemExit(f"'{args.new}' already exists -- use --unit {args.new} to edit it")
    canvas = None
    if args.canvas:
        m = re.fullmatch(r"(\d+)x(\d+)", args.canvas.lower())
        if not m:
            raise SystemExit(f"--canvas wants WxH, e.g. 100x64; got '{args.canvas}'")
        canvas = (int(m.group(1)), int(m.group(2)))
    export_new(args.new, us.UnitSheet(args.like, args.root), canvas, max(1, args.frames), args.out, args.xcf)


if __name__ == "__main__":
    main()
