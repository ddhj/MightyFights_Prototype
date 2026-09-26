# GIMP bridge: paint a unit in GIMP, put it back in the game

Modelled on SettlementARPG's `tools/gimp_bridge` (the paperdoll bridge), cut down to what a
MightyFights unit is: **one sheet + one AnimationData JSON** (`content/Sprite Data/.../<Unit>/<Unit>.png`
+ `<Unit>Array.json`), side-view, art facing left (the game mirrors it for `bDir`).

## For the artist

**The full field guide is `ARTIST_GUIDE.html`**: one self-contained page covering how the game plays a
unit, the grid, every animation and when it plays, timing, adding frames, the midline, hit frames, new
units, and what each check message means. `python tools/gimp_bridge/make_guide.py` rebuilds it. The
pictures come from the game's own sheets through the bridge's own exporter (`guide/guide_art.py`) and
the prose lives in `guide/guide.src.html`. `build_bridge.py` rebuilds it on every build and ships it in
the package next to `README.txt`. The short version:

- **One GIMP file per unit.** Every **row** is an animation (idle, low, stab, ... walk, run, flee),
  every **column** a frame, and each cell is the unit's frame size. Magenta labels give the animation,
  its type and ms per frame.
- **Add frames** by painting into the empty columns to the right of a row. **Remove frames** by erasing
  a row's last cells.
- **Keep the body on the dotted blue midline.** The game mirrors each frame across it when the unit
  turns, so an off-centre body jumps sideways (the Bandit flip bug, `docs/AI_ART_PIPELINE.md`).
- **`#hit frames` layer:** a red dot in a cell = the attack lands on that frame (a `Collision`
  keyframe). An attack row with no dot never deals damage.
- **New units:** menu 2 / `new` writes a blank grid with the rows and timings of an existing unit and
  that unit faintly behind it for scale.

## For the developer

```
python tools/gimp_bridge/export_unit.py --list
python tools/gimp_bridge/export_unit.py --unit Bandit [--xcf] [--spare 6]      # -> tools/gimp_bridge/work/
python tools/gimp_bridge/export_unit.py --new Goblin --like Peasant [--canvas 140x96]
python tools/gimp_bridge/import_unit.py returned/ --dry-run                     # what changed, row by row
python tools/gimp_bridge/import_unit.py returned/                               # -> tools/gimp_bridge/out/
python tools/gimp_bridge/import_unit.py returned/ --write-content               # into content/Sprite Data
python tools/gimp_bridge/build_bridge.py --out <dir>                            # the package + its proof
```

- **The template is the unit's current sheet.** Untouched frames keep their exact crop, and changed
  or new frames are trimmed with `stitch.py`'s own `trim()`. The whole sheet is repacked with
  `stitch.py`'s `shelf_pack()`. `ActionTypes`, `iIncrement` timings and non-hit keyframes (`SelfHeal`)
  are copied, never re-derived. **An unchanged import of a stitched unit reproduces its JSON and PNG
  byte for byte.** The old TexturePacker sheets (Chaplain) come back pixel-equal with the same
  frames/keyframes, but repacked unrotated.
- **Frame counts.** A row ends at its last non-empty cell. Trailing frames that were already blank in
  the sheet and still are (Drau) are kept, not dropped.
- **Hit frames** come from the `#hit` layer. With no `#hit` layer at all, the sheet's existing hit
  frames are kept, with a warning.
- **What it refuses:** a resized canvas, an empty row, a hit mark past a row's last frame, a hit mark on
  a frame that already has another keyframe, two files for one unit, a new unit whose name exists, and
  a **stale export** (the `#guides` hash doesn't match the current sheet, so untouched frames would
  revert newer art). `--force` accepts a stale export.
- **After `--write-content`:** `dotnet build MightyFights.sln` so MGCB rebuilds the sheet. A **new**
  unit also needs its `Content.mgcb` rows (the import prints them) and code that spawns it.
- **Not supported: the Halberd.** `HalberdArray.json` is one layout shared by 14 palette-swapped
  textures under `Textures/`, so a repack would have to re-lay-out all 14. `find_units()` skips it (and
  the taxonomy-less `Front Facing` sheet).
- `.xcf` goes through `gimp-console` batch mode (Python-Fu), launched from Python with stdin closed.
  `.ora` needs Pillow alone. See SettlementARPG's bridge README for why ORA + GIMP batch rather than
  parsing `.xcf`.

### Packaging: `UnitBridge.exe`

Every `dotnet publish` of `MightyFights.Desktop` (the `win-x64` profile included) runs the
`PublishUnitBridge` target. It calls `build_bridge.py`, which writes `UnitBridge/` INSIDE the game's
publish folder (`publish/win-x64/UnitBridge/`), so zipping the publish folder sends the game and the tool together: a frozen `unit_bridge.py` (one exe,
subcommands `list` / `export` / `new` / `import`, a menu on double-click), every unit sheet, a
`README.txt`, and empty `work/` and `out/`. The build **proves the frozen exe** before it succeeds:

- every unit round-trips unchanged
- one painted pixel changes exactly one frame
- a spare-column paint adds exactly one frame
- a hit dot becomes exactly one `Collision` keyframe
- a new unit comes out complete
- writes outside `out/` are refused
- the `.xcf` path round-trips through GIMP (when GIMP is installed)

It needs Python + Pillow + numpy + PyInstaller on the publishing machine. Pass `-p:BuildUnitBridge=false`
to skip it. In the package, import writes to `out\` beside the exe and nowhere else.
