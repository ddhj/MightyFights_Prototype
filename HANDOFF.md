# HANDOFF.md

Resume-here doc for the MightyFights MonoGame port. Read this first, then
`mightyfights_monogame_port_plan.md` (the phase plan) and `docs/PORT_NOTES.md` (the detailed
decision log — every finding/deviation below is written up in full there, this file is just the
"where are we, what's next" summary). `CLAUDE.md` covers repo conventions.

**Last updated:** 2026-07-05. Since the 07-04 handoff below was written, commit `0f5f394` landed
11 items (leveling loop in Camp, Bandit + SwordHero stitched into content, CombatTuning.json,
win-x64 publish). Then this session: **the "direction flip looks wrong" bug is root-caused and
fixed** — it was never the trooper heuristics (they're correct); the Bandit's GIMP-exported frames
had the body at x≈21 of the 100px canvas instead of centered, and because the flip math mirrors
across the canvas AND `Trooper.UpdateRefPoints` derives `tCenter` from the same flip offsets,
every `bDir` change teleported sprite+`tCenter` ~48–58px and made `bDir` oscillate near any
destination. Fix: `stitch.py` gained a `"recenter"` config option + a flip-jump validator, Bandit
and SwordHero were re-stitched (movement-frame flip-jump now 0–2px, was 48/18px), content rebuilds
clean. New: `docs/AI_ART_PIPELINE.md` — the template contract for generating new units (artist or
AI) + tool options. All uncommitted alongside the owner's own working-tree edits.
**The project has pivoted from porting to building** — read `docs/DESIGN_DIRECTION.md` first for
the owner's decisions, the kaiju design, the art pipeline, and the reconstructed original design.

## Current state

- **Branch:** `monogame-port` (tag `pre-port` marks the commit before any port work started).
- **The port is done except V6.7** (Linux build+boot smoke). Phases 0–5 complete; Phase 6
  runtime-parity gates V6.1–V6.6 validated (owner playtest + scripted verification). The
  map-campaign design was deprecated by the owner — V6.3 is obsolete, not failed.
- **The game is playable end-to-end**: Title → mode select → **Skirmish** (classic Camp →
  Battleground loop) or **Kaiju Hunt** (new demo mode). Music, SFX, particles, buffs, save/load
  all live. Boot loads the saved campaign when one exists (`LoadData` with `InitNew` fallback).
- **Kaiju Hunt v1 is feature-complete and actively being balanced/debugged by the owner directly**
  (division of labor as of this session: owner drives game mechanics/combat balance/leveling with
  their own IDE debugger; Claude continues the GIMP art-recovery pipeline — see below).
  - Wheel-selected spawn roster (Halberdier/Captain/Peasant, costs + caps), mid-battle
    reinforcement clicks (now guarded so a click on a HUD/field pickup can't also spawn a
    reinforcement — see `BObjectManager.IsClickableAt`).
  - Reinforcements spawn straight into `Ready`/charging state — no more scripted march-to-formation
    (that was a Skirmish-only mechanic; removed for Kaiju Hunt in `KaijuHunt.SetupBattle`/
    `SpawnTrooper`).
  - Attack targeting was completely reworked this session: the old fixed 6-slot
    `ETrooperAttackPos` enum (removed from `Enums.cs`) is replaced by a **dynamic radial slot
    system** (`Trooper_Combatant.cs`: `GetVectByPos`/`GetAttackPoint`/`EnsureSlotArray`) — slots
    are evenly spaced around the target's `tCenter`, count driven by `_iAvailablePositions`
    (now `protected`, in `Trooper.cs`). This field is the target's **"rated strength"** — a
    design/gameplay knob for how many attackers it can meaningfully engage at once, explicitly
    **decoupled from `_fScale`** (visual size) per owner directive, because minion tiers will need
    independent tuning from their render scale. Kaiju is currently `_iAvailablePositions = 200`
    (`Kaiju.cs`, was 14 — too tight, caused "troopers standing around" and "attacking but missing"
    before the radial rewrite fixed the underlying crowding).
  - Kaiju drops are now **HP-threshold-based, not death-based** (`Kaiju.DealDamage` override,
    `_iNextDropThresholdPct` stepping down every 10% of max HP) since a kaiju only dies once per
    hunt — a pure death-drop would mean zero rewards for the whole fight. The base Trooper
    death-drop still also fires on the final kill.
  - Kaiju armor tuned down `12 → 3` (`Kaiju.cs` ctor) after "damage numbers are 0's" — the
    `DealDamage` formula's armor subtraction was eating nearly all incoming damage.
  - **Owner is mid-iteration on a brand-new `Attack_Kaiju` heuristic** (`Kaiju.cs`, assigned over
    `Attack_Basic` in the ctor) — mirrors `Attack_Basic` but with a flat 10%-defend-chance branch
    instead of Attack_Basic's attacker-count-based one. This needed three `Trooper` fields
    (`_bAttacking`, `_iAttackingPos`, `_cAnimProc`) promoted from `private` to `protected` to
    compile — done, build is clean. **This and the surrounding files are currently uncommitted
    working-tree changes** (see "Uncommitted work" below) — don't assume they're landed.
  - **F2 toggles a debug overlay** (`DataStore.bDebugCenters`, wired in `GameShell.Update` and
    drawn in `BattleSceneBase.Draw`): a red dot at each combatant's `tCenter` and a green line
    showing `bDir` (facing). Built fast/minimal (token-constrained) specifically so the owner could
    self-diagnose the still-open "direction flip looks wrong on new spawns" complaint without
    another round-trip. **Only one concrete cause of that complaint was fixed** (`SpawnTrooper` was
    setting `tPos` — which triggers `UpdateRefPoints()` — before `bDir`, so the first ref-point calc
    used the wrong facing; fixed by reordering). A second theory (kaiju's own `_tCenter` not
    refreshing during attack-animation frames, since `UpdateRefPoints()` only runs from `tPos`'s
    setter) was tried as a blanket unconditional call in `TrooperUpkeep` and **reverted** — owner
    reported "looks like sprite flip is worse now." **That deeper issue is still open/unresolved**;
    the owner is now chasing it directly via the F2 overlay + Core breakpoints (which needed a fix
    too — see Gotchas).
- **`BattleSceneBase`** (`Scenes/InGame/Battlegrounds/`) is the shared battle driver — state
  machine, victory+tally, drop economy, slow-mo, cursor, music, common draw frame, teardown.
  `BattleGround_Basic` and `KaijuHunt` are thin modes over it (hooks: `SetupBattle`, `DrawHud`,
  `OnBattleOver`, `ProcessVictoryState`, `UpdateDebug`). New modes extend the base — don't copy.
- **Combat geometry is scale-relative** (`_fScale` drives `tCenter`, attack rings, and the zone
  grid governs navigation: zoned units can't leave the field; healer trips are the exception;
  unzoned units — spawn marches, kaiju entrance — roam until they step on). Off-field units are
  untargetable (int-division truncation used to leak them into edge zones). `InWeaponRange` also
  now tolerances by `nOpponent.fCombatantScale` (new virtual on `Combatant`, overridden in
  `Trooper`) — a first-pass mitigation for the scale/miss problem, now mostly superseded by the
  radial-slot rewrite above but left in place since it's harmless and still correct.
- **The lost art toolchain is rebuilt and the Peasant is live**: `tools/sprite_stitcher/stitch.py`
  turns per-frame PNG folders or GIFs into a packed sheet + the game's AnimationData JSON
  (TexturePacker frames + ActionTypes taxonomy + pack-time aliasing onto the 18 action names the
  trooper AI requests by name). Peasant is stitched, content-wired (`content/Sprite Data/Troopers/
  Peasant/`), and in the Kaiju Hunt spawn roster — first unit built this way, now the reference
  example for the next ones.
- **GIMP `.xcf` recovery pipeline is built, documented, and committed**: `tools/gimp_xcf/` (see its
  README) is a deterministic, rerunnable, git-trackable CLI toolchain (Script-Fu batch mode, driven
  via env vars, **must be invoked from Bash — hangs forever under PowerShell**, see its own
  gotchas doc) for inventorying and exporting frames out of the original artist's `.xcf` files at
  `c:\dev\art assets raw\wodyn`. A full 63-file scan is done and written up in
  `docs/DESIGN_DIRECTION.md`'s archaeology section: most files are concept art / single-image
  mockups (dragon, skeleton, wild_dog, slug, drau, Griggan_units, quartermaster, trainer,
  master_sheet, etc. — **not** extractable animated sprites), but **`bandit.xcf` and
  `sword_hero.xcf` are confirmed real numbered-frame animated units** and have already had their
  frames exported via `export_layers.scm`/`export_driver.scm`. **Neither is stitched into a
  playable unit yet** — that's the immediate next step (below).
- **Next up:**
  1. *(art path, Claude's lane per current division of labor)* Finish scanning the full-scan report
     for any other files with real numbered/sequential frame structure beyond bandit/sword_hero (in
     progress at handoff time — not yet conclusively ruled out for all 63 files). Then stitch
     bandit.xcf and/or sword_hero.xcf into playable units — likely blocked on partial action
     coverage (neither has all 18 action names Peasant had), which motivates building the
     `AnimationProcessor` fallback chain first so sparse units degrade gracefully instead of
     needing every action hand-filled.
  2. *(mechanics path, owner's lane)* Finish diagnosing the remaining direction-flip issue via the
     F2 overlay + `Attack_Kaiju` debugging; continue kaiju combat balance; once balance settles,
     surface leveling in the Camp menus + gating.
  3. Kaiju minion tiers (spawn skeleton/dragon/etc. alongside the boss) is blocked on new art —
     none of the concept-only creature `.xcf` files are usable as-is; needs either new hand-drawn
     frames from the owner's artist or an AI-assisted art pass.
  4. V6.7 Linux smoke test is the one remaining port-plan gate, untouched all session.
- Note: `CampClickables.cs` is dirty in the working tree — that's the **owner's** local edit
  (spawn-count tuning); don't commit or revert it. It's part of the current uncommitted set (below)
  but must stay excluded from any commit.
- Content pipeline (MGCB) builds clean: `dotnet build
  src/MightyFights.Desktop/MightyFights.Desktop.csproj -t:RunContentBuilder` → 328/328 entries,
  0 errors. Now that Core compiles, a full solution build succeeds too; the explicit
  `-t:RunContentBuilder` target is still the way to force just the content build.

## Uncommitted work in the tree right now

HEAD is `893b938` ("fix: kaiju balance (slots/armor) + spawn bDir ordering + F2 debug overlay").
On top of that, `git status` shows six modified files, **not yet committed** because the owner is
mid-iteration on several of them by hand:

- `Kaiju.cs` — the new `Attack_Kaiju` heuristic (owner-authored) + the `protected` field fix that
  unblocked it compiling.
- `Trooper.cs` — `_bAttacking`, `_iAttackingPos`, `_cAnimProc` promoted `private → protected`.
- `Trooper_Actions.cs` — the reverted speculative `UpdateRefPoints()` call in `TrooperUpkeep` (net
  change is just an explanatory comment; behavior is back to pre-attempt).
- `BObjectManager.cs` — `IsClickableAt` now returns `IClickable` (the matched object, or `null`),
  not `bool` — the owner changed this signature from what Claude originally wrote; respect it.
- `KaijuHunt.cs` — owner's own direct edits (e.g. `iInitialSquad` raised from 3, mid-playtest
  tuning) layered on top of Claude's session changes (immediate-charge spawn, click-guard wiring).
- `CampClickables.cs` — owner's own local tuning, unrelated to this session's feature work; never
  commit or revert this file regardless of what else is being committed.

**Do not commit any of this without asking first** — several of these files are actively being
hand-edited outside of any assistant session right now.

## Build note for debugging inside Core

The Desktop project's build/debug target in Visual Studio can silently not rebuild
`MightyFights.Core` on F5 if the solution's Configuration Manager isn't set to build Core too —
breakpoints in Core then don't bind to what's actually running. If Core changes don't seem to take
effect under the debugger, check Build > Configuration Manager's Core checkbox first; `dotnet build
MightyFights.sln` from a shell is a reliable sanity check since it always rebuilds everything
regardless of VS's per-config build state.

## Repo layout (new, alongside the untouched legacy XNA tree)

```
MightyFights.sln                    classic .sln format (not .slnx — see gotchas below)
src/MightyFights.Core/              all game code, net8.0, references MonoGame.Framework.DesktopGL + Particles directly
src/MightyFights.Desktop/           WinExe head, Program.cs only, also references the framework directly
src/MightyFights.Particles/         Mercury shim, full simulation (ProjectMercury / .Emitters / .Modifiers / .Renderers / .Serialization)
content/                            copy of the original content tree, exact relative paths preserved
content/Content.mgcb                generated (see scratchpad script mentioned in PORT_NOTES if regenerating)
content/Particles/*.xml             15 particle effect defs -- copied raw, parsed at runtime, not MGCB-built
.config/dotnet-tools.json           dotnet-mgcb 3.8.4.1 local tool manifest
docs/PORT_NOTES.md                  full decision log, one section per phase
MightyFights_Prototype/             original XNA project — untouched, kept as reference/ground-truth
```

## Open items that need a real decision (not just "keep going")

1. **WinForms debug tooling (StatsDialog/DebugData/CapCompTempEditor).** Found during Phase 1,
   stripped from `MightyFights.Core`'s build (source still intact in the legacy tree). The
   *display* (a live grid the original dev used for balance testing) has no cross-platform
   replacement yet — currently the same data dumps to a text file (`ExpData_*.txt`) instead. If
   an interactive replacement is wanted (e.g. an ImGui.NET overlay), that's new scope not in any
   phase of the plan. See `PORT_NOTES.md` → Phase 1 → "WinForms debug tooling" for full detail.
2. **You're running multiple parallel Claude threads on this branch.** At least once this session,
   a sibling thread's work (the `ExperienceTally` extraction) landed in the tree without this
   thread having seen it happen — it was caught, verified, and fixed (a reset-timing bug + a
   missing output path) before committing. If picking this up in a new thread, **check `git log`
   and `git status` against what you expect before trusting your own mental model of the tree** —
   another thread may have moved it since you last looked. (I made an unrelated near-miss this
   session too: almost deleted a pre-existing, unrelated git submodule gitlink — `MightyFightsMain`
   — while cleaning up scaffolding cruft. Caught it before committing. Worth double-checking
   `git status` output carefully rather than assuming everything unfamiliar is your own mess.)

## Gotchas worth knowing before you touch this again

- **XML comments and `--`.** Hit `error MSB4025: An XML comment cannot contain '--'` **six
  separate times** across this session, in different `.csproj` files, from writing `word -- word`
  as an em-dash substitute in a `<!-- comment -->`. Don't use `--` inside any XML/MSBuild comment,
  full stop — use `;` or a period instead. "Being more careful" didn't work across six tries;
  what actually worked was writing a real checker (extract every `<!-- -->` body, including
  multi-line, and grep it for `--`) and running it before any `dotnet` command touches a
  freshly-written project file. If picking this back up, that script is worth re-writing rather
  than trusting eyeballs again — it's about 15 lines of Python, see PORT_NOTES.md Phase 3/4.
- **`dotnet new sln` defaults to `.slnx` now** (this machine has .NET 10 SDK). Use
  `dotnet new sln -n X -o . -f sln` to force the classic format if regenerating.
- **`dotnet new tool-manifest` writes `dotnet-tools.json` at the invocation root, not
  `.config/dotnet-tools.json`.** Move it manually if regenerating from scratch.
- **`MonoGamePlatform` MSBuild property gotcha:** `MonoGame.Content.Builder.Task`'s
  `RunContentBuilder` target needs `MonoGamePlatform` set, which only comes from
  `MonoGame.Framework.DesktopGL`'s own `.targets` file — which only auto-imports into a project
  that references the package **directly**. A `ProjectReference` to Core (which has the package)
  is not enough; `MightyFights.Desktop.csproj` needs its own direct `PackageReference` too. This
  is now wired correctly — just don't "simplify" it away thinking it's redundant.
- **git-bash mangles leading-slash CLI args** (e.g. `dotnet mgcb /help` gets path-rewritten). Use
  the PowerShell tool for anything involving `/flag`-style arguments to native Windows tools.
- `Base Objects/BasicSprite.cs` still throws a non-blocking `SYSLIB0050` warning (obsolete
  `ISerializable`/`ISafeSerializationData`). Phase 5 established this is **irrelevant to save/load**
  (BasicSprite isn't in the persisted `Steward` graph), so it was left alone. If you ever wire
  `BasicSprite` into runtime binary serialization, that's when it matters — otherwise ignore it.

## How to sanity-check you're starting from a good state

```
cd C:\Users\djorl\source\repos\ddhj\MightyFights_Prototype
git status --short          # should be empty except maybe .vs/
git log --oneline -6        # should show the phase 5 commit at HEAD, or later
dotnet build MightyFights.sln 2>&1 | tail -5   # should show "Build succeeded. 0 Error(s)"
```

If any of those don't match, read `git log` and `docs/PORT_NOTES.md`'s Gate Summary table before
doing anything else — something changed since this doc was written.
