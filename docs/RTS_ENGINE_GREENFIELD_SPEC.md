# RTS_ENGINE_GREENFIELD_SPEC.md — new-repo RTS engine (harvest MightyFights, don't inherit its cage)

**Spec for the next session (Sonnet) to stand up a NEW repository: a purpose-built RTS engine.**
Written 2026-07-18. This is the *opposite* framing to `docs/RTS_MODE_SPEC.md`: that one bolts an RTS
mode onto MightyFights and works *around* the fixed battle grid. **This one starts fresh** and
designs the spatial/AI/render systems an RTS actually wants, while **lifting everything reusable**
from MightyFights so we don't rebuild art, pipeline, or combat feel from zero.

Governing principle from the owner: *"use as much as we can from MightyFights but don't let it hinder
capability — if we need a new engine, build what's needed."* So: **harvest assets and patterns,
discard the XNA-era constraints.**

---

## 1. Harvest vs. leave (the crux)

| From MightyFights | Verdict | Notes |
|---|---|---|
| **The gen-AI art package** (`Downloads/aiart`) + village art already in `content/In Game/Village` | **Harvest** | but pick **one style lane** (see §2) — the package is 3–4 inconsistent styles today |
| **`stitch.py` sprite pipeline** (`tools/sprite_stitcher/`) → packed sheet + `AnimationData` JSON | **Harvest wholesale** | repo-independent Python; the recenter + flip-jump validator work (bandit lesson) carry over |
| **`AnimationData` / `AnimationProcessor`** format (named actions, frames, TexturePacker layout) | **Harvest** | proven data-driven animation; keep the JSON schema, reimplement the runtime lean |
| **`Stats` model** + damage/armor/crit/atk-speed math | **Harvest the numbers/formula** | port as a plain struct + a pure `Combat.Resolve` function |
| **The heuristic pattern**: `DActionHeuristic` delegates queued/scheduled by `ActionManager<T>`, one eval per frame | **Harvest the *idea*** | this is the AI substrate — reframe as behaviors on agents (§3.4); it's the most valuable non-art asset |
| **Drop/buff economy** (hammer + animal-gem buffs, radius apply) | **Harvest as optional mechanic** | good pickups system; decouple from any battle-scene base |
| **Kaiju model**: huge HP, HP-threshold drops, "rated strength" (`_iAvailablePositions`) decoupled from render scale | **Harvest the model** | great super-unit template |
| **Unit/mode concepts**: Peasant/Halberdier/Wizard, Kaiju, the Village economy, `AImap` regions-as-missions | **Harvest as content/design** | data, not code |
| **8×6 `Zone` grid / `BattleData` / `SetZone` targeting** (`Enums.cs:15`, `BattleData.cs`) | **LEAVE** | the exact ceiling we're escaping — replace with a spatial hash broadphase (§3.2) |
| **`Trooper`/`Kaiju` classes, `BattleSceneBase`, `SceneManager` stack** | **LEAVE / rewrite** | too coupled to the grid + XNA scene model |
| **`fastJSON`, `IsolatedStorage` saves, Mercury Particles binary, XNA content pipeline quirks** | **LEAVE** | dead/legacy; use `System.Text.Json`, plain files, a modern particle lib or a small shim |

**One-line takeaway:** we keep the **art, the sprite pipeline, the animation data format, the stats
math, and the heuristic-as-behavior idea**; we throw away **the zone grid and everything welded to
it.**

## 2. Best-fit stack & art lane

- **Language/framework: C# / .NET 8 + MonoGame DesktopGL.** This maximizes reuse — the animation
  data, stats, and heuristic patterns are already C#, the team has a working MonoGame toolchain
  (`src/MightyFights.Desktop`), and KNI/BlazorGL is the known web path later. A more exotic engine
  would cost more than it buys. **MonoGame is the render/input/audio substrate only** — the sim is
  greenfield (§3), not `Game`-component soup.
- **Simulation architecture: a lightweight ECS** (e.g. `Arch` or `Friflo`/`MonoGame.Extended`
  entities, or a hand-rolled struct-of-arrays) — RTS wants hundreds–thousands of agents; OO
  `Trooper` objects with per-frame virtual calls won't scale. Entities = components
  (`Transform`, `Velocity`, `Health`, `Unit`, `Sprite`, `Behavior`, `Owner`).
- **Art lane: standardize on the cartoon-vector "village/unit-icon" style** (the `village icons*`,
  `soldier icons*`, `knight icons*` sheets) — it reads best small and top-down, scales to many
  on-screen units, and is the most internally consistent lane in the package. Portraits (painterly
  busts) → UI/command-card only. Pixel demons → a distinct "monster" faction if wanted. **Route all
  of it through `stitch.py` → `AnimationData`.** Owner's artist + AI lane both already feed this.

## 3. Engine components (freed from the grid)

### 3.1 World & time
Continuous 2D world (float coords), no cell snapping. **Fixed-timestep simulation** (e.g. 30 Hz)
decoupled from render framerate for determinism; render interpolates. Camera is a world-space view
transform (translation + optional zoom), applied as a `Matrix` to the world sprite batch; HUD/minimap
drawn untransformed.

### 3.2 Spatial partitioning (replaces the zone grid)
A **uniform spatial hash** (or loose quadtree) purely as a **broadphase** for neighbor queries
(target acquisition, aoe, selection, collision) — cell size ~= a unit's engage radius. This is the
clean replacement for `SetZone`/`GetZoneByPosition`: same "who's near me" service, **no fixed field,
no untargetable-if-outside failure mode.** Rebuild/refresh each sim tick.

### 3.3 Movement & steering
Steering behaviors (seek/arrive/separation) for v1 — flat world, straight-line move + boids-style
separation so squads don't stack. **Pathfinding deferred** (no terrain in v1); when terrain arrives,
add flow-fields (best for RTS mass movement) or grid A* on a navigation grid that is **separate from
any combat concept**.

### 3.4 AI / heuristics (the harvested pattern, modernized)
Carry forward the MightyFights idea — *a per-agent behavior evaluated each tick* — as a **behavior
component**: each unit has a small state machine or behavior tree (Idle → Move → Gather → Attack →
Flee). Target acquisition = spatial-hash nearest-enemy. Keep the **behavior pluggable** (a delegate
or a `IBehavior.Tick(ref sim, entity)`), so the owner's "AI components out there" can be grafted per
unit type without touching the engine. **v1 stubs are fine** — orders + auto-aggro.

### 3.5 Rendering
Sprite batching with **Y-sort for depth**, frustum/view culling, animated via the ported
`AnimationProcessor`. Selection rings, HP bars, order markers as primitives. **Minimap** as a scaled
world projection with unit dots, buildings, and the camera viewport rect; click-to-move-camera and
click-to-order.

### 3.6 Command & selection
Single click, drag-box select, right-click move/attack/gather, **control groups (0–9)**, shift-queued
orders, attack-move. (v1 can ship single+box+right-click and defer groups/queue.)

## 4. RTS game mechanics (the feature set)

- **Resources**: 1–2 resource types; **worker gather loop** (harvest → return-to-depot → deposit) via
  the behavior system.
- **Production & build**: buildings produce units on a cost + build-time queue; tech/upgrade tree
  (defer tree to later). Model costs after Kaiju Hunt's `SpawnType { iCost, iCap, iSpawned }`.
- **Army & combat**: the harvested `Stats`/`Combat.Resolve`; unit types Peasant / Halberdier /
  Wizard (ranged) / Kaiju (super-unit); death, corpses/drops optional.
- **Fog of war**: deferred (v1 full-visibility); when added, a per-owner visibility grid.
- **Missions / skirmish**: `AImap` regions as a mission select (harvest the Village region-pick
  design); skirmish vs. a stubbed enemy AI (build order + attack waves). Win = raze enemy base / kill
  Kaiju; lose = lose your base.
- **Minimap, control groups, hotkeys** as above.

## 5. New-repo layout (suggested)

```
RtsEngine.sln
src/Engine/            sim core: ECS, spatial hash, steering, behavior runtime, combat math (no MonoGame refs ideally)
src/Render/            MonoGame: camera, sprite/anim renderer, minimap, HUD, input
src/Game/              the actual RTS: unit/building/mission defs, economy, skirmish AI, main loop
src/Content/           AnimationData JSON + packed sheets (from stitch.py), unit/mission JSON defs
tools/sprite_stitcher/ copied from MightyFights, unchanged
docs/                  this spec + decisions log
```
Keep `Engine` free of MonoGame types where practical (pure sim → testable, portable to a web head).

## 6. Phased stand-up plan

| Phase | Deliverable | Gate |
|---|---|---|
| **E0** | New repo, .NET 8 + MonoGame skeleton, `stitch.py` + one unit's `AnimationData` ported, a sprite drawn in a scrollable camera view | a unit animates on a scrollable field |
| **E1** | ECS + spatial hash + steering: spawn N units, select (single/box), right-click move, separation | a squad moves and doesn't stack |
| **E2** | Behavior runtime + combat: auto-aggro, `Combat.Resolve`, death; two factions fight | armies clash and resolve |
| **E3** | Economy: resource nodes, worker gather loop, a depot, build queue for Peasant/Halberdier/Wizard | gather → build an army |
| **E4** | Skirmish: enemy base + stub AI (build + attack waves), win/lose, minimap, `AImap` mission select | a full skirmish is winnable/losable |
| **E5** | Kaiju super-unit + polish (control groups, HP bars, order feedback, culling/perf) | few-thousand-unit skirmish runs smoothly |

## 7. Open decisions for the owner

1. **Reuse C#/MonoGame (recommended, max harvest) or jump frameworks?** — recommend staying.
2. **ECS choice** — off-the-shelf (`Arch`) vs. hand-rolled SoA.
3. **Art lane lock** — confirm the cartoon-vector/unit-icon style as canonical; portraits→UI only.
4. **How much MightyFights code is literally copied vs. re-derived** — I recommend copying only
   `stitch.py`, the `AnimationData` schema, and the `Stats`/combat math; re-derive the rest clean.
5. **One repo or a shared "core" library** the old game could also consume later.

## 8. Where to lift from (paths in this repo)

- Sprite pipeline: `tools/sprite_stitcher/stitch.py` (+ its recenter/flip-jump validator); pattern in
  `docs/AI_ART_PIPELINE.md`.
- Animation runtime shape: `AnimationProcessor` / `AnimationData` (support lib + `DataManager.cs`
  `CreateTemplate`).
- Stats + damage math: `Stats` struct + `Trooper.DealDamage` (armor/crit/atk-speed).
- Heuristic pattern: `Support Classes/ActionManager.cs` + `Trooper` `DActionHeuristic` usage.
- Build/economy affordance: Kaiju Hunt `SpawnType` roster (`KaijuHunt.cs`).
- Drop/buff mechanic: `Scenes/InGame/Battlegrounds/Objects/Buffs/*` (see `VILLAGE_MODE.md §5`).
- Region-as-mission UX: `Scenes/InGame/Village/Village.cs` (region pick).
- Kaiju knobs: HANDOFF "Kaiju Hunt" section.

**Explicitly do NOT copy:** `BattleData`/`Zone`/`SetZone`, `EZoneData`, `BattleSceneBase`,
`SceneManager`, `fastJSON`, isolated-storage saves, Mercury Particles.
