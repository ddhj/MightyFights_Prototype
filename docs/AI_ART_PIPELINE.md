# AI_ART_PIPELINE.md

Written 2026-07-05. Answers "can we use the Peasant/Halberd sprite format as a template and
generate new units with AI?" — the follow-through on DESIGN_DIRECTION.md's art-pipeline
AI-generation note. Companion to `tools/sprite_stitcher/` (which already automates the entire
JSON/packing side) and to the owner's artist friend, who can use the same contract below.

## The template contract (what ANY art source must deliver)

Everything downstream of raw frames is already automated by `stitch.py`. A new unit is "done"
when someone — human or model — delivers per-frame PNGs meeting this contract:

- **Layout:** a folder per source animation, frames as RGBA PNGs with transparent backgrounds,
  alphabetical order = playback order (the wodyn `bandit_pngs/` layout).
- **Canvas:** one fixed canvas for the whole unit (Halberd/Peasant: 100x64; per-unit sizes are
  fine — the loader reads `sourceSize` per file, SwordHero ships at 86x49). Frames smaller than
  the canvas are auto-anchored bottom-center by the stitcher.
- **Facing: LEFT.** The game's convention is art faces left; `bDir` flips it right at draw time.
- **Horizontally centered body.** The flip mirrors the frame across the canvas AND moves the
  unit's logical `tCenter` with it (`Trooper.UpdateRefPoints`), so an off-center body makes the
  unit teleport sideways and flip-flop direction every time it turns — this is exactly what was
  wrong with the first Bandit stitch (body at x≈21 of 100 → 48–58px teleport + `bDir`
  oscillation). `stitch.py` now has a `"recenter"` config option and warns when movement frames
  are off-center by more than 6px per side. Weapons may extend off-center (Halberd's attack
  poses reach 36px of flip-shift and always have); the *body* on walk/run/flee/idle is what
  must sit on the midline.
- **Feet on a consistent baseline** across all frames (the stitcher doesn't fix vertical drift).
- **Action coverage:** the trooper AI requests 18 names. Sparse sets are fine — alias in the
  config the way `bandit.config.json` maps 2 source animations onto all 19 slots. Reference
  frame counts from the real units (a model/artist can deliver fewer; timing is tunable via
  per-action `increment`):

  | action | Peasant | Halberd |   | action | Peasant | Halberd |
  |--------|--------:|--------:|---|--------|--------:|--------:|
  | idle   | 9       | 10      |   | parry  | 11      | 7       |
  | stance | 6       | 5       |   | victory| 8       | 8       |
  | ready  | 6       | 4       |   | death  | 13      | 14      |
  | pant   | 8       | 6       |   | deathb | 13      | 13      |
  | walk   | 6       | 6       |   | low    | 10      | 8       |
  | run    | 9       | 9       |   | stab   | 9       | 8       |
  | flee   | 7       | 8       |   | chop   | 17      | 11      |
  | lunge  | 14      | 13      |   | chopb  | 16      | 9       |
  | bigchop| 18      | 13      |   | stick  | 11      | 8       |
  | punch  | 11      | 10      |   |        |         |         |

## Three generation paths, cheapest first

1. **Palette swap / kit-bash (no AI, ship today).** The original dev's own approach — the
   Halberd has 15+ recolor sheets (`Textures/f*.png`) and `Utilities/` contains his Sprite Color
   Swapper + Color Map Creator WinForms tools. A ~40-line Python recolor (hue-remap on the
   existing Peasant/Bandit/Halberd sheets, since recoloring the packed PNG needs no re-stitch)
   gives visually-distinct minion tiers for free. Best effort/reward for filling the kaiju
   minion roster *now*.

2. **Pixel-art-native AI (the real "new unit" path).** Two purpose-built services do exactly
   "reference image → consistent character animation frames":
   - **PixelLab** (pixellab.ai) — skeleton-driven character animation (walk/run/attack/custom
     from pose templates), keeps character identity across actions, has a paid API and an
     Aseprite plugin. Best fit for humanoid units: one generated character → all 18 actions
     from pose skeletons.
   - **Retro Diffusion** (retrodiffusion.ai) — pixel-art diffusion with an animation model
     (sprite-sheet output), API available (also resellable via Runware; the open-source
     SpriteBrew project on GitHub is a working example of driving it end-to-end).
   Feed either the .xcf concept exports (skeleton, wild_dog, slug, drau, dragon — see
   DESIGN_DIRECTION archaeology) as style/reference images. Expect a human cleanup pass per
   DESIGN_DIRECTION; the stitcher's validators (canvas, centering, action coverage) gate the
   output mechanically.

3. **General image models (kaiju-scale one-offs).** gpt-image / Imagen / SD+ComfyUI img2img
   from the dragon.xcf concept. Frame-to-frame consistency is the weak point, which matters
   least for a huge single-instance boss with 4–6 actions at `fKaijuScale`. Still the
   recommended source for the kaiju per DESIGN_DIRECTION; not recommended for 100px humanoids.

## Recommended first experiment

Skeleton minion via PixelLab: export `skeleton.xcf` composition as the reference, generate
idle + walk + one attack + death (4 animations ≈ bandit-level sparseness), drop frames into
`C:/dev/art assets raw/wodyn/skeleton_pngs/<action>/`, copy `bandit.config.json` →
`skeleton.config.json` (keep `"recenter": "idle"`), stitch, wire a roster entry. Total new
tooling required: none. Needs an API key / small budget decision from the owner.

## TexturePacker note (owner has the new version installed)

Not needed for this pipeline: `stitch.py` already replaces both TexturePacker's packing *and*
the lost merge utility that injected the `ActionTypes` taxonomy — modern TexturePacker can only
emit the `frames` half, so using it would still require a merge step (and its rotated-frame
packing exercises the trickier `bRot` branches of the loader for no gain at these sheet sizes).
Where it *could* earn its keep later: repacking the legacy Halberd variants if they're ever
re-exported, or if sheet count grows enough that packing efficiency starts to matter.
