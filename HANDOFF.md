# HANDOFF.md

Resume-here doc for the MightyFights MonoGame port. Read this first, then
`mightyfights_monogame_port_plan.md` (the phase plan) and `docs/PORT_NOTES.md` (the detailed
decision log — every finding/deviation below is written up in full there, this file is just the
"where are we, what's next" summary). `CLAUDE.md` covers repo conventions.

**Last updated:** 2026-07-03, end of Phase 3 work.

## Current state

- **Branch:** `monogame-port` (tag `pre-port` marks the commit before any port work started).
- **Phases 0, 1, 2, 3 are done** (commits `855c811`, `c4b38f2` + fixup `cadd4f3`, `2e24b88`, and
  the Phase 3 commit — check `git log --oneline` for the exact hash if not shown here).
- **Next up: Phase 4 — Particle Shim: Effect Loading & Simulation.** The shim project
  (`src/MightyFights.Particles/`) exists with the correct namespace-preserving API surface
  (`ProjectMercury.ParticleEffect`/`ParticleEffectManager`, `ProjectMercury.Emitters.Emitter`,
  `ProjectMercury.Renderers.Renderer`/`SpriteBatchRenderer`) and compiles clean, but simulation
  internals are still no-ops/stubs (T3.2 scope was API definition only, not simulation — that's
  explicitly T4.1-T4.4). Phase 4 needs to: parse the 15 particle XML files (inventory already
  done in PORT_NOTES Phase 0: CircleEmitter/ConeEmitter/LineEmitter/RectEmitter, 12 modifier
  types — **no PointEmitter**, contrary to the plan's original guess), implement those emitter
  types + modifiers, and wire real per-particle simulation into `Emitter`/`ParticleEffect.Update`/
  `SpriteBatchRenderer.RenderEffect`.
- **The solution is 1 error away from building clean.** `dotnet build MightyFights.sln` now fails
  with **exactly 1 `CS0246` error**: `fastJSON` in `DataStore.cs` (persistence, Phase 5 scope).
  All Mercury-related errors are gone as of Phase 3. If a future build shows Mercury errors again,
  or more than 1 total error, something regressed — check `git log`/`git status` first.
- Content pipeline (MGCB) builds clean independently: `dotnet build
  src/MightyFights.Desktop/MightyFights.Desktop.csproj -t:RunContentBuilder` → 328/328 entries,
  0 errors. (A full solution build still can't reach this target — MSBuild aborts on Core's one
  remaining compile error before Desktop's content-build target runs. Will resolve itself once
  Phase 5 closes the `fastJSON` gap.)

## Repo layout (new, alongside the untouched legacy XNA tree)

```
MightyFights.sln                    classic .sln format (not .slnx — see gotchas below)
src/MightyFights.Core/              all game code, net8.0, references MonoGame.Framework.DesktopGL + Particles directly
src/MightyFights.Desktop/           WinExe head, Program.cs only, also references the framework directly
src/MightyFights.Particles/         Mercury shim (ProjectMercury/.Emitters/.Renderers namespaces) -- API done, simulation is Phase 4
content/                            copy of the original content tree, exact relative paths preserved
content/Content.mgcb                generated (see scratchpad script mentioned in PORT_NOTES if regenerating)
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

- **XML comments and `--`.** I hit `error MSB4025: An XML comment cannot contain '--'` **five
  separate times** this session, in different `.csproj` files, from writing `word -- word` as an
  em-dash substitute in a `<!-- comment -->`. Just don't use `--` inside any XML/MSBuild comment,
  full stop — use `;` or a period instead. Genuinely stop and grep for it before running any
  `dotnet` command against a `.csproj` you just wrote.
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
- Both `Base Objects/BasicSprite.cs` (SYSLIB0050, obsolete binary serialization) and the general
  shape of `Steward`'s object graph are flagged as risk **R2** evidence for Phase 5's
  `System.Text.Json` round-trip test — worth re-reading that PORT_NOTES section before starting
  Phase 5, don't rediscover it from scratch.

## How to sanity-check you're starting from a good state

```
cd C:\Users\djorl\source\repos\ddhj\MightyFights_Prototype
git status --short          # should be empty except maybe .vs/
git log --oneline -6        # should show the phase 3 commit at HEAD, or later
dotnet build MightyFights.sln 2>&1 | tail -5   # should show "1 Error(s)" (fastJSON only) and nothing else
```

If any of those don't match, read `git log` and `docs/PORT_NOTES.md`'s Gate Summary table before
doing anything else — something changed since this doc was written.
