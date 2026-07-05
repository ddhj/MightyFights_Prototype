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

## First experiment: Skeleton via PixelLab — DONE 2026-07-05, findings

The skeleton minion was generated end-to-end on a free-trial key (11 generations total,
including 3 burned on learning the quirks below) and is stitched at
`content/Sprite Data/Troopers/Skeleton/` (sources kept at
`C:/dev/art assets raw/wodyn/skeleton_pngs/`, config `tools/sprite_stitcher/skeleton.config.json`).
A bonus find made this cheap: **`skeleton.xcf` already contains three finished game-scale
skeleton sprites** (bone / silver-armored / blue-sword variants, plus the artist's palette
swatches), so no base-character generation was needed — the blue-sword one, background-keyed
and 2x-upscaled to fill a 64x64 canvas, went straight in as `reference_image`.

Hard-won settings for `POST /v1/animate-with-text` (64x64 only, ~4 frames/call, 1 "generation"
billed per call, Bearer auth):
- **The reference must substantially fill the canvas.** A native-scale 21x26 character in a
  64x64 frame returned four 100%-empty frames — billed, no error. 2x nearest-neighbor upscale
  fixed it completely.
- **`image_guidance_scale` 10 (default 1.4) is the identity knob.** At default, the skeleton
  came back as a hooded human with a yellow sword; at 10 the skull/ribcage/blue-sword identity
  held across every subsequent call. Trade-off: at 10 the pose is also pinned — "collapse to
  the ground" death animations came back standing, even at guidance 4 with explicit prompts.
- Negative prompt earning its keep: `"human, skin, flesh, clothes, hood, robe, hair, sword
  trail, motion blur, glowing arc, energy wave"` (the model loves giant anime slash arcs;
  two idle frames still caught them and were dropped).
- What worked per action: walk (4 frames, clean cycle), stab attack (`"quick short sword stab
  forward, blade stays small"`), idle (2 of 4 frames usable). **Death had to be hand-built**:
  stagger frame from a rejected take + the idle body split from its ground shadow, rotated
  -45deg/-90deg onto the baseline — the retro tip-over corpse. Budget for this kind of
  assembly pass; pure-API animation coverage was ~3 of 4 actions.
- PixelLab centered the character well: the stitched unit needed dx=0 recentering and shows a
  1px movement flip-jump.

Wired into the Kaiju Hunt spawn wheel (cost 2, no cap) alongside Bandit/SwordHero in
`KaijuHunt.SetupBattle`, with undead-flavored placeholder stats (never heals, never flees,
armor 2) inline like the other stitcher units — promote to a CombatTuning slot if it sticks.

Post-playtest fixes (owner feedback, same day): PixelLab output faces EAST even with
`direction: "west"` — the stitcher gained a `"flipX"` config option (game art faces left);
assume every animate-with-text unit needs it. The hand-built death frames originally rotated
part of the ground shadow with the body — fixed with a per-pixel split (dark ground-shadow
pixels stay at the baseline, bright bone pixels rotate). Size note: 64x64 output reads big
in-game (owner: kaiju-sized); if smaller tiers are wanted, add a downscale step at stitch
time or drive per-template scale in-engine.

## Second unit: Griggan knight (from Griggan_units.xcf) — DONE 2026-07-05

Confirms the pipeline generalizes to the concept sheets. `Griggan_units.xcf` (400x300) is not
just a moodboard — its top rows are individually croppable ~20x49 dark-armored soldiers with
clean spacing; connected-component isolation on the background-keyed sheet cuts any of them
out (neighboring weapons stay separate components). One horned red-eyed pike soldier was
scaled to fill 64x64 (1.22x non-integer nearest — the model doesn't care) and animated with
the skeleton recipe. New failure mode: **the model hallucinated a giant white banner/pennant
off the pale pike tip in nearly every frame**, surviving explicit anti-flag negatives and a
re-roll (a "flag on a pike" prior, triggered by bright weapon-tip pixels). What shipped:
post-processed frames — flood-fill deletion of big near-white components (>55px) for
attack/idle, brightness-darkening to "dark iron" for walk (its blade was at least consistent),
idle built as clean-frame + 1px bob, death tip-over rotation (this art has no ground shadow,
so no shadow split needed). Practical rule learned: **give the model references with dark,
low-contrast weapons, or expect a cleanup pass on every bright prop.** 5 generations spent
(18 total on the trial key so far, no quota wall yet). Also croppable for future units:
drau.xcf (6 ape-brute variants), wild_dog.xcf (4 dogs), more Griggan variants including 1.5x
horned commanders; dragon.xcf is single-pose kaiju-scale and needs the
`/animate-with-skeleton` endpoint (up to 256px) instead of animate-with-text (64 only).

## Third unit: Drau brute (drau.xcf) — DONE 2026-07-05, end of trial session

Wheel entry "Drau" (cost 4): glass-cannon brute (HP 140, power 8, armor 0, never heals/flees).
Trial quota is 40 generations; **23 used**, 17 left for next session. New data point: the
**empty-frames failure is action-phrase-correlated, not just reference-size** — "idle,
standing hunched, breathing heavily" and both attack phrasings returned 4 fully-transparent
frames (billed) with the same reference that produced a good walk. Drau's idle = walk frame 0
(which is always the reference pose — useful trick), attack = the two lunging walk frames
bracketed by idle (both API attack takes failed). Queued for next session with remaining
quota: wild_dog quadruped test, Griggan commander (1.5x variant), dragon kaiju via
/animate-with-skeleton at 128-256px, or re-rolls of the weak actions above.

## TexturePacker note (owner has the new version installed)

Not needed for this pipeline: `stitch.py` already replaces both TexturePacker's packing *and*
the lost merge utility that injected the `ActionTypes` taxonomy — modern TexturePacker can only
emit the `frames` half, so using it would still require a merge step (and its rotated-frame
packing exercises the trickier `bRot` branches of the loader for no gain at these sheet sizes).
Where it *could* earn its keep later: repacking the legacy Halberd variants if they're ever
re-exported, or if sheet count grows enough that packing efficiency starts to matter.
