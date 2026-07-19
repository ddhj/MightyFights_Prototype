# RTS_MODE_SPEC.md — "Conquest" mode (working title)

**Design + build spec for a StarCraft-vein real-time strategy mode built on the MightyFights combat
engine.** Written 2026-07-18 for **the next session (Sonnet) to execute**. It is deliberately
phased so each phase compiles, runs, and is demoable, matching repo conventions (minimal, greppable
diffs; Hungarian style; tabs). Read `CLAUDE.md` (conventions), `HANDOFF.md` (state), and — because
this mode is built on the exact same seams — `docs/WAR_BATTLE_DESIGN.md` and `docs/VILLAGE_MODE.md`
(the two most recent "new mode on the scene stack" precedents). This doc cites the real engine with
`file:line` so you don't have to re-derive the constraints; **verify them against current code before
relying on a line number** (the tree moves).

Owner's ask, verbatim intent: take the battle mechanics, use the Village world map so **each region
is a mission**, StarCraft-vein RTS — **scroll**, **resource + build (peasants, wizards,
halberdiers)**, borrow some **kaiju** mechanics, **flat surface (no terrain) for now**, **scale/tile
the existing backgrounds**, **minimap**, **stub the unit AI** (graft into the heuristic system later
if the framework proves interesting). Put it in the mode menu.

---

## 0. The one decision that governs everything (read first)

**The combat core is welded to a fixed battlefield and you must NOT widen it.** Concretely:

- The zone grid is compile-time `8 × 6` cells of `100 × 72 px` (`EZoneData`, `Enums.cs:15-21`),
  and the zone array is literally `new Zone[8][]` (`BattleData.cs:98`), origin `(112,70)`, so the
  playfield is exactly `800 × 432`. `SetZone` (`BattleData.cs:150`) **silently refuses to zone any
  unit outside that rect** — an unzoned unit is untargetable and inert. WarClash/VillageRaid both
  carry comments about clamping spawns inside it.
- **`Trooper` depends on that field**: `TrooperUpkeep` calls `_cBattleDataRef.SetZone(this)` every
  frame (`Trooper_Actions.cs:146`), and target-acquisition/flee all read the zone grid. So a
  `Trooper` cannot exist in a big scrolling world without dragging the fixed field in with it.
- The port plan and HANDOFF **explicitly warn against touching the zone/team core.**

**Therefore the RTS is a standalone `IGameScene` with its OWN world-space, camera, units, and update
loop — it does not run on `BattleSceneBase`/`BattleData` at all.** It **reuses the content and the
math** (animation data, textures, stats, damage formula, the drop/buff idea, the Kaiju stat model)
but **reimplements movement, target-acquisition, and rendering in free world-space.** This is how we
honor "use the battle mechanics" without rewriting the core the project depends on.

### Reuse vs. reimplement (the whole plan in one table)

| Concern | Decision | Why / where |
|---|---|---|
| Scene lifecycle, mode-menu entry | **Reuse** the `IGameScene` + scene-stack pattern | `WarBattle.cs`, `Village.cs`, `ModeSelect.cs` are copy-ready templates |
| Input (mouse/keys/wheel) | **Reuse** `InputSystem` events | `WarBattle`/`Village` handler pattern |
| Unit art + animation | **Reuse** `DataManager.CreateTemplate(cfg)` → `TrooperTemplate { cTextureRef, cAnimProcessorRef, cStats }` (`DataManager.cs:79`) and drive an `AnimationProcessor` yourself | gives Peasant/Halberdier/etc. sprites + the 18 named actions for free |
| Unit stats + damage numbers | **Reuse** the `Stats` struct and the damage formula (copy `Trooper.DealDamage`'s armor/crit math into a static helper, or call it if it's not zone-coupled) | keeps combat "feel" consistent |
| Build-a-unit with cost/cap | **Reuse the design** of Kaiju Hunt's `SpawnType { iCost, iCap, iSpawned }` + `_iReserves` roster (`KaijuHunt.cs` ~356) | proven build/economy affordance |
| Kaiju as a super-unit | **Reuse the model** (huge HP, HP-threshold drops, `_iAvailablePositions` "rated strength" decoupled from scale) — reimplement as an `RtsUnit` variant, **not** the `Kaiju` class (it's `BattleData`-coupled) | HANDOFF documents the kaiju knobs |
| The `Trooper`/`Kaiju` classes themselves | **Do NOT reuse directly** | they call `SetZone`; they need the fixed field |
| `BattleData` zones, `SceneManager` clash-push | **Do NOT use** | RTS is one continuous scene, not a push-to-battle |
| Drops/buffs HUD economy | **Optional, deferred** | it's `BattleSceneBase`-coupled; give RTS its own later if wanted |

If, mid-build, `Trooper` turns out to decouple cleanly from zones behind a small shim, that's a nice
future migration — but **v1 uses a lightweight `RtsUnit`** (below) so the fixed-field core is never
at risk.

---

# PART A — DESIGN SPEC

## A1. Vision & pillars

A small, readable, medieval RTS skirmish. You start with a keep and a few peasants on a flat green
field larger than the screen; you **scroll** around it, **gather** a resource, **build** a small army
of Peasants, Halberdiers, and Wizards, and **destroy the enemy keep** (and/or slay a **Kaiju**
guarding an objective). One screen, one continuous battle, StarCraft-shaped controls.

Pillars: **(1) readable at a glance** (few unit types, clear colors, minimap); **(2) reuse the
look** of MightyFights units; **(3) low-complexity AI** — units obey orders and auto-engage what's
near them, nothing cleverer for v1.

## A2. The world & camera

- **Flat world**, e.g. `3000 × 2000` world units (tunable). No terrain, no pathing obstacles for v1
  — units move in straight lines.
- **Background**: tile the existing `Backgrounds\dirt_grass 800x436` across the world rect (cheapest,
  seamless-ish), or scale one background up. No new art required. (Alternatively reuse
  `dirt_grass_large.png`.)
- **Camera**: a `Vector2` offset into world-space. Move it by **edge-scroll** (mouse near screen
  edge), **WASD/arrow keys**, and **minimap click**. Optional **mouse-wheel zoom** (defer if it
  complicates selection math). Screen↔world is a simple add/subtract of the camera offset.

## A3. Missions = map regions

The **region-select screen reuses the Village pattern** (`Village.cs` `Region` phase): the `AImap`
backdrop with clickable region markers. Each region is a **mission** with a config (world size, enemy
keep + starting army, whether a Kaiju is present, number of resource nodes, win condition). **v1: one
fully playable mission**; show the others as "locked" or as trivial variants of the same config.
Later these become a campaign.

## A4. Resources & building (StarCraft-vein)

- **One resource** for v1 — call it **Supply** (or "Coin", reuse the village term). Displayed
  top-left.
- **Resource nodes** on the map (a handful). **Peasants gather**: ordered onto a node, a peasant
  harvests on a timer, walks back to the **Keep**, deposits `+N`, repeats. *(Stub-friendly: if the
  gather loop isn't done in a phase, fall back to a passive `+N/sec` trickle so building still works.)*
- **Build menu** at the Keep (bottom bar, like a build palette): **Peasant**, **Halberdier**,
  **Wizard**, each with a **cost** and a **build time**; on completion the unit spawns at the Keep's
  **rally point**. Model the cost/cap after Kaiju Hunt's `SpawnType`.

## A5. Units (v1 roster)

| Unit | Role | Source template | Notes |
|---|---|---|---|
| **Peasant** | worker + weak melee | the stitched `Peasant` template (in the Kaiju roster today) | gathers + fights |
| **Halberdier** | line melee | `Halberdier` template | main army body |
| **Wizard** | ranged/magic | map to `Priest`/`Chaplain` caster art, or a template — **confirm what exists in the roster** | v1 can stub "ranged" as a longer attack range + a projectile-less hit |
| **Kaiju** | boss/objective | `RtsUnit` with kaiju-scale stats | huge HP, threshold drops optional, slow |

All are drawn as `RtsUnit`s (below), not `Trooper`s. Enemy units are the same set, red-tinted.

## A6. Selection & orders

- **Left-click**: select one unit. **Left-drag**: box-select many. Selected units get a highlight
  ring/underline.
- **Right-click**: on ground = **move**; on an enemy = **attack**; **attack-move** on a modifier or a
  palette toggle (defer the modifier if tight — plain move + auto-aggro is enough for v1).
- **Control groups / shift-queue: deferred.**

## A7. Minimap

Corner box (e.g. bottom-right). World scaled to the box; draw **unit dots** (team-colored), the
**Keeps**, the **Kaiju**, and the **camera viewport rectangle**. **Click to recenter the camera.**
Drawn in a HUD `SpriteBatch.Begin` block with no camera matrix.

## A8. Win / lose

Destroy the enemy Keep (and/or kill the mission's Kaiju) → **win**. Lose your Keep → **lose**.
`Esc` bails to the mode menu.

## A9. v1 scope vs. deferred

**In v1:** scroll + camera + tiled world + minimap; `RtsUnit` with select/move/auto-aggro (stub AI);
resource + build (Peasant/Halberdier/Wizard); one enemy keep with a small army; one mission; one
Kaiju objective; win/lose.
**Deferred:** terrain/pathfinding, fog of war, multiple resources, upgrades, control groups,
shift-queue, smarter AI, drops/buffs HUD, campaign progression across regions, zoom, mixed real
`Trooper` reuse.

---

# PART B — BUILD SPEC

## B1. Files (new; under `src/MightyFights.Core/Scenes/InGame/Rts/`)

| File | Contents |
|---|---|
| `RtsTypes.cs` | enums + data: `ERtsPhase { Region, Play, GameOver }`, `ERtsUnitType`, `EUnitState { Idle, Moving, Gathering, Attacking, Dead }`, `EOrderType`, `RtsUnitDef` (cost/build-time/stats/template key), `MissionDef`, `ResourceNode`, `Building` (Keep). |
| `RtsUnit.cs` | the lightweight unit: `{ ERtsUnitType eType; Vector2 tWorldPos; bool bDir; EUnitState eState; float fHp; Stats cStats; AnimationProcessor cAnimProc; Texture2D cTex; int iTeam; RtsUnit cTarget; Vector2 tOrderPos; }` + `Update(dt)` (state machine) + `Draw(batch)` (world-space). |
| `RtsUnitController.cs` | the "AI"/order logic — see B3. Can also be methods on `RtsUnit`; keep separable so a real heuristic can replace it. |
| `Rts.cs` | the `IGameScene`: region pick (Village-style) → play (camera, selection, build, minimap, sim) → game over. Owns the world, camera, unit lists, resource, build queue, input. Split into `Rts.cs` + `Rts_Draw.cs` + `Rts_Input.cs` partials if it gets large (repo convention). |
| `RtsMissions.cs` | the `MissionDef` table (mirrors `Village`'s region/vocation tables). |
| `ModeSelect.cs` (edit) | add `"Conquest"` entry + a `case` that news up `Rts` and `AddScene`s it — exactly like the Village entry (`ModeSelect.cs:146`). |

## B2. Camera & coordinate system

- `Rts` holds `Vector2 _tCamera` (top-left of the view in world-space), clamped to
  `[0, worldW-viewW] × [0, worldH-viewH]`.
- **Two draw passes** (the key rendering fact — `BattleSceneBase.Draw` today uses
  `Begin(SpriteSortMode.BackToFront, null)` with **no** matrix, `BattleSceneBase.cs:197`):
  1. **World pass**: `_cBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null,
     Matrix.CreateTranslation(-_tCamera.X, -_tCamera.Y, 0))` — draw background tiles, resource nodes,
     buildings, units, selection rings, order markers. Cull anything off-view.
  2. **HUD pass**: `_cBatch.Begin()` (no matrix) — resource readout, build palette, minimap,
     selection count, messages.
- **Screen→world**: `world = screen + _tCamera`. Every mouse handler that touches the field must
  convert first. Minimap and build-palette clicks stay in screen-space.

## B3. The unit controller (the "graft point" for AI)

The existing combat is a **perm-action wrapping a `DActionHeuristic` delegate, ticked once per frame
by `ActionManager<T>`** (see `CLAUDE.md` → Input/actions, and `Trooper.BasicBattleManager`). The RTS
mirrors that shape so a smarter heuristic can drop in later — but v1 is a plain per-unit state
machine in `RtsUnit.Update` / `RtsUnitController`:

- **Idle**: if an enemy is within `fAggroRadius`, acquire it (`Attacking`); else hold.
- **Moving** (from a move order): step `tWorldPos` toward `tOrderPos` by `cStats.iMovement`-derived
  speed; arrive → `Idle`. On attack-move, also scan aggro each step.
- **Attacking**: if target dead/gone → `Idle`; if out of range → step toward it; if in range →
  play the attack animation and apply damage on the swing tick.
- **Gathering** (peasants): Move→node, harvest timer, Move→Keep, deposit, repeat.
- **Target acquisition** = **O(n) nearest-enemy-by-distance** scan (fine for a few hundred units in a
  mock; add a coarse grid/spatial hash only if profiling says so). **This replaces the zone lookup —
  do NOT reintroduce zones.**
- **Damage**: reuse the `Stats` fields and the armor/crit math (copy `Trooper.DealDamage`'s formula
  into a static `RtsCombat.Resolve(attacker, defender)` helper). If that formula proves entangled,
  **stub v1 as `defender.fHp -= max(1, atk.iPower - def.iArmorClass)`** and refine later — the owner
  said stubbing the AI/combat is fine.

Keep the controller behind one method so replacing it with a real heuristic (or an off-the-shelf AI
lib grafted onto `DActionHeuristic`) is a localized change.

## B4. Animation reuse

`DataManager.CreateTemplate(cfg)` returns `{ cTextureRef, cAnimProcessorRef, cStats }`. Give each
`RtsUnit` its **own `new AnimationProcessor(animData)`** (don't share one across units — they'd
fight over frame state). Drive it with `SetAnimationCriteria(...)` on state changes (Walk/Attack/
Idle/Die), the same call `BasicBuff`/`Trooper` use. Set `bDir` from movement direction; draw with the
same flip convention troopers use. **Heed the bandit lesson** (`[[bandit-flip-lesson]]`,
HANDOFF): flip-jump comes from off-center sprite frames, not heuristics — if a unit teleports on
turn, it's the sprite data, not your code.

## B5. Rendering details

- Sort units by world-Y for a fake-depth look (`SpriteSortMode.BackToFront` + a `layerDepth` from Y).
- Cull: skip draws whose world rect doesn't intersect `[_tCamera, _tCamera+view]`.
- Selection ring / HP pip via the 1×1 `_cPixel` (same trick `WarBattle`/`Village` use).
- Minimap: `worldPos * (miniW/worldW)` offset into the minimap rect; dots as small pixel rects;
  camera viewport as an outlined rect.

## B6. Input

- `InputSystem.MouseMove` (edge-scroll test + drag-box update + hover), `MouseDown`/`MouseUp`
  (select / box-select / issue order / palette+minimap clicks), `KeyDown` (WASD/arrows/Esc),
  `MouseWheel` (zoom, optional). Same subscribe/unsubscribe shape as `Village.RegisterHandlers`.
- Left-down starts a potential drag; left-up with negligible drag = single select, else box-select.
- Right-down issues an order to the current selection (world-converted).

## B7. Content

- No new art strictly required for v1. Background: tile `Backgrounds\dirt_grass 800x436` (already in
  `Content.mgcb`). If you scale a bespoke big ground, add it via the pipeline: PNG →
  `content/...` → `Content.mgcb` block → `RunContentBuilder` (see `VILLAGE_MODE.md §4` for the exact
  steps and the `--` MSBuild-comment landmine).
- Region backdrop for mission-select: reuse `In Game\Village\region_map` (already built).
- Unit art: whatever templates exist in `cLSteward.cTemplates` / the Kaiju roster (Peasant,
  Halberdier, Captain). **Confirm the Wizard**: if no caster template exists, stub Wizard as a
  re-tinted Halberdier with longer range for v1 and flag art as a follow-up (owner's artist lane).

## B8. Phase plan (each phase compiles, runs, is demoable)

| Phase | Deliverable | Exit gate |
|---|---|---|
| **R1** | `Conquest` in ModeSelect → `Rts` scene: region pick (Village-style) → a scrollable tiled world with working camera (edge-scroll + WASD + minimap-click) and a live minimap showing the camera rect. No units. | You can scroll the whole map and the minimap tracks the view. |
| **R2** | `RtsUnit` + a Keep + a few starting Peasants. Left-click/drag select; right-click move. Units walk (animated) to ordered points. | Select and move a squad around the world; animations play; selection reads clearly. |
| **R3** | Resource (node gather **or** passive trickle) + build palette at the Keep (Peasant/Halberdier/Wizard with cost + build time), spawning at a rally point. | Gather/tick resource, build all three unit types, watch them muster at the Keep. |
| **R4** | Combat: attack-move + auto-aggro (stub AI), an enemy Keep with a small red army, damage via `RtsCombat.Resolve` (or the stub), death/removal, win (raze enemy Keep) / lose (lose yours). | Two armies fight, a Keep falls, the mission ends win/lose. |
| **R5** | A **Kaiju** objective (`RtsUnit` w/ kaiju stats), plus polish: HP pips, order feedback, message line, `Esc` to menu, cull/perf pass. | Kill the Kaiju to win a mission; runs smoothly with a few hundred units. |

Land R1 first and get an owner boot on it before R2 — the camera/coordinate system is the spine and
cheapest to correct early.

## B9. Grounded engine facts (verify before trusting the line number)

- **Scene stack / mode entry**: `SceneManager.AddScene`/`RemoveScene` (push/pop, only top ticks);
  `IGameScene` contract in `Interfaces/IGameScene.cs`; menu wiring in `ModeSelect.cs` (Village is
  `case 3`, `ModeSelect.cs:146`). Standalone-scene templates: `Scenes/InGame/War/WarBattle.cs`,
  `Scenes/InGame/Village/Village.cs`.
- **Fixed-field constraint (do not widen)**: `EZoneData` `8×6 / 100×72` (`Enums.cs:15-21`);
  `Zone[8][]` (`BattleData.cs:98`); `SetZone` drops off-field units (`BattleData.cs:150`);
  `TrooperUpkeep` calls `SetZone` each frame (`Trooper_Actions.cs:146`).
- **Camera injection point**: `SpriteBatch.Begin(..., Matrix transformMatrix)` overload; today's
  battle draw passes no matrix (`BattleSceneBase.cs:197`).
- **Unit content**: `DataManager.CreateTemplate(TemplateCfgMaster)` → `TrooperTemplate`
  (`DataManager.cs:79`); templates live on `DataStore.cInstance.cLSteward.cTemplates` /
  `cRSteward.cTemplates` (used in `WarBattle.BuildArmies`).
- **Build/economy precedent**: Kaiju Hunt `SpawnType { iCost, iCap, iSpawned }` + `_iReserves` +
  wheel select; `SpawnTrooper` spawn idiom (**`bDir` before `tPos`**), `LevelCurve.ApplyLevel`
  (`KaijuHunt.cs:283-360`).
- **Kaiju knobs**: `_iAvailablePositions` = "rated strength", decoupled from `_fScale`;
  HP-threshold drops (HANDOFF "Kaiju Hunt" section) — model, don't inherit the class.
- **Resolution**: `1024 × 576` (`GameShell.cs:28-29`). **Input**: `InputSystem` events
  (`WarBattle`/`Village` handlers). **Content pipeline**: `VILLAGE_MODE.md §4`.

## B10. Verify / hand-off

Per the standing workflow, **the owner boots and playtests** — don't launch the GUI
(`[[owner-runs-the-game]]`). Gate each phase with `dotnet build MightyFights.sln` (0 errors) +
`RunContentBuilder` for any content, confirm built artifacts land under the Desktop output's
`Content/…`, then summarize what to click and hand the boot to the owner.

## B11. Open questions for the owner (surface before/while building)

1. **Name** — "Conquest" is a placeholder (also considered: Dominion, Warlord, Command).
2. **Resource flavor** — reuse village "Coin", or a new "Supply"?
3. **Wizard** — is there caster art/template, or stub as ranged-Halberdier for v1?
4. **Kaiju role** — neutral creep guarding a resource, or the enemy's super-unit?
5. **Mission source** — bespoke `MissionDef`s, or generated from the `AImap` regions like Village?
