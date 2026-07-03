# HANDOFF.md

Resume-here doc for the MightyFights MonoGame port. Read this first, then
`mightyfights_monogame_port_plan.md` (the phase plan) and `docs/PORT_NOTES.md` (the detailed
decision log — every finding/deviation below is written up in full there, this file is just the
"where are we, what's next" summary). `CLAUDE.md` covers repo conventions.

**Last updated:** 2026-07-03, end of Phase 4 work.

## Current state

- **Branch:** `monogame-port` (tag `pre-port` marks the commit before any port work started).
- **Phases 0, 1, 2, 3, 4 are done.** Check `git log --oneline` for exact commit hashes.
- **Next up: Phase 5 — Persistence & Serialization.** Replace `IsolatedStorageFile` with a
  `SaveStorage` abstraction (desktop = `%AppData%`, web = localStorage later) and `fastJSON` with
  `System.Text.Json`, behind a round-trip test (`InitNew()` → save → load → deep-compare). Risk
  **R2** (does `Steward`'s object graph survive `System.Text.Json`?) has concrete supporting
  evidence already logged in Phase 1's section — `BasicSprite` uses obsolete
  `ISafeSerializationData`/`ISerializable` binary-serialization interfaces. Read that before
  starting, don't rediscover it.
- **The solution is 1 error away from building clean and has been since Phase 3.**
  `dotnet build MightyFights.sln` fails with **exactly 1 `CS0246` error**: `fastJSON` in
  `DataStore.cs`. That's the *only* remaining gap before the whole ~15K-LOC game project compiles
  — Phase 5 is genuinely the last blocker to a clean build. If a future build shows more than 1
  error, or any Mercury-related error, something regressed — check `git log`/`git status` first.
- **Particle simulation is real now** (`src/MightyFights.Particles/`), not stubs: 5 emitter
  shapes (base `Emitter` + Circle/Cone/Line/Rect), 16 modifier types, a real XML parser
  (`Serialization/ParticleEffectXmlLoader.cs`), verified against all 15 actual particle XML files
  via an isolated smoke test (parses, simulates, particles expire on schedule, particle counts
  stay bounded by `Budget` even under sustained repeated triggering). Not yet verified
  *in-game* (the game can't boot until Phase 5 closes the last gap) — that's Phase 6's job
  (V6.4 already covers it).
- Content pipeline (MGCB) builds clean independently: `dotnet build
  src/MightyFights.Desktop/MightyFights.Desktop.csproj -t:RunContentBuilder` → 328/328 entries,
  0 errors. (A full solution build still can't reach this target — MSBuild aborts on Core's one
  remaining compile error before Desktop's content-build target runs. Resolves itself once Phase 5
  closes the `fastJSON` gap.)

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
- Both `Base Objects/BasicSprite.cs` (SYSLIB0050, obsolete binary serialization) and the general
  shape of `Steward`'s object graph are flagged as risk **R2** evidence for Phase 5's
  `System.Text.Json` round-trip test — worth re-reading that PORT_NOTES section before starting
  Phase 5, don't rediscover it from scratch.

## How to sanity-check you're starting from a good state

```
cd C:\Users\djorl\source\repos\ddhj\MightyFights_Prototype
git status --short          # should be empty except maybe .vs/
git log --oneline -6        # should show the phase 4 commit at HEAD, or later
dotnet build MightyFights.sln 2>&1 | tail -5   # should show "1 Error(s)" (fastJSON only) and nothing else
```

If any of those don't match, read `git log` and `docs/PORT_NOTES.md`'s Gate Summary table before
doing anything else — something changed since this doc was written.
