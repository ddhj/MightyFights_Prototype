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
    "actions": [
      { "main": "Attack", "sub": "Basic", "name": "chop", "src": "pea_1knife", "increment": 100 },
      ...
    ]
  }
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
    os.makedirs(args.out, exist_ok=True)

    # ---- optional horizontal recentering ----
    # The game's flip math mirrors a frame across its source canvas, and
    # Trooper.UpdateRefPoints derives tCenter from the same flip offsets, so a unit
    # whose body sits off-center in the canvas teleports sideways by 2x the offset --
    # and oscillates bDir -- every time it turns. One constant dx for ALL frames (so
    # in-animation motion like lunges is preserved), anchored on the named action's
    # first frame.
    dx = 0
    ref_name = cfg.get("recenter")
    if ref_name:
        ref_act = next((a for a in cfg["actions"] if a["name"] == ref_name), None)
        if ref_act is None:
            raise SystemExit(f"recenter action '{ref_name}' not in actions list")
        ref_src = ref_act["src"]
        ref_frames = (load_frames_from_gif(gifs[ref_src]) if ref_src in gifs
                      else load_frames_from_folder(os.path.join(source, ref_src)))
        bbox = fit_to_canvas(ref_frames[0], canvas).getbbox()
        if bbox is None:
            raise SystemExit(f"recenter action '{ref_name}' first frame is empty")
        dx = round(canvas[0] / 2 - (bbox[0] + bbox[2]) / 2)
        print(f"recenter: shifting all frames by dx={dx} (anchor '{ref_name}')")

    # ---- gather frames per output action, in taxonomy order ----
    entries = []            # one dict per packed frame
    taxonomy = {}           # MainType -> SubType -> [ {sAction, iIncrement} ]
    seen_names = set()

    for act in cfg["actions"]:
        out_name, src = act["name"], act["src"]
        if out_name in seen_names:
            raise SystemExit(f"duplicate output action name: {out_name}")
        seen_names.add(out_name)

        if src in gifs:
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
            fr = shift_x(fit_to_canvas(fr, canvas), dx)
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
