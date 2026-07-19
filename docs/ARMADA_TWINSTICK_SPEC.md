# ARMADA_TWINSTICK_SPEC.md — twin-stick space shooter engine (new repo, harvest MightyFights)

**Spec for the next session (Sonnet) to stand up a NEW repository: a twin-stick, top-down space
shooter/RPG in the vein of the Sega Dreamcast game *Armada* (Metro3D, 1999).** Written 2026-07-18.
**Pure engine, no art this pass.** Same harvest philosophy as `docs/RTS_ENGINE_GREENFIELD_SPEC.md`
(read its §1–§2, §5, §8 — the stack, repo layout, and pipeline lifts are identical and are not
repeated here). This doc covers what's *different*: the twin-stick shooter feel and the systems it
needs. Phase 1 = baseline movement + space field + shooting; everything else grows organically on
top.

---

## 1. The *Armada* feel (what we're emulating)

- **Twin-stick, top-down:** left stick thrusts the ship, right stick aims + fires. Continuous open
  space, camera follows the ship.
- **RPG progression on a ship, not a character:** kills drop **credits/XP**; you **level up** and
  spend on **weapon/engine/shield/hull upgrades**. Persistent between runs (save).
- **Open galaxy of sectors:** many connected space "rooms"/sectors you travel between; escalating
  enemy density; a menacing enemy faction (the "Armada"). *(Sector graph is a grow-later system;
  P1 is one endless field.)*
- **Co-op DNA:** designed so 2–4 ships can share the field (defer actual multiplayer; keep the sim
  N-player-clean — no "the player" singletons).
- **Feel target:** fast, readable, lots of bullets + enemies, satisfying pickups.

## 2. Harvest from MightyFights (shooter-relevant)

| Piece | Use as | Where |
|---|---|---|
| **`Stats` + damage/armor/crit math** | ship + enemy stats; `Combat.Resolve(shot, target)` | `Stats`, `Trooper.DealDamage` |
| **Heuristic/`ActionManager` pattern** (per-agent delegate ticked each frame) | **enemy ship AI** — seek / strafe / kite / swarm / flee behaviors, pluggable per enemy type | `Support Classes/ActionManager.cs`, `Trooper` `DActionHeuristic`; Kaiju has a bespoke attack heuristic worth studying |
| **Drop/buff economy** (roll on death, click to collect, apply effect) | **pickups**: credits, health/shield orbs, weapon powerups, temporary buffs | `Objects/Buffs/*` (see `VILLAGE_MODE.md §5`) |
| **Leveling: `LevelCurve` + `ExperienceData` + banking + save** | **ship progression** (XP→level→upgrade points), persisted | `KaijuHunt.BankSurvivorExperience`, `LevelCurve`, `Steward`/`DataStore` save concept (use `System.Text.Json`, not fastJSON/isolated storage) |
| **Kaiju model** (huge HP, HP-threshold drops, rated-strength) | **boss ships / capital ships** that drop loot as you chip them down | HANDOFF "Kaiju Hunt" |
| **`AnimationData`/`stitch.py`/animation runtime** | ship + projectile + explosion sprites when art comes | as in greenfield §1 |
| **Spatial-hash broadphase idea** (NOT the zone grid) | bullet↔ship + ship↔ship collision, aim-assist neighbor queries | greenfield §3.2 — the zone grid is left behind |

**Leave behind** (same as greenfield): `Zone`/`BattleData`/`SetZone`, `BattleSceneBase`,
`SceneManager`, fastJSON, isolated storage, Mercury particles.

## 3. Engine components (twin-stick specifics)

### 3.1 Input — the twin sticks
- **Gamepad:** left stick = thrust direction/magnitude; right stick = aim direction (+ auto-fire while
  deflected, or right-trigger to fire). **KB/M fallback:** WASD thrust, mouse aim, LMB fire.
- Abstract to an **intent** per ship: `{ Vector2 tMove; Vector2 tAim; bool bFire; ... }` — so AI ships
  and human ships feed the same movement/weapon code (co-op- and AI-clean).

### 3.2 Movement / physics
Arcade-Newtonian: thrust adds to `velocity`, apply **drag** + **max speed**; ship faces `tAim`
(decoupled from travel = the twin-stick signature). Optional inertia tuning knob (drifty↔tight).
Fixed-timestep sim (30–60 Hz), render interpolates.

### 3.3 Weapons & projectiles
Data-driven weapon defs `{ fireRate, damage, projectileSpeed, spread, count, lifetime, pattern }`.
A **projectile pool** (thousands of bullets — pool, don't GC-churn). Bullets are entities with
`Transform`+`Velocity`+`Damage`+`Owner`+`Lifetime`. Weapon tiers = upgrade targets.

### 3.4 Enemies & AI (the harvested heuristic pattern)
Each enemy is an agent with a **behavior** (the reframed `DActionHeuristic`): acquire nearest hostile
via spatial hash → steer (chase/strafe/orbit/retreat) → fire when aimed + in range. Types differ by
behavior + stats + weapon (e.g. Chaser, Strafer, Turret, Swarmling, Boss). **P1 stubs** = one or two
simple behaviors; keep pluggable so smarter AI drops in later.

### 3.5 Collision
Spatial-hash broadphase + circle tests. Pairs: bullet↔ship, ship↔ship (bump), pickup↔player.
No terrain in P1.

### 3.6 Health / shields / damage
`{ hull, shield, armor }`; shield regens after a no-hit delay; `Combat.Resolve` from the harvested
math. Death → explosion + drop roll.

### 3.7 Pickups & economy
On enemy death, roll (harvested drop economy): **credits/XP**, health/shield orbs, weapon powerup,
temporary buff. Auto-collect on contact (twin-stick convention) rather than click.

### 3.8 Progression & persistence
XP→level (`LevelCurve`); levels grant upgrade points spent on weapon/engine/shield/hull. **Save the
ship** between sessions (`System.Text.Json`, plain file). Banking mirrors
`KaijuHunt.BankSurvivorExperience`.

### 3.9 Spawning, sectors, waves
P1: an endless spawner (increasing rate/difficulty) on a bounded/wrapping field. Grow: a **sector
graph** (nodes = space rooms, edges = jumps), per-sector spawn tables, a boss sector, the "Armada"
faction pressure.

### 3.10 Camera & HUD
Camera follows ship (with look-ahead toward aim). HUD: hull/shield bars, credits/XP, current weapon,
score, and a **radar/minimap** ring showing nearby enemies + objectives.

## 4. Phase plan (grow organically from P1)

| Phase | Deliverable | Gate |
|---|---|---|
| **P1 — baseline** | New repo (greenfield stack); a **space field** (scrolling starfield/bounded world), one **player ship** with full **twin-stick move + aim + fire**, a **projectile pool**, a couple of **stub-AI enemies**, **collision + damage + death**, follow-camera. | You fly, shoot, kill enemies, take damage, and die — the core loop is alive. |
| **P2 — combat depth** | Multiple weapon types, enemy variety (2–3 behaviors), explosions, HP/shield + regen, a basic HUD + radar. | Fights feel varied and readable. |
| **P3 — economy/progression** | Pickups (credits/XP/health/powerups), XP→level→upgrade spend, ship save/load. | Killing things makes your ship better, and it persists. |
| **P4 — world** | Sector graph + jumps, per-sector spawn tables, one boss (Kaiju-model capital ship). | You travel a small galaxy and fight a boss. |
| **P5 — feel/scale + co-op-clean** | Juice (screen shake, hit flash), perf (pooling/culling for thousands of bullets), verify the sim is N-ship clean for future co-op. | Dense, juicy, smooth; a 2nd ship could slot in. |

## 5. Repo layout, stack, pipeline
**Identical to `RTS_ENGINE_GREENFIELD_SPEC.md` §2/§5/§8** — C#/.NET 8 + MonoGame render/input,
greenfield ECS-lite sim, spatial hash, `stitch.py`→`AnimationData` when art arrives, `Engine` kept
free of MonoGame types where practical. Don't restate; reuse that doc.

## 6. Open decisions for the owner
1. **Input priority** — ship gamepad-first (true twin-stick) or KB/M-first for early dev?
2. **Movement feel** — drifty/inertial (Asteroids-like) vs. tight/responsive.
3. **Field topology in P1** — bounded arena vs. wrapping torus vs. free-scroll.
4. **How RPG** — light (weapon tiers) vs. deep (Armada-style stat trees, ship classes).
5. **Co-op** — commit to N-player-clean sim now (recommended, cheap if done early) even though
   multiplayer is far off.

## 7. Lift-from paths (this repo)
Stats + damage: `Stats` / `Trooper.DealDamage`. AI pattern: `Support Classes/ActionManager.cs` +
`Kaiju.cs` bespoke heuristic. Pickups: `Scenes/InGame/Battlegrounds/Objects/Buffs/*`. Leveling/save:
`KaijuHunt.BankSurvivorExperience`, `LevelCurve`, `DataStore`/`Steward`. Sprite pipeline:
`tools/sprite_stitcher/stitch.py`, `docs/AI_ART_PIPELINE.md`. **Do NOT copy** the zone/battle-scene
stack.
