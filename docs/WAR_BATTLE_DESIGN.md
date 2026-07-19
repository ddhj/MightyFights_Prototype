# WAR_BATTLE_DESIGN.md

Design doc for **War Battle** — a new selectable game mode (working name) that recreates the
large-scale army-battle sequences from the *Suikoden* PlayStation games on top of the existing
MightyFights combat engine. Written 2026-07-09.

This doc is the **what and how**: what the mode is, and precisely how it reuses the MightyFights
engine to get there. It sits alongside `DESIGN_DIRECTION.md` (the owner's broader direction) and
`HANDOFF.md` (current state). Read those first if you haven't. Implementation is phased (bottom of
this doc); each phase ends playable.

## 1. Concept

A **large-scale battle**, not an RPG sortie. You command several **divisions** — formation-blocks
of many troopers each — on a **strategic grid**. The enemy fields divisions too. You maneuver them
toward contact; when two opposing divisions meet, the game drops into a **real, real-time field
battle** where both divisions' troops actually fight using the MightyFights combat engine. The
survivors carry their counts back up to the strategic layer. Destroy the enemy's divisions to win
the battle.

**Inspiration, and what we take from where:**
- *Suikoden I* (PS1) — the **rock-paper-scissors matchup layer**: Charge ▸ beats ▸ Bow ▸ beats ▸
  Magic ▸ beats ▸ Charge. Picking the right stance for a clash grants a decisive edge. We keep this
  as the *stance* committed when two divisions meet.
- *Suikoden II / V* — the **grid of divisions maneuvering to clash**, where positioning and who
  reaches whom matters. We keep this as the strategic layer.
- **The emphasis is the battle spectacle**, per owner steer (2026-07-09): "this is about the large
  scale battles more than any of the rpg elements." So the recruitment/character-unlock economy that
  drives Suikoden's special commands is **out of scope for v1**; the clash itself — real armies
  colliding on the field — is the point.

**Owner decisions locked in (2026-07-09, via design Q&A):**
- Clashes are **engine-backed** — real formations fighting on `BattleSceneBase`, not abstract
  numbers.
- Structure is a **blend** — grid maneuvering *plus* a per-clash stance advantage.
- (Assumed defaults, changeable): armies are **fixed demo divisions defined in the scene** (not the
  persisted roster) for v1; enemy division movement is **simple "advance toward nearest player
  division"** AI.

## 2. Why this maps cleanly onto the engine

The whole design rests on three facts about the current code, all verified:

1. **`SceneManager` (`Management Classes/SceneManager.cs`) is a scene stack.** `AddScene` (line 54)
   pushes a new scene, marks the one below `Inactive`, and unregisters its input handlers;
   `RemoveScene` (line 68) pops, reactivates the scene below, and re-registers its handlers. Only
   the top scene updates/draws (lines 42-52). This is precisely the control-flow a strategic layer
   needs: push a clash, let it run to completion, pop back and resume where we left off. No new
   plumbing required. (There is also `PopToRoot` (line 94) for a clean "quit to title.")

2. **`BattleSceneBase` (`Scenes/InGame/Battlegrounds/BattleSceneBase.cs`) is a complete,
   self-contained battle** with exactly the extension points we need. `Init()` runs common pre-init,
   then the mode's `SetupBattle()` hook (line 64), then common post-init. The state machine (Update,
   lines 160-189) runs `Init → Battle → Victory`, and **victory fires the moment either team's
   `cActiveList` empties** (lines 168-178), at which point it runs the exp tally and calls
   `OnBattleOver(iLosingTeamId)` (line 71). Modes also override `DrawHud` (line 67) and
   `ProcessVictoryState` (line 74). `KaijuHunt` and `BattleGround_Basic` are both thin modes over
   this — **`WarClash` is a third one.**

3. **`BattlegroundData` (`Support Classes/BattleData.cs`) is hardwired to exactly two teams** —
   id 0 (left/player) and id 1 (right/enemy), constructed in its ctor (lines 138-141). A battle is
   won when one of those two is wiped. **This is a perfect fit for a pairwise division-vs-division
   clash, and it is also the design's hard constraint** (see §7): a single clash is always exactly
   one player division against one enemy division. Three-way melee in one field battle is not
   supported and we do not attempt it.

So the mode is **two scenes**:

| Layer | Scene | Base | Responsibility |
|---|---|---|---|
| Strategic | `WarBattle` | `IGameScene` (new, standalone) | the grid, the divisions, movement, contact detection, stance selection, win/lose, HUD. Pushes clashes. |
| Tactical | `WarClash` | `BattleSceneBase` | one field battle between two divisions; spawns both formations, applies the stance advantage, runs to a wipe, reports survivors. |

## 3. The strategic layer — `WarBattle`

A new `IGameScene`, registered as a fourth entry in `ModeSelect` (`Scenes/OutGame/ModeSelect/
ModeSelect.cs` — adding an entry is one string in `_saEntries` and one `case` that news up the scene
and `AddScene`s it, exactly like Skirmish/Kaiju Hunt at lines 134-144).

**What it owns:**
- A **grid** (e.g. an N×M board of cells) drawn over the battlefield background. Cell size and grid
  dimensions are tunable; the strategic grid is *its own* coordinate space, unrelated to the combat
  engine's man-sized `Zone` grid (that one only exists inside a clash).
- A list of **`Division`** tokens per side (see §5), each sitting on a cell, drawn as a labeled
  marker (unit icon + troop count via the spritefont — no new art needed to start).
- **Turn-based maneuvering** (proposed): the player selects a division and moves it toward the
  enemy; then the enemy divisions take their step (simple pursuit AI). Discrete turns read most like
  a "war battle" and sidestep real-time pathing on the strategic layer. (Real-time-with-pause is a
  possible later variant; turns are the v1 call.)
- **Contact detection**: when a player division and an enemy division become adjacent / share a
  cell, a clash is triggered.
- **Stance selection**: on contact, the player commits a stance (Charge/Bow/Magic); the enemy's
  stance is chosen by AI (and optionally hidden — the Suikoden mind-game). The matchup is resolved
  to an *advantage* for one side (§6), which is handed to the clash.
- **Win/lose**: player wins when every enemy division is destroyed; loses when all of theirs are.

## 4. The clash lifecycle (how a battle is run and results come back)

This is the crux — how a `BattleSceneBase` battle is launched from the strategic layer and returns
its outcome without any callback system in `SceneManager`.

1. **Trigger.** `WarBattle` detects contact between player division `P` and enemy division `E`,
   resolves the stance matchup into an advantage, and builds a **`WarClashPlan`** (the two divisions'
   templates + troop counts + which side has the advantage and how much).
2. **Push.** `WarBattle` constructs `new WarClash(plan, result)` and calls
   `DataStore.cInstance.cSceneMgr.AddScene(clash)`. `WarBattle` is now frozen and its handlers are
   unsubscribed — the clash has the screen and input.
3. **Setup.** `WarClash.SetupBattle()` (the `BattleSceneBase` hook) spawns division `P` as a
   formation of troopers on team 0 and division `E` on team 1, applying the advantage (§6). Spawn
   uses the **exact pattern `KaijuHunt.SpawnTrooper` already uses** (lines 283-308): `CreateTemplate`
   → `new Trooper` → add to `cTeam.cActiveList`/`cMembers` → set `bDir` **then** `tPos` (order
   matters, per the ref-point comment there) → `_cObjMgr.AddObject` → set `eState = Ready` →
   `AddPermAction(BasicBattleManager)`. Formations spawn inside the field bounds (x 112-912,
   y 70-506; see the `KaijuHunt` click-guard at line 353) — one side massed left, the other right.
4. **Run.** The base state machine runs the fight to a wipe (no changes needed). All the existing
   machinery — zones, AI heuristics, buffs/drops, slow-mo, pause, music — comes for free.
5. **Resolve.** When a team empties, the base calls `WarClash.OnBattleOver(iLosingTeamId)`. Here we
   **read the survivors out of the winning team's `cActiveList` before teardown** (the same read
   `KaijuHunt.BankSurvivorExperience` does at lines 316-327) and write them into the shared
   **`WarClashResult`** object: winner side, survivor count, (optionally) banked exp. We do *not*
   save/level here for v1 (fixed demo armies).
6. **Return.** After a short victory beat, `WarClash.ProcessVictoryState` (or a right-click, matching
   the battleground withdraw convention) calls `BackToMenu()` → `RemoveScene(this)`. `WarBattle`
   reactivates.
7. **Read-back.** `WarBattle` polls the `WarClashResult` each `Update` (it holds the reference it
   passed in). Seeing it complete, it updates the map: the losing division is removed, the winning
   division's count is set to the survivor count, and the turn continues. Win/lose is re-checked.

**Why a shared result object instead of a return value:** scenes are decoupled through the stack;
there's no "the scene you pushed has finished" event. Passing a mutable `WarClashResult` into the
clash's ctor and polling its `bComplete` flag after control returns is the minimal, robust idiom —
no changes to `SceneManager` or `IGameScene`.

## 5. Data model

Kept deliberately small and mode-local (nothing in the shared engine changes):

- **`Division`** (strategic-layer object): grid position; `side` (player/enemy); `iTroopCount`;
  the `TemplateCfgMaster` its troops are built from (fixed demo templates for v1 — e.g. reuse
  Halberdier/Peasant/etc. from `cLSteward`, but *not* mutating persisted progression); a base
  **unit type / stance affinity** (Charge/Bow/Magic — see §6); display label.
- **`WarClashPlan`** (strategic → tactical): player division snapshot, enemy division snapshot,
  resolved advantage (which side, magnitude). Immutable input to the clash.
- **`WarClashResult`** (tactical → strategic): `bComplete`, winning side, survivor count. The
  strategic layer creates it, passes it to the clash, and reads it after pop.

## 6. The stance / RPS blend

Each clash resolves a Suikoden-style matchup into a concrete combat edge:

- **The three stances:** Charge, Bow, Magic. **Charge beats Bow, Bow beats Magic, Magic beats
  Charge.** Same stance = even fight, no edge.
- **How it's chosen (blend):** each division has a base affinity, and on contact the player *commits*
  a stance for that clash (defaulting to the division's affinity, or freely chosen — the mind-game
  layer). Enemy stance is AI-picked. The winner of the RPS gets the advantage; a tie gives neither.
- **How the advantage is applied in the clash** (this is what makes it "engine-backed," not a dice
  roll): the advantaged side spawns with a **bonus** — the cleanest, most readable options, in
  preference order:
  1. **Bonus troops** — the advantaged division fields a % more troopers in the formation (most
     legible on screen; directly uses the spawn loop).
  2. **A stat buff** — apply a power/hp multiplier to the advantaged division's template before
     spawn, mirroring how `LevelCurve.ApplyLevel` already tweaks `cStats` pre-spawn in
     `KaijuHunt.SpawnTrooper`.
  3. **Upfront casualties** — the disadvantaged division loses a slice of its count before the fight.

  v1 proposal: **bonus troops** (+ maybe a small power buff), exact numbers deferred to tuning
  (a `CombatTuning`-style knob, matching how the rest of the mode's balance is externalized).

## 7. Constraints & landmines (from the real engine)

- **Two teams only — clashes are strictly pairwise.** `BattlegroundData` builds exactly teams 0 and
  1 and wins on either wipe. A field battle is always one division vs one division. Multiple player
  divisions ganging one enemy is expressed on the *strategic* layer (sequenced clashes), never as a
  3-team field battle. Designing around this from day one avoids a rewrite of the zone/team core the
  port plan explicitly warned against touching.
- **Zones are man-sized and the field is fixed** (background at 112,70, 800×436; zone origin 112,70).
  Formations of many troopers are fine — that's literally what Skirmish does — but spawn **inside**
  the field: `BattleData.SetZone` (lines 158-164) deliberately refuses to zone units left/above the
  grid origin (they become untargetable). Use the `KaijuHunt` spawn bounds.
- **Handler-leak / stale-eState gotchas are already fixed in `SceneManager` / noted in
  `BattleData`.** `RemoveScene` now unregisters the removed scene's handlers (SceneManager comment,
  lines 70-79). But `BattlegroundData.Clear()` does **not** reset `eState` off `Victory` — so a
  fresh clash must set its own state (the base does: `Init` at line 127). Re-entering the mode many
  times (many clashes per battle) is exactly the reentrancy path those comments warn about; verify
  no handler leak across dozens of clashes.
- **Survivors must be read in `OnBattleOver`, before `Unload`/`CleanCommon`** — teardown clears the
  teams (`CleanCommon` → `_cBattleData.Clear()`, base line 302-308). `OnBattleOver` runs first
  (line 176), which is why the result is captured there.
- **Music:** each clash starts its own random battle track and the base stops it on victory; a
  string of clashes will restart music each time. Acceptable for v1; a "strategic theme that ducks
  under clashes" is a later polish item.

## 8. Phase plan

Each phase compiles, runs, and is independently demoable (minimal-diff, per repo conventions).

| Phase | Deliverable | Exit gate |
|---|---|---|
| **W1** | `ModeSelect` gains "War Battle". `WarBattle` scene: grid render, player + enemy `Division` tokens, select-and-move, turn hand-off. No clashes yet. | Divisions maneuver on the grid; enemy divisions step toward you; it renders and reads clearly. |
| **W2** | `WarClash : BattleSceneBase` + `WarClashPlan`/`WarClashResult`. Contact pushes a real field battle of both formations (no stance yet, even fight); survivors banked back; loser removed. | Two divisions touching triggers a live battle; on return, counts update and the loser is gone. |
| **W3** | Stance/RPS layer: commit a stance on contact, resolve the matchup, apply the advantage (bonus troops) in the clash. | The stance choice visibly and measurably changes the clash outcome. |
| **W4** | Win/lose conditions, enemy division AI polish, strategic HUD, externalized tuning; docs updated (`HANDOFF`, `DESIGN_DIRECTION`, this file). | The full mode is winnable and losable end-to-end. |

## 9. Open questions / deferred decisions

- **Name.** "War Battle" is a working title (Suikoden's own term). Alternatives: "Grand Battle,"
  "War," "Army Battle." Owner call before it lands in `ModeSelect`.
- **Army source.** v1 = fixed demo divisions in the scene. Later: derive player divisions from the
  persisted, leveled roster (like Kaiju Hunt banks exp) so War Battle plugs into progression. Kept
  out of v1 per the "battles over RPG elements" steer.
- **Division composition.** v1 = one template per division (homogeneous formation). Mixed-unit
  divisions (some halberdiers + some archers) are a natural extension once the clash spawn supports
  multiple templates on one team — trivial (loop over a list), deferred only to keep W2 small.
- **Retreat vs annihilation.** v1 = a clash runs to a full wipe; the loser division is destroyed.
  A "reduced below threshold → retreat with survivors" rule (more Suikoden-faithful) is a W3/W4
  refinement once the base loop feels right.
- **Strategic turn model.** Proposed discrete turns; real-time-with-pause is the alternative if the
  owner wants it to feel less like a board game.
