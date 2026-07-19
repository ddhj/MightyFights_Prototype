# VILLAGE_MODE.md

Guide for **Village** — the settlement/economy game mode, selectable from the mode menu. Built
2026-07-18 as a **mock** from the designer's two raw docs (`docs/my_vision.txt`, `docs/food.txt`).
It reuses the existing combat engine for the fights and is deliberately un-tuned (the docs say to
make it entertaining, not balanced). This file is the "what it is, how to change it" doc so the
mode can be iterated — especially the **art** — without re-reading the code.

It's built on the exact pattern as War Battle (`docs/WAR_BATTLE_DESIGN.md`): a standalone strategic
`IGameScene`, plus an engine-backed battle pushed onto the scene stack whose result is polled back.
Read that doc if you want the deeper "why this maps onto the engine" reasoning; it all applies here.

## 1. What it is

Found a village in a region, pick two vocations, then play a **weekly turn loop**. Food is produced
each week as a *percentage of your population's need*; surplus sells at Market for coin. Every week
your livestock may be **raided** — and defending a raid drops into a **real field battle** on the
combat engine. Survive; if food or population hits zero, the village dies.

The whole thing is a compressed take on `my_vision.txt` (the region → vocation front-end) welded to
`food.txt` (the food/predation/market weekly economy).

## 2. Where the code lives

Everything is under `src/MightyFights.Core/Scenes/InGame/Village/` (plus one line in ModeSelect):

| File | Role |
|---|---|
| `VillageTypes.cs` | Data model: `EVocation`, `ETerrain`, the `VillageRules.cDefs` vocation table (straight from the two docs), `Region`, `PredationEvent`, and the `VillageRaidPlan`/`VillageRaidResult` pair passed across the scene-stack push. |
| `Village.cs` | The strategic `IGameScene`. Six phases (`Region → Vocations → Weekly → Predation → Resolving → GameOver`), all input/draw, and the entire food/market/population economy. |
| `VillageRaid.cs` | A `BattleSceneBase` mode (like `KaijuHunt`/`WarClash`): spawns the muster vs the raiders as real troopers, runs to a wipe, writes the outcome into the shared result before teardown. |
| `Scenes/OutGame/ModeSelect/ModeSelect.cs` | The "Village" menu entry (`_saEntries` + `case 3` news up `Village` and pushes it). |

## 3. The flow

1. **Region** (`region_map` backdrop) — 5 clickable region markers. Terrain (pasture/fields/woods/
   mountains/water) gates which vocations are selectable.
2. **Vocations** (`vocations_bg` backdrop) — pick **2 of 12**. Hover shows food %, raid chance,
   market good, perk. Food vocations (Hogs 300% … Orchards 25%) drive the economy; the rest just
   trickle coin + flavor. → FOUND VILLAGE.
3. **Weekly** (`village_bg` backdrop, `skyline` title band) — stats rail on the left; buttons on the
   right: **REPLENISH HERDS** (pay coin to restore raided herds), **FORAGE** (one-shot +food/week),
   **NEXT WEEK** (with a yes/no confirm).
4. **Predation** — ending a week rolls each food vocation's raid chance. A hit → *a pack of Trolls /
   a Hungry Giant approaches your [Hogs]* → **DEFEND** or **IGNORE**.
   - **DEFEND** pushes `VillageRaid` (a live battle). Per `food.txt`, fighting **protects that herd's
     production even in defeat**; winning also lowers its future raid chance; the fallen muster costs
     you villagers.
   - **IGNORE** halves that herd's production.
5. **Resolving** — the battle runs; when it pops, `Village.Update` polls the result and applies it.
6. Yield resolves (consume need; famine/emigration if short; sell surplus; spoilage), then **lose if
   food or population hits zero**. Survive past week 8 → the village **thrives** (a soft win so the
   mock has an ending).

## 4. Swapping the art  ← the designer's main lever

The four backdrops are ordinary PNGs compiled through the MonoGame content pipeline (MGCB). The
Village scene loads them **by name**, so swapping an image needs **no code change**.

Files (in `content/In Game/Village/`), and where each shows up:

| PNG | Screen | Source image it came from |
|---|---|---|
| `region_map.png` | Region pick | `AImap.jpg` |
| `vocations_bg.png` | Vocation pick | `village art.jpg` |
| `village_bg.png`  | The weekly village screen | `fort art.jpg` |
| `skyline.png`     | Title band across the top of the village screen | `village icons3.jpg` |

**To replace one:**
1. Drop the new image in `content/In Game/Village/` with the **same filename** (convert to `.png`
   first — the pipeline is set to `TextureImporter`/`TextureProcessor`; a `.jpg` won't match the
   `.png` entry in `Content.mgcb`).
2. Rebuild content:
   `dotnet build src/MightyFights.Desktop/MightyFights.Desktop.csproj -t:RunContentBuilder`
   (from Bash or PowerShell at the repo root). A full `dotnet build MightyFights.sln` also works.
3. Re-run the game. That's it.

**Adding a *new* image** (not replacing) additionally needs a 4-line block in
`content/Content.mgcb` (copy any existing `#begin … TextureImporter … /build:` block and change the
path), then a `Content.Load<Texture2D>(@"In Game\Village\your_name")` call in `Village.cs:Init`.

Notes on how they're drawn (in case something looks cropped):
- All backdrops are **cover-scaled** to the 1024×576 window (`DrawCover` in `Village.cs`) — aspect
  preserved, overflow cropped top/bottom. The source art is 1152×896, so ~110px is cropped off the
  top and bottom. If a composition (e.g. `fort art`'s side panels) needs to sit fully in frame,
  either re-crop the source to 16:9 (1024×576 / 1280×720) before dropping it in, or ask and I'll
  switch `DrawCover` to letterbox-fit instead of cover.
- Text sits on semi-transparent dark panels for legibility over busy art; if a new backdrop needs
  the panels moved, that's a code tweak — flag it.

If the designer wants to iterate on visuals *without* the rebuild step each time, say so — the scene
can be switched to load these four from a plain folder at runtime (`Texture2D.FromStream`), making
them hot-swappable drop-ins. It's a small, contained change to `Village.cs:Init` only.

## 5. The raid battle: drops & buffs (inherited from the combat engine)

A `VillageRaid` **is** a `BattleSceneBase`, so it comes with the engine's full drop economy for free —
the same one Skirmish and Kaiju Hunt use. Nothing here is Village-specific code; this section just
documents what the player sees during a DEFEND fight, since the designer asked.

**How drops appear.** Every time a combatant *dies*, the engine rolls one pickup onto the field
(`BattleData.CreateDrop`, `BattleData.cs:323`): **~30% a hammer, ~70% a buff gem** (the roll is
`rand.Next(10) > 6`). It sparkles in a random empty zone and lasts a few seconds. You **click** it to
grab it.

**Buff gems — the ones that change the fight.** Clicking a gem flies it into its matching **container
slot** in the HUD row along the bottom (`ProcessBuffClick`); the slot shows how many you've banked.
**Click a filled slot to spend one**, deploying that buff on your team (whole-team, or a placeable
radius in the debug `bRad` mode). Each buff lasts **4 seconds** and modifies stats
(`BasicBuff.cs:BuffActions`):

| Gem | Effect | Feel |
|---|---|---|
| **Dragon_Wing** | +3 armor, +5 power, −10 movement | tanky bruiser |
| **Lion_Paw** | +3 power, +15 attack speed | aggressive DPS |
| **Wolf_Ear** | +15% crit, +15 movement | skirmisher |
| **Eagle_Feather** | +20 movement, +20 attack speed | swarm/rush |
| **Crab_Claw** | +4 power, −20 movement | slow heavy hitter |
| **Squirrel_Acorn** | +40 attack speed, +35 movement, −1 power | flurry |
| **Toad_Eye** | (none — stubbed) | not implemented |
| **Snake_Fang** | (none — stubbed) | not implemented |

So the tactical lever in a raid is: let your troopers trade blows, then **pick up gems and time your
team buffs** to swing a clash. Buffs stack while active and expire after 4s.

**The hammer — what it does today.** Clicking a hammer runs `ProcessHammerClick`
(`BattleSceneBase.cs:349`): it **increments a counter and plays a chime**, and draws "x N" beside the
hammer icon top-right (`BattleSceneBase.cs:214`). That is the *entire* current behaviour — the hammer
is a **tally only**; the count (`_iHammerCtr`) is never read or spent anywhere in the code. It has no
effect on the battle or on the village. It's effectively a "hammers collected this fight" score.

> **Design hook (deferred):** the hammer is the obvious bridge between the raid and the village
> economy — a "materials/build" token that a repelled raid could bank back into the village (coin,
> or a build resource toward the walls/scout-towers in `food.txt`). Wiring `_iHammerCtr` into the
> `VillageRaidResult` and paying it out in `Village.ResolveRaid` would make defending pay off
> materially, not just protect production. Flagged, not built — say the word.

## 6. Tuning knobs (file:line)

All numbers are placeholders. Every knob is a named `const` or a single expression.

**Raid battle — `VillageRaid.cs`:**
| Knob | Line | Effect |
|---|---|---|
| `fPowerScale` = 3.0 / `fHpScale` = 0.5 | 42–43 | Global "make clashes brisk" — every trooper hits harder / dies faster. |
| `fRaiderPower` = 3.5 / `fRaiderHp` = 6.0 | 44–45 | How much nastier a raider is than a defender (this is why you got decimated — raiders are few but brutal). Lower these for a fairer fight. |
| `iDefenderCap` = 40 / `iRaiderCap` = 24 | 35–36 | Hard caps on formation size. |
| `iRowsPerCol`/spacing | 32–34 | Formation shape on the field. |

**Economy / strategic — `Village.cs`:**
| Knob | Line | Effect |
|---|---|---|
| Starting state `_iPop=100 _iCoin=40 _fFood=30` | 255 | Week-1 resources. |
| `iWinWeek` = 8 | 92 | Survive past this many weeks → thrive. |
| Raid roll `_cRnd.Next(100) < _ciPredChance` | 316 | Per-vocation weekly raid chance (base chances live in the vocation table, `VillageTypes.cs`). |
| Raider count `bBig ? Next(3,6) : Next(6,11)` | 321 | How many monsters show up. |
| Muster size `Max(8, Min(30, _iPop/6))` | 344 | How many defenders you field. |
| Ignore penalty `iBefore / 2` | (in `IgnoreOutcome`) | How hard an ignored raid hits production (currently halves it). |
| Emigration `… * 0.15f` | 421 | How fast villagers leave when short of food. |
| Market reserve `iNeed * 2` | 427 | Food kept back before selling surplus. |
| Spoilage `_fFood * 0.9f` | 444 | Weekly food decay. |

**Vocation table — `VillageTypes.cs` (`VillageRules.cDefs`):** the food %, raid %, terrain
requirement, market good, and perk for all 12 vocations, one row each. This is the first place to
edit balance/flavor; it's a direct transcription of `my_vision.txt` + `food.txt`.

## 7. Known rough edges (mock-level, by design)

- **Un-tuned numbers** — the raid may be a slaughter and the economy easy or brutal; that's a tuning
  pass, not a bug (§5).
- **Region markers are approximate** — placed at readable screen spots, not aligned to features on
  the AI-generated map.
- **One template per side in the raid** — defenders are `cLSteward.cTemplates[0]`, raiders are
  `cRSteward.cTemplates[1]` (a plain enemy trooper standing in for "trolls/giants"). Real monster
  art/units would slot in here later, same as the Kaiju-minion plan.
- **No village naming / coat-of-arms** front-end from `my_vision.txt` yet — the mock auto-names the
  village and jumps straight to region select.

## 8. Natural next steps (deferred)

- Village naming + coat-of-arms intro (`my_vision.txt` opening).
- Mixed-unit / real monster raiders once creature art exists.
- Deeper Market (bartering, holding for price swings), foraging skills, town upgrades/scout towers
  from `food.txt` — all currently stubbed or flavor-only.
- Wire the village muster to the persisted, leveled roster (like Kaiju Hunt banks exp) instead of a
  fixed template, if progression should carry across.
