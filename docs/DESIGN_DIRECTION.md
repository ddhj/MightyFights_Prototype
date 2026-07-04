# DESIGN_DIRECTION.md

Written 2026-07-03, at the transition from "port MightyFights" to "build on MightyFights."
Captures (a) the owner's design decisions for where this goes next, and (b) the reconstructed
design archaeology from the original prototype, so neither lives in 13-year-old memory again.
Port status/history lives in `PORT_NOTES.md`; this doc is about *what to build*.

## Owner decisions (2026-07-03)

- **Mechanics are validated and frozen as a foundation.** V6.2–V6.6 equivalents were validated by
  owner playtest. Combat sim, templates, buffs, exp tally: keep as-is, build on top.
- **Camp & Map are artifacts of an abandoned earlier design.** The strategic-overworld campaign
  (Title → Map → Camp/Battle) is no longer the plan. **Salvage:** the level-up mechanic and the
  Camp (as home for the salvaged template/level-up loop). The rest (Map overworld, Smith,
  Captains/Dialogue) is additive-maybe-later, not on the path.
- **New direction: technical demos on the combat core, selected from a mode menu.** First demo:
  **Kaiju Hunt** — player spawns troopers freely to bring down one huge spawned monster; survivors
  level up through the existing template/company/buff mechanics.

## Mode select

Title "Play Game" now leads to a ModeSelect scene instead of directly to Camp:
- **Skirmish** — the existing Camp → Battleground loop (unchanged; Camp remains reachable because
  it hosts the salvageable Template editor).
- **Kaiju Hunt** — the new demo mode (scene scaffolded; combat build-out is the active task).
- **Back** — return to title.
Rendered with the spritefont; no new art dependencies.

## Kaiju Hunt — design sketch

Player-facing loop: spawn troopers (as many as you want; maybe costed later) → they swarm the
kaiju using the existing trooper AI → kaiju AoE-attacks back → kill it or lose the field →
survivors bank `ExperienceData` → level-up via templates in Camp.

Architecture mapping (from code review, see PORT_NOTES Phase 6 archaeology):
- Kaiju = third `Combatant` archetype (precedent: `Trooper`, `Healer`/Priest). Boss behavior fits
  the existing `ActionManager<T>` + `DActionHeuristic` queued-heuristic pattern well.
- **Keep the kaiju OUT of the zone dictionaries** (zones are man-sized). Troopers get a targeting
  override: kaiju is targetable when adjacent to its bounds. Avoids reworking the zone core.
- Mid-battle player spawning has precedent: buffs are already player-applied during battle via
  `BObjectManager`'s click path.
- `ExperienceTally` + per-template `cExpData` already count kills/crits/etc. The **exp → level →
  stat-growth formula was never designed** (the old WinForms StatsDialog existed to tune it).
  **Decided 2026-07-04 (owner): Fibonacci thresholds** — the classic early-RPG curve — implemented
  in `Support Classes/LevelCurve.cs` (cumulative 1,1,2,3,5,8,…×100 XP; XP weighted kills-first;
  +6%/+8%/+4% power/hp/AC per level at spawn). **Templates level, not instances** (owner confirmed
  the original design). Survivors bank exp into the persisted roster at hunt victory; SaveData runs
  immediately.
- **Owner direction (2026-07-04):** as kaiju difficulty tiers rise, the kaiju should **spawn
  minions from the non-human sprite set** (skeleton, slug, wild dog, bandit, drau — all .xcf-locked,
  pending the scripted GIMP export). The leveling mechanic must also **stitch into the Camp menus**
  (level display in the Template editor, content gating by roster level).
- Loader prerequisite: generalize the hardcoded 100px `sourceSize` assumption in
  `AnimationDataLoader` (kaiju sprites won't be 100px wide).
- **Decided 2026-07-04 (owner): attack-slot count is a "rated strength" knob, not a pure
  visual-scale one.** `Trooper._iAvailablePositions` (how many attackers a target can
  meaningfully hold at once) is a designed, per-target gameplay decision independent of
  `_fScale` (pure render/geometry size) -- because once minions exist, troopers need to be able
  to peel off the kaiju to fight them, and each minion will want its own appropriately-sized cap
  rather than everything being driven by body size alone. The underlying attack-slot geometry
  (`Trooper_Combatant.cs`) was rewritten from a fixed 6-slot Left/Right x Top/Mid/Bottom enum to
  a dynamic radial system: however many slots `_iAvailablePositions` says, that many rendezvous
  points get generated evenly around the target. (The fixed 6-slot system is also *why* troopers
  were "attacking but missing" the kaiju -- once Top/Bottom filled, every further attacker piled
  onto the identical Mid point and couldn't all physically stand there.) See PORT_NOTES-equivalent
  commit history for the full mechanics; this doc just records the design call.

## Art pipeline (for new units and the kaiju)

Reconstructed original toolchain (evidence in PORT_NOTES Phase 6 / the JSONs themselves):
- Sheets packed with **TexturePacker** (codeandweb.com; `meta.app` fingerprint in every JSON;
  owner likely held a license). Frames named `<action><NN>.png` (exactly 2-digit) — the filename
  prefix is the join key to the action taxonomy.
- A **lost custom utility** merged in the `ActionTypes` taxonomy (MainType/SubType/action +
  per-action `iIncrement` timing) and sparse per-frame `KeyFrame` event markers ({Type, oData},
  consumed by `TrooperActMgr.ProcessKeyFrame`). The utility is gone; the **schema is fully
  specified** by `AnimationDataLoader`'s DTOs, with `HalberdArray.json` and `ChaplainArray.json`
  as reference outputs. Rebuilding it as a small stitcher script/tool is bounded work.
- Compatibility bar for any new/found art: the Trooper AI requests exactly these pairs —
  `Attack:Basic`, `Attack:Critical`, `Defend:Parry`, `Idle:Normal/Battle/Pant/Victory`,
  `Death:Normal`, `Move:Walk/Run/Flee` (Healer: `Idle:Normal`, `Heal:Basic`). Most free packs ship
  ~6 animations, so add an **AnimationProcessor fallback chain** (Pant→Idle, Flee→Run,
  Critical→Basic, Parry→skip) to widen usable art dramatically.
- Sourcing: Universal LPC Spritesheet Character Generator (unit variety, thrust anim, CC-BY-SA/GPL),
  itch.io CC0 packs, OpenGameArt; owner's separate artwork repo may hold original style-matched
  units. AI generation: strongest for the kaiju (large canvas, 4–6 actions, single instance);
  humanoid frame-sets need a human-in-the-loop cleanup pass; the JSON side is fully automatable.
- **2026-07-04: Peasant is live** — the stitcher's first unit shipped end-to-end. Content at
  `content/Sprite Data/Troopers/Peasant/` (`Peasant.png` MGCB-built + `PeasantArray.json` raw,
  mirroring Chaplain's single-texture layout since there's only one skin so far — Peasant is a
  combat `Trooper` like Halberd, not a `Priest`/healer; the layout choice was file-organization
  only). Wired into the Kaiju Hunt wheel roster (cost 1, cheap fodder, appended after Captain so
  index 0/the initial squad stays Halberdier). Verified in a live battle: renders correctly
  (visually distinct from the Halberdiers), fights the kaiju, no exceptions — confirms
  `AnimationDataLoader` parses stitcher output through the real game path, not just the
  stitcher's own self-check. The **AnimationProcessor fallback chain is still unbuilt** — Peasant
  didn't need it (its config carries all 18 action names), but the non-human minion sets (task
  #16/#17) likely have sparser coverage and will need it.
- 2026-07-04: full .xcf archaeology scan (tools/gimp_xcf), 63/63 files, 0 failures. Report +
  thumbnails classify every recovered file by actual contents. Findings:
  - Griggan_units.xcf and units.xcf/master_sheet.xcf reveal a much bigger planned roster than
    ever shipped -- full enemy-army concept sheets (dark-armored soldiers with swords/pikes,
    likely a named "Griggan" faction), multiple knight factions with banners (gold/blue/teal),
    skeleton warriors, ape/troll creatures, at least one more dragon design, and dozens more
    humanoid unit variants. Dense reference/moodboard compilations (units.xcf even appears to
    contain a screenshot of a palette-swap tool UI), not extractable single assets -- but they
    conclusively answer the "more than pikemen" question with a yes.
  - quartermaster.xcf -- a bearded NPC vendor behind a supply cart plus two spear-soldiers.
    Reads as a Camp shop NPC concept, distinct from (and maybe overlapping with) the never-
    implemented Smith clickable.
  - trainer.xcf -- an actual UI mockup for a weapon/ability progression system: "Attack
    Combinations," an "Upgrade Weapon Phase 2" wheel, "Rate of Progress," "Main Training," a
    named character ("Brave Bill"), and icon-grid skill trees. A planned progression system
    distinct from the simple stat-leveling (LevelCurve.cs) built this session -- worth a look
    if/when the Camp-menu leveling task (#18) wants richer presentation.
  - sword_hero.xcf -- a lone orange-armored hero character, likely a protagonist/avatar design
    that was never wired into the game.
  - Kaiju-art reality check: dragon.xcf is a 2-pose concept sketch (3 layers: medium/big/
    Background), not an animation set. Same for skeleton.xcf/wild_dog.xcf/slug.xcf/drau.xcf
    (2-7 composition layers each, single illustrations). Strong style references for a real
    kaiju/minion redesign, but NOT mechanically extractable -- real animated sprites for any of
    these need new art (hand-drawn to match, or AI-assisted from these references; see the
    art-pipeline AI-generation note above), not a batch-export job.
  - bandit.xcf (11 numbered frames, confirmed idle -> wind-up -> attack-lunge -> idle) and
    sword_hero.xcf (6 numbered frames, a small attack swing) are genuinely extractable,
    hand-animated units -- tools/gimp_xcf/export_layers.scm pulls each layer out as its own
    correctly-isolated PNG (see that tool's README for two real bugs hit and fixed: naively
    flattening a duplicated-in-place layer re-composites everything underneath it, and copied
    layers inherit the source's hidden visibility state). Not yet run through the sprite
    stitcher into a playable unit -- that needs someone to look at all 11/6 frames and map them
    onto the trooper AI's action names, the same way peasant.config.json did for Peasant.

## Reconstructed original design (archaeology record)

- Scene graph as-built: Title → Camp → Battleground (right-click at Victory exits; left-click
  rematches — it was a balance-testing loop, no victory screen ever existed).
- `Map` was the intended strategic overworld: 11 declared locations (Dungeon, Citadel, Darvi,
  Tower1/2, BridgeNE/S/SE, Chapel, Hamlet, Hut) — never instantiated, blocked on art ("these will
  all have to be sprites... when we get the sprite data"), no input wiring. Now deprecated by the
  decisions above.
- KnightMenu was the army-management hub: Captain (stub), Company (WinForms editor, stripped in
  port), Dialogue (mis-wired to the Captain stub), **Template (works — the troop editor)**.
- Smith clickable: hit-test wired, action body empty — never implemented.
- Two animated unit types exist: **Halberd** trooper (21 actions, 5 color variants) and
  **Chaplain** healer. "More unit types" beyond that were portfolio-only, never imported.
- Pikard clickable = debug battle-starter (force-sets both armies to 100 pikemen; commented lines
  show 3 template slots were intended).
