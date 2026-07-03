# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A 2D tactics/RPG prototype ("MightyFights") built on **XNA 4.0 / VS2010 / .NET 4.0 Client Profile / x86 / HiDef**. There is an active plan to port it to MonoGame 3.8.x DesktopGL on .NET 8 (and later a KNI BlazorGL web head) — see **`mightyfights_monogame_port_plan.md`** at the repo root. That plan is the source of truth for anything related to modernization work: phase order, exit gates, known API landmines, and the risk register. Read it before starting any port-related task, and keep `docs/PORT_NOTES.md` (created during the port) updated as decisions are made.

Until the port lands, the code in `MightyFights_Prototype/` is genuine XNA 4.0, not MonoGame — don't assume MonoGame-only APIs are available unless you're working inside the port itself.

## Build

There is no modern build command here. This solution targets .NET Framework 4.0 Client Profile via the legacy XNA 4.0 toolchain and requires **Visual Studio 2010 + XNA Game Studio 4.0** (or an equivalent old MSBuild toolset) to build — `dotnet build` will not work against it. Content (`MightyFights_PrototypeContent.contentproj`) is built through the XNA content pipeline, not MGCB.

- `MightyFights_Prototype.sln` — the real solution: `MightyFights_Prototype` (game), `MightyFights_PrototypeContent` (content pipeline project), `AnimationDataEx` (custom content importer/processor/writer), `MightyFights_Support` (shared/animation-data library), `MightyFights_DebugPipeline`.
- `Utilities/Utilities.sln` — standalone WinForms tools (`Sprite Color Swapper`, `Sprite Color Map Creator`), unrelated to the game build and explicitly out of scope for the port.

There is no test suite in this repo.

## Architecture (current XNA codebase)

**Entry point:** `MightyFights_Prototype/MightyFights_Prototype/GameShell.cs` (not `Game1.cs`) — a `Microsoft.Xna.Framework.Game`. `Initialize()` wires up the `DataStore` singleton, calls `Steward`/campaign setup (`InitNew()` or hardcoded debug data), initializes `DataManager`, constructs a `SceneManager` game component, sets up the custom Win32 input hook, and pushes the initial `TitleScreen` scene. `GameShell.Update`/`Draw` are near-empty XNA boilerplate — real per-frame work happens inside `SceneManager`.

**Scene stack:** `Management Classes/SceneManager.cs` is a `DrawableGameComponent` holding scenes in a **stack** (`List<IGameScene>`) — only the top scene updates/draws; `AddScene` pushes and deactivates the previous top, `RemoveScene` pops and reactivates the new top. Scenes implement `Interfaces/IGameScene.cs` directly (no common base class). Scenes live under `Scenes/OutGame/TitleScreen` and `Scenes/InGame/{Camp,Map,Battlegrounds/Basic}`; larger scenes are split into `partial class` files (e.g. `Camp.cs` + `Camp_Events.cs`) and delegate to per-scene helper classes under their own `Managers/` folders (`CObjectManager`, `CMenuManager`, `BObjectManager`, `BMenuManager`, etc.).

**Content/object factory:** `Management Classes/DataManager.cs` is a singleton (`cInstance`) sitting on top of `ContentManager`. It caches loaded assets (`AnimationData`, `Texture2D`, `ParticleEffect`, `SoundEffect`, `Song`) in per-type dictionaries keyed by asset path, and exposes `Create*` factory methods that both load content and construct wired-up game objects (`CreateTemplate`, `CreatePriest`, `CreateBuff`, `CreateParticleSystem`, `CreateSfx`, etc.) rather than exposing raw `Load<T>` calls to callers.

**Campaign/save state:** `Management Classes/Steward.cs` (`[Serializable]`) holds the persisted campaign/roster state — templates/captains, buffs, and companies (with lookup dictionaries rehydrated post-deserialize via `HydrateCompanyRefLists()`). `Management Classes/DataStore.cs` is the top-level singleton holding two `Steward`s (`cLSteward` player / `cRSteward` enemy), all XNA service references (content/graphics/scene manager/random/font), global gameplay toggles, and battle-scoped data. Persistence is `DataStore.SaveData/LoadData` → `IsolatedStorageFile` ("SaveData.waf") serialized with the third-party `fastJSON` (`3rd Party/fastJSON.dll`) — this and the isolated-storage approach are slated for replacement in the port (Phase 5 of the port plan).

**Input/actions:** Despite the filename, `Management Classes/InputManager.cs` actually defines a static `InputSystem` class in namespace `Microsoft.Xna.Framework.Input` — a Win32 window-proc hook exposing higher-level input events (`KeyDown/Up`, `CharEntered`, `MouseDown/Up/Move/Hover/Wheel/DoubleClick`) instead of raw XNA polling. Per-entity behavior (combat/animation state machines) is driven separately by `Support Classes/ActionManager.cs` — a generic `ActionManager<T>` action-queue/scheduler subclassed per entity type (`TrooperActMgr`, `HealerActMgr`), where each queued `Action` wraps a `DActionHeuristic` delegate evaluated once per frame.

**Namespaces:** almost everything in the main game project lives in one flat `MightyFights_Prototype` namespace regardless of folder depth — folder nesting is organizational only, not namespace-reflected. The support library is `MightyFights_Support`; each sibling tool project (`AnimationDataEx`, `MightyFights_DebugPipeline`, the WinForms utilities) has its own top-level namespace.

**Particles:** effect definitions live as XNA `IntermediateSerializer` XML under content (`Particles/*.xml`), driven through the binary-only, XNA-linked Mercury Particle Engine 3.1 (`3rd Party/Mercury Particle Engine 3.1 for XNA 4.0 (Binaries)`) — dead upstream, and the whole thing is being replaced by an in-repo shim (`MightyFights.Particles`) per Phase 3–4 of the port plan. Only ~17 files touch `ProjectMercury` types and the actual API surface used is small (documented in the port plan §3).

## Conventions to preserve (do not "clean up")

- Hungarian-style prefixes throughout (`c`=object ref, `s`=string, `i`=int, `f`=float, `b`=bool, `e`=enum, `t`=Vector/Point/struct, `n`=new instance, `d`=delegate).
- Tabs for indentation; braces often on the same line as the control statement.
- Singleton via a static `cInstance` property + private constructor (`DataStore`, `DataManager`) — this is the established pattern, not something to refactor away.
- `partial class` splits for large scenes (`X.cs` main logic + `X_Events.cs` handlers).
- Comments tagged `//// ddhj:` are the original author's own notes/TODOs — leave them, don't strip them during edits.
- A project-local `Interfaces/IDrawable.cs` intentionally collides with `Microsoft.Xna.Framework.IDrawable`; existing using-aliases disambiguate this deliberately.
- Preserving this style matters specifically for the ongoing port: an agentic, phase-gated port relies on minimal, greppable diffs, and reformatting creates diff noise that defeats reviewability (see port plan §6).
