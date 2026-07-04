# HANDOFF.md

Resume-here doc for the MightyFights MonoGame port. Read this first, then
`mightyfights_monogame_port_plan.md` (the phase plan) and `docs/PORT_NOTES.md` (the detailed
decision log — every finding/deviation below is written up in full there, this file is just the
"where are we, what's next" summary). `CLAUDE.md` covers repo conventions.

**Last updated:** 2026-07-04, after the kaiju-mode build-out and BattleSceneBase consolidation.
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
- **Kaiju Hunt v1 is feature-complete**: wheel-selected spawn roster (Halberdier/Captain, costs +
  caps), mid-battle reinforcement clicks, one 2.5x-scale kaiju (fsable halberdier placeholder art),
  survivor exp banking into the persisted roster, fibonacci leveling (`LevelCurve.cs`) applied at
  spawn, drop economy (buffs drag-apply, hammer counter).
- **`BattleSceneBase`** (`Scenes/InGame/Battlegrounds/`) is the shared battle driver — state
  machine, victory+tally, drop economy, slow-mo, cursor, music, common draw frame, teardown.
  `BattleGround_Basic` and `KaijuHunt` are thin modes over it (hooks: `SetupBattle`, `DrawHud`,
  `OnBattleOver`, `ProcessVictoryState`, `UpdateDebug`). New modes extend the base — don't copy.
- **Combat geometry is scale-relative** (`_fScale` drives `tCenter`, attack rings, and the zone
  grid governs navigation: zoned units can't leave the field; healer trips are the exception;
  unzoned units — spawn marches, kaiju entrance — roam until they step on). Off-field units are
  untargetable (int-division truncation used to leak them into edge zones).
- **The lost art toolchain is rebuilt**: `tools/sprite_stitcher/stitch.py` turns per-frame PNG
  folders or GIFs into packed sheet + the game's AnimationData JSON (TexturePacker frames +
  ActionTypes taxonomy + pack-time aliasing onto the 18 action names the trooper AI requests by
  name). The original art source tree lives at `c:\dev\art assets raw\wodyn` — Peasant (stitched,
  pending content wiring), Yeoman (config away), Vintenar x2 (GIF mode), dragon/skeleton/etc. in
  .xcf (kaiju/minion art, needs scripted GIMP export).
- **Next up (owner-agreed order):** (4) wire the stitched Peasant into MGCB content + the spawn
  roster; then GIMP batch export (dragon = real kaiju art), kaiju minion tiers, leveling surfaced
  in the Camp menus + gating. Fibonacci leveling curve and template-level progression are settled
  decisions (see DESIGN_DIRECTION).
- Note: `CampClickables.cs` may be dirty in the working tree — that's the **owner's** local edit
  (spawn-count tuning); don't commit or revert it.
- Content pipeline (MGCB) builds clean: `dotnet build
  src/MightyFights.Desktop/MightyFights.Desktop.csproj -t:RunContentBuilder` → 328/328 entries,
  0 errors. Now that Core compiles, a full solution build succeeds too; the explicit
  `-t:RunContentBuilder` target is still the way to force just the content build.

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
