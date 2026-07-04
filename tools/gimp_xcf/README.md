# GIMP .xcf inventory scanner

Non-destructive inventory scan of a folder of `.xcf` files: canvas size, top-level layer
names, and a flattened PNG thumbnail per file. Built to classify recovered/found art by what's
actually inside it instead of guessing from filenames — see `docs/DESIGN_DIRECTION.md` → "Art
pipeline". Also the tool to point at any new `.xcf` drops from the original artist (creatures,
minions, whatever comes next) so classification is a rerun, not a re-investigation.

## Usage

```bash
bash tools/gimp_xcf/scan_xcf_inventory.sh "<source_dir>" "<out_dir>" [gimp_console_path] [max_thumb_dim] [timeout_s]
```

Example:
```bash
bash tools/gimp_xcf/scan_xcf_inventory.sh "C:/dev/art assets raw/wodyn" "C:/scratch/xcf_scan"
```

Produces `<out_dir>/report.txt` (canvas size + layer names per file) and `<out_dir>/thumbs/*.png`
(one flattened thumbnail per source file — check these visually; see "why no group recursion"
below for why a picture beats a layer-name list here).

Run from **Bash/MSYS, not PowerShell** — see the note in `scan_xcf_inventory.sh`'s header:
`gimp-console` reliably runs to completion from a bash shell, but reproducibly hung after
finishing its work (confirmed via step markers: execution reaches `gimp-quit`, then the
`script-fu.exe` helper subprocess logs a `gimp_wire_read(): error` and the whole process never
exits) when launched via PowerShell's `Start-Process` *or* the native `&` call operator, with or
without output redirection. Specific to GIMP's plugin-wire shutdown under a real Win32 console
host, not a bug in this script. If you need this from PowerShell for some reason, shell out to
bash rather than re-fighting this.

## Extracting animation frames from a scanned file

Once `scan_xcf_inventory.sh` has told you a file has real per-frame layers (numbered/named
layers of *varying* content — check the thumbnail and layer list in `report.txt`; most of the
recovered `.xcf` set turned out to be single-pose concept art with 1-5 composition layers, not
extractable animation, so check before assuming), pull the frames out individually:

```bash
export SCAN_LIB="$(pwd)/tools/gimp_xcf/export_layers.scm"
export SCAN_PATH="C:/dev/art assets raw/wodyn/bandit.xcf"
export SCAN_FILENAME="bandit.xcf"
export SCAN_THUMB_DIR="C:/scratch/bandit_frames"   # reused as the output dir, despite the name
export SCAN_PREFIX=""                               # optional filename prefix, e.g. "bandit_"
timeout 90 "/c/Users/$USERNAME/AppData/Local/Programs/GIMP 2/bin/gimp-console-2.10.exe" \
  -i -d -f -b "(load \"$(pwd)/tools/gimp_xcf/export_driver.scm\")"
```

Writes one PNG per top-level layer, named `<prefix><layer-name>.png`, cropped to that layer's
own bounds. Feed the real per-frame ones (skip base/background/reference layers) into
`tools/sprite_stitcher/stitch.py` the same way Peasant's `pea_*` source frames were used.
Confirmed working on `bandit.xcf` (11 real frames: idle → wind-up → attack lunge → idle) and
`sword_hero.xcf` (6 real frames, a small attack swing).

**Two things this got wrong before it got right, in case you're editing it:**
- `gimp-image-flatten` flattens *every visible layer in whatever image it's called on*. The
  first version duplicated the target layer into the *source* image (which still had all its
  other layers) and flattened that — every "isolated" frame came out byte-identical, a full
  composite of everything, not the one layer. Fixed by copying the layer into a brand-new,
  otherwise-empty image via `gimp-layer-new-from-drawable` before flattening.
- A copied layer inherits the source layer's visibility. Many of these frame layers are saved
  *hidden* (the artist toggles one visible at a time while drawing), so the fresh image had no
  visible layer and `gimp-image-flatten` errored outright. Fixed with an explicit
  `gimp-item-set-visible new-layer TRUE` after inserting the copy.

## Files

- `scan_layers.scm` — the Script-Fu library (`scan-one`). Deliberately does **not** recurse into
  layer groups: `gimp-item-get-children` crashes the script-fu plugin on this GIMP 2.10.38
  Windows build for real legacy-format `.xcf` group layers (reproducible; a synthetic
  freshly-built group layer does not crash, so this is specific to how old `.xcf` files
  serialize groups, not a general API bug). The thumbnail substitutes for recursion — a
  flattened composite shows what a group contains without needing to enumerate it.
- `scan_driver.scm` — thin driver invoked via `gimp-console -b '(load "scan_driver.scm")'`.
  Reads its parameters from environment variables (`SCAN_LIB`, `SCAN_PATH`, `SCAN_FILENAME`,
  `SCAN_THUMB_DIR`, `SCAN_MAX_DIM`) rather than the `-b` argument string, set by the shell
  driver per file.
- `scan_xcf_inventory.sh` — the batch driver. One `gimp-console` invocation per file (a single
  malformed/huge file can't take down the whole batch), each capped by a `timeout`. Enumerates
  via `find -print0` / `read -d ''` (NUL-delimited), which is the only fully space/special-char
  -safe way to loop over filenames in bash — a naive `for f in $(ls *.xcf)` word-splits on
  spaces and silently mangles names like `"camp sketch.xcf"`.
- `export_layers.scm` / `export_driver.scm` — per-file frame extraction, see above. No batch
  shell wrapper yet (used by hand, one file at a time, so far); write one the same way as
  `scan_xcf_inventory.sh` if extracting frames from several files becomes routine.

## GIMP install

Requires GIMP 2.10 (not 3.x — this predates it and hasn't been tested there) with the
`gimp-console-2.10.exe` binary, e.g. via `winget install --id GIMP.GIMP.2`. Default lookup path
is `%LOCALAPPDATA%/Programs/GIMP 2/bin/gimp-console-2.10.exe`; pass a different path as the 3rd
script argument if yours differs.
