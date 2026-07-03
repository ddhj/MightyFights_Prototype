# MightyFights_Prototype → MonoGame Port Plan

**Version:** v0.1 draft
**Source:** github.com/ddhj/MightyFights_Prototype (XNA 4.0, VS2010, .NET 4.0 Client Profile, x86, HiDef)
**Primary target:** MonoGame 3.8.x DesktopGL on .NET 8 (Windows/Linux/macOS)
**Secondary target (Phase 7):** KNI BlazorGL (WebAssembly / WebGL2, browser)
**Execution model:** Agentic (oriel_loom / Claude Code). Each phase has conjunctive exit gates; do not begin phase N+1 until all gates of phase N pass.

---

## 0. Goals and Non-Goals

**Goals**
- G1: Game builds and runs on .NET 8 + MonoGame DesktopGL with behavior parity to the XNA original (title → camp → map → battleground loop functional).
- G2: All game code stays inside the XNA 4.0 API surface (SpriteBatch, ContentManager, SoundEffect, Song, Vector2/Rectangle/Color, GameTime, Mouse/Keyboard) so the KNI web head is a project-file swap, not a code fork.
- G3: Zero binary-only dependencies. Mercury Particle Engine, fastJSON.dll, and the custom XNA content pipeline extension are all removed or replaced with source in-repo.
- G4: Deterministic, verifiable phases suitable for agentic execution.

**Non-Goals**
- No gameplay changes, no rebalancing, no new features.
- No renderer modernization (stays SpriteBatch immediate-mode, fixed 1024×576 backbuffer).
- No performance work until parity is verified (correct before optimized).
- Web head is scaffolded and boots, but web polish (load-time optimization, atlas repacking, AOT tuning) is a follow-on project.

---

## 1. Current-State Facts (verified against repo)

| Aspect | Finding |
|---|---|
| Code | ~15.2K LOC C#, 92 files, 6 projects in solution |
| Game project | `MightyFights_Prototype/MightyFights_Prototype/` — the only project that ships |
| Support lib | `MightyFights_Support/` — `AnimDataReader` (XNB ContentTypeReader for AnimationData) + shared classes. Referenced by game. |
| Pipeline ext | `AnimationDataEx/` — XNA ContentImporter/Processor/Writer converting TexturePacker JSON → XNB. Uses `System.Web.Script.Serialization` (removed from modern .NET). **Delete; replace with runtime JSON loading.** |
| Debug/utility projects | `MightyFights_DebugPipeline/`, `Utilities/Sprite Color Swapper` (WinForms), `Utilities/Sprite Color Map Creator`. **Out of scope for the port; exclude from new solution.** |
| Content | 303 PNG, 22 MP3, 3 .spritefont, 12 TexturePacker JSON (source data present in repo — pipeline bypass is possible), 15 Mercury particle-effect XMLs |
| Content loads | 126× `Load<Texture2D>`, 17× `Load<AnimationData>`, 10× `Load<SpriteFont>`, 2× `Load<ParticleEffect>`, 1× `Load<Song>`, 1× `Load<SoundEffect>` (plus SFX loaded via `DataManager.CreateSfx` path) |
| Particles | Mercury Particle Engine 3.1, **binary-only, XNA-linked, dead project.** Used in 17 files but the *actual* API surface is small (see §4). Effect definitions are XNA IntermediateSerializer XML in `Content/Particles/*.xml`. |
| Persistence | `DataStore.SaveData/LoadData` via `IsolatedStorageFile` + `fastJSON.dll` → single-line JSON in `SaveData.waf` |
| Framework refs | GamerServices, Avatar, Net, Xact referenced in csproj but **zero call sites** (usings only). Delete. |
| Input | Mouse + keyboard only. 1 GamePad reference (verify and remove/guard). Good for web. |
| Backbuffer | Fixed 1024×576 windowed, HiDef profile |
| Music | OC ReMix Final Fantasy arrangements. **Fine for personal builds; must be replaced before any public/browser distribution.** |

---

## 2. Target Repository Structure

```
MightyFights/
├─ MightyFights.sln
├─ src/
│  ├─ MightyFights.Core/            # ALL game code (netstandard2.0 or net8.0 lib)
│  │                                 #  - everything from MightyFights_Prototype + MightyFights_Support merged
│  │                                 #  - references framework via the head project (bait-and-switch pattern)
│  ├─ MightyFights.Desktop/          # MonoGame.Framework.DesktopGL head; Program.cs only
│  ├─ MightyFights.Web/              # (Phase 7) KNI BlazorGL head; Blazor host page + Program
│  └─ MightyFights.Particles/        # Mercury replacement shim (pure managed, no native deps)
├─ content/
│  ├─ Content.mgcb                   # textures, spritefonts, audio
│  └─ raw/                           # TexturePacker JSONs + particle XMLs copied as raw assets
└─ docs/
   └─ PORT_NOTES.md                  # running decision log (agent maintains)
```

Simplification option: if the Core/head split fights the tooling, collapse to a single `MightyFights.Desktop` project for Phases 1–6 and split when the web head lands. Splitting early is preferred; it enforces G2.

---

## 3. Phase Plan

### Phase 0 — Baseline & Inventory
- T0.1 Clone repo; tag `pre-port` on a new branch `monogame-port`.
- T0.2 Generate an asset manifest: every `Content.Load<...>` call site → (type, asset path). Emit `docs/asset_manifest.csv`. (157 call sites expected.)
- T0.3 Audit texture dimensions: `identify` or ImageSharp over all PNGs; flag anything > 2048px (WebGL2 comfort ceiling; HiDef desktop is fine to 4096+). Flag list goes in PORT_NOTES.
- T0.4 Record the exact Mercury API surface by grep (expected surface documented in §4 — verify, don't assume).

**Gates:** manifest exists and its asset count reconciles with grep count; texture audit complete.

### Phase 1 — Solution Modernization
- T1.1 New SDK-style solution per §2. .NET 8, AnyCPU, nullable disabled (legacy code), LangVersion latest.
- T1.2 Copy game sources: `MightyFights_Prototype/MightyFights_Prototype/**/*.cs` + `MightyFights_Support/*.cs` (excluding `AnimDataReader`'s ContentTypeReader plumbing — see Phase 2) into `MightyFights.Core`.
- T1.3 Add `MonoGame.Framework.DesktopGL` (latest 3.8.x) to the Desktop head.
- T1.4 Delete all `using Microsoft.Xna.Framework.GamerServices;` / Avatar / Net / Storage usings. Remove the single GamePad call site (or guard `#if !BLAZORGL`).
- T1.5 Do **not** attempt to compile clean yet; expected failures are Mercury (Phase 3–4), AnimationData (Phase 2), IsolatedStorage/fastJSON (Phase 5). Stub with `#error PHASE_N` markers so remaining work is greppable.

**Gates:** solution restores; failure list consists *only* of Mercury, AnimationData, and persistence symbols (any other error class must be dispositioned in PORT_NOTES first).

### Phase 2 — Content Pipeline Replacement
- T2.1 Build `Content.mgcb`: all PNGs (Texture2D), 3 spritefonts, MP3s (Song for music; battle SFX as SoundEffect — mirror original DataManager usage), preserving the original folder-relative asset paths exactly (the code addresses content by string paths like `Sprite Data\Troopers\Halberd\...`; normalize `\` → `/` in one shared helper, not at 157 call sites).
- T2.2 Kill `AnimationDataEx` and the XNB route for animation data. Rewrite `AnimDataReader`/`AnimationData` as a runtime loader: parse the TexturePacker JSON (present in `content/raw/`) with `System.Text.Json` into the existing `AnimationData` structure. Replace the 17 `Content.Load<AnimationData>(path)` calls via a `DataManager` helper (`LoadAnimationData(path)`) so call-site churn is minimal — DataManager already caches these in a dictionary.
- T2.3 Spritefont check: XNA `.spritefont` XML is MGCB-compatible; verify the 3 fonts reference fonts available on the build machine or swap to bundled TTFs.

**Gates:** MGCB builds with zero errors; a unit test loads every entry in the asset manifest through ContentManager/JSON loader without exception (headless where possible, or a boot-time asset-sweep debug mode).

### Phase 3 — Particle Shim: API Definition
Recreate **only** the Mercury surface actually consumed (verified by grep):

```
namespace ProjectMercury (keep namespace to avoid touching 17 files)
  class ParticleEffect : List<Emitter>-like
      void Initialise();
      void LoadContent(ContentManager);       // resolves each Emitter.ParticleTextureAssetName
      void Update(float elapsedSeconds);
      void Trigger(Vector2 position);
      ParticleEffect DeepCopy();              // verify if used; DataManager caches suggest it may be
  class Emitter
      string Name, ParticleTextureAssetName;
      float Term;
      (Budget, ReleaseQuantity/Speed/Colour/Opacity/Scale, Modifiers as data)
namespace ProjectMercury.Renderers
  class SpriteBatchRenderer
      GraphicsDeviceService { set; }
      void LoadContent(ContentManager);
      void RenderEffect(ParticleEffect, ref Matrix)   // verify exact overload used
```

- T3.1 Verify exact call sites/overloads (esp. `RenderEffect` signature and any `DeepCopy`/clone use in `DataManager` + `TerminatingParticleEffect` in `Particle System Objects.cs`).
- T3.2 Write the shim skeleton in `MightyFights.Particles` — pure managed, MonoGame-types only. CPU-simulated particles + SpriteBatch rendering is sufficient at this game's scale (budgets ~500).

**Gates:** game project compiles against the shim with Mercury binaries deleted from the tree.

### Phase 4 — Particle Shim: Effect Loading & Simulation
- T4.1 Parser for the 15 `Content/Particles/*.xml` files (XNA IntermediateSerializer format, `Emitters:`/`Modifiers:` namespaces). Two acceptable strategies: (a) direct XDocument parse into shim types; (b) one-time conversion tool XML → JSON, runtime loads JSON. Prefer (a) — fewer artifacts, XMLs are small and regular.
- T4.2 Implement emitter types found in the XMLs (inventory them first — expect CircleEmitter, PointEmitter, maybe ConeEmitter) and the modifier set actually present (opacity/scale/colour interpolators, gravity/damping if present). Implement only what the 15 files use.
- T4.3 Wire the 4 triggered effects (BloodSpray, HealingCircle, HealerRecharge, Buff Sparkle) + the 2 `Load<ParticleEffect>` paths through DataManager.
- T4.4 Visual parity is best-effort, not pixel-exact: acceptance is "effect triggers at the right place/time, roughly right look, terminates correctly" (Term/TerminatingParticleEffect semantics must be exact — gameplay code may depend on effect completion).

**Gates:** all 15 effect XMLs parse; the 4 trigger sites fire visibly in-game; no unbounded particle growth over a 10-minute soak.

### Phase 5 — Persistence & Serialization
- T5.1 Replace `IsolatedStorageFile` with a `SaveStorage` abstraction: desktop = `Environment.SpecialFolder.ApplicationData/MightyFights/SaveData.json`; web head later = browser localStorage via KNI/JS interop. Single interface, two implementations (G2 discipline).
- T5.2 Replace `fastJSON` with `System.Text.Json`. **Risk:** existing `Steward` object graph may have cycles, private fields, or polymorphism fastJSON tolerated. Disposition: write round-trip test (`InitNew()` → save → load → deep-compare). Old `.waf` save migration is explicitly out of scope.
- T5.3 Remove `fastJSON.dll` and `3rd Party/` folder from the tree.

**Gates:** round-trip test passes; solution compiles with **zero** references to fastJSON, System.Web, IsolatedStorage, or Microsoft.Xna binary assemblies.

### Phase 6 — Runtime Parity Verification
Full compile is expected green entering this phase; now verify behavior scene by scene.

- V6.1 Title screen renders, music plays, click-through works.
- V6.2 Camp scene: menus open (Company, Knight, Template editor), sliders/stat displays functional, clickables respond.
- V6.3 Map scene: buildings render, building menus open, action icons (attack/infiltrate/etc.) respond, enemy icons animate.
- V6.4 Battleground: troopers spawn from templates, animate (TexturePacker frames correct — this validates Phase 2 end-to-end), combat resolves, damage numbers animate, heals fire, crits play SFX, particle effects trigger, flee/heal thresholds behave, battle end returns to map.
- V6.5 Save/load mid-campaign round-trips.
- V6.6 Soak: 15 minutes of battleground without crash, leak (watch GC/heap), or audio drop.
- V6.7 Cross-platform smoke: build + boot on Linux (DGX Spark makes this convenient) in addition to Windows.

**Gates:** all of V6.1–V6.7. This is the port's definition of done for G1.

### Phase 7 — Web Head (KNI BlazorGL)
- T7.1 `dotnet new install nkast.Kni.Templates` → scaffold `kni-blazor-gl` head; reference `MightyFights.Core`; swap framework packages to `nkast.Xna.Framework.*` for this head only.
- T7.2 Content for web: KNI has its own content build; reuse Content.mgcb via KNI's pipeline. Confirm MP3/Song support in BlazorGL backend; if Song is unsupported, music routes through an HTML5 `<audio>` interop shim (isolated behind the same abstraction as SaveStorage).
- T7.3 SaveStorage web implementation → localStorage.
- T7.4 Gate first audio behind first user input (browser autoplay policy).
- T7.5 AOT publish (`RunAOTCompilation=true`, `WasmStripILAfterAOT=false`), measure first-load; record but do not optimize further in this project.
- T7.6 Textures flagged >2048 in T0.3: downscale or split for the web head only if WebGL2 rejects them.

**Gates:** game boots in Chrome + Firefox, reaches battleground, saves persist across reload. (Music licensing must be resolved before this build goes anywhere public — see §5.)

---

## 4. Known API Deltas / Landmines (XNA 4.0 → MonoGame 3.8)

1. `Song`/`MediaPlayer` MP3 support exists but codec behavior differs per platform; prefer OGG re-encode if MP3 misbehaves on DesktopGL.
2. Project-local `Interfaces/IDrawable.cs` collides with `Microsoft.Xna.Framework.IDrawable` — original code already disambiguates; preserve existing using-aliases, don't "clean up."
3. MonoGame content paths are case-sensitive on Linux; asset manifest (T0.2) must match file casing exactly.
4. `SpriteBatch.Begin` default sort/blend semantics match XNA, but verify any nondefault Begin calls against MonoGame docs.
5. `Random` seeding / float determinism: not a concern (no lockstep networking).
6. x86 → AnyCPU: remove any `processorArchitecture=x86` assumptions; none expected in code.
7. Tabs/`sHungarian` naming throughout: preserve style, do not reformat (diff noise kills reviewability of an agentic port).

## 5. Risk Register

| # | Risk | Likelihood | Mitigation |
|---|---|---|---|
| R1 | Mercury XMLs use emitters/modifiers beyond the shim's initial set | Med | T4.2 inventories XMLs *before* implementation; implement to inventory |
| R2 | `Steward` graph doesn't survive System.Text.Json | Med | Round-trip test in T5.2; fall back to Newtonsoft.Json (`ReferenceHandler`/field support) if needed |
| R3 | TexturePacker JSON schema drift vs. `AnimDataImporter` expectations (rotated/trimmed frames) | Med | Port the field mapping directly from `AnimationDataEx/SupportClasses.cs` (TPFrame etc.), which is the ground-truth schema |
| R4 | Spritefonts reference fonts absent on Linux build hosts | Low | Bundle TTFs in repo |
| R5 | KNI Song/audio gaps in browser | Med | Audio abstraction seam (T7.2); HTML5 audio fallback |
| R6 | OC ReMix music in a public web build | Certain if shipped | Personal builds only until replaced; tracked as release blocker, not port blocker |
| R7 | Behavioral drift hidden by "it compiles" | Med | Phase 6 is mandatory and scene-exhaustive; no gate skipping |

## 6. Notes for Agentic Execution (oriel_loom)

- Phases are strictly ordered; gates are conjunctive. A failed gate halts the phase and emits a disposition entry in `docs/PORT_NOTES.md` before any workaround is attempted.
- Prefer minimal-diff edits inside game code; concentrate new code in `MightyFights.Particles`, the JSON loaders, and `SaveStorage`. The 17 Mercury-touching files should ideally see **zero** edits (namespace-preserving shim).
- Every phase ends with a commit on `monogame-port` named `port: phase N — <summary>`, plus updated PORT_NOTES.
- Anything discovered that contradicts §1's facts gets logged before proceeding — the facts table is the spec's ground truth and must stay true.
