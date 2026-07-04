// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: Kaiju Hunt demo mode (docs/DESIGN_DIRECTION.md). One huge monster versus the
	//// player's army: a starting squad deploys automatically, and the player left-clicks the
	//// field to deploy the wheel-selected reinforcement from a limited reserve. Survivors bank
	//// their exp into the persisted roster (fibonacci leveling, LevelCurve) at victory. All the
	//// shared battle machinery lives in BattleSceneBase; this scene is just the mode.
	public class KaijuHunt : BattleSceneBase
	{
		Kaiju			_cKaiju;
		int				_iReserves;
		bool			_bPlayerWon;

		//// ddhj: wheel-selectable spawn roster -- scroll to pick what the next left-click
		//// deploys. Captains ride the existing "Cap"-in-name 1.4x scale mechanic.
		class SpawnType
		{
			public string				sLabel;
			public TemplateCfgMaster	cCfg;
			public int					iCost;
			public int					iCap;		// -1 = unlimited
			public int					iSpawned;
			public int					iLevel;		// fibonacci level from the banked roster exp
		}
		List<SpawnType>	_caSpawnTypes = new List<SpawnType>();
		int				_iSelSpawn;

		//// ddhj: survivors bank battle exp into the persisted roster templates (template-level
		//// progression, the original design). Keyed by sTemplateName; the captain's clone banks
		//// into the REAL cCaptains entry, not the clone.
		Dictionary<string, TemplateCfgMaster>	_cBankTargets = new Dictionary<string, TemplateCfgMaster>();
		string			_sBankReport = "";

		const int		iInitialSquad = 10;
		const int		iReserveMax = 60;

	// BattleSceneBase hooks

		protected override void SetupBattle()
		{
			DataStore		cData = DataStore.cInstance;
			GraphicsDevice	cGraphics = cData.cGraphics;
			DataManager		cMgr = DataManager.cInstance;
			Team			cTeam;
			Trooper			cTrooper;
			Priest			cHealer;

			_bPlayerWon = false;
			_sBankReport = "";
			_iReserves = iReserveMax;

			//// ddhj: 2026 -- artist-tunable combat balance (content/Config/CombatTuning.json,
			//// see CombatTuning.cs). Kaiju and a brand-new Peasant slot pull their stats straight
			//// from here every hunt since neither is persisted progression; the Halberdier
			//// ("footmen") baseline instead only applies at DataStore.InitNew (a fresh campaign),
			//// since cLSteward.cTemplates[0].cStats is live player progression (Template leveling)
			//// after that and must not be clobbered on every hunt.
			CombatTuning	cTuning = CombatTuning.Load();

			// the wheel-selectable spawn roster. Index 0 stays Halberdier -- it seeds the
			// initial squad below, so new entries are appended, never inserted, to keep that
			// behavior stable across roster changes.
			_caSpawnTypes.Clear();
			_iSelSpawn = 0;
			_caSpawnTypes.Add(new SpawnType { sLabel = "Halberdier", cCfg = cData.cLSteward.cTemplates[0], iCost = 2, iCap = -1 });
			TemplateCfgMaster cCapCfg = new TemplateCfgMaster(cData.cLSteward.cCaptains[0]);
			// the copy ctor doesn't carry the name; "Cap" in the name drives the 1.4x scale
			cCapCfg.sTemplateName = "Captain " + cData.cLSteward.cCaptains[0].sTemplateName;
			_caSpawnTypes.Add(new SpawnType { sLabel = cCapCfg.sTemplateName, cCfg = cCapCfg, iCost = 6, iCap = 2 });

			//// ddhj: 2026 build-order step 4 -- the first unit out of the rebuilt sprite
			//// stitcher (tools/sprite_stitcher), from the recovered original art at
			//// c:/dev/art assets raw/wodyn/peasant_pngs. Now a real persisted roster template
			//// (DataStore.InitNew adds it for fresh saves); find-or-create here self-heals any
			//// save file written before this template slot existed, so an in-progress campaign
			//// doesn't lose the ability to level Peasant.
			TemplateCfgMaster cPeasantCfg = cData.cLSteward.cTemplates.Find(t => t.sTemplateName == "Peasant");
			if(cPeasantCfg == null) {
				cPeasantCfg = new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Peasant\PeasantArray", @"Sprite Data\Troopers\Peasant\Peasant"));
				cPeasantCfg.cStats = cTuning.Peasant;
				cPeasantCfg.sTemplateName = "Peasant";
				cPeasantCfg.iTLeftPos = cPeasantCfg.iBLeftPos = 466;
				cPeasantCfg.iCount = 20;
				cPeasantCfg.iId = cData.cLSteward.cTemplates.Count;
				cData.cLSteward.cTemplates.Add(cPeasantCfg);
			}
			_caSpawnTypes.Add(new SpawnType { sLabel = "Peasant", cCfg = cPeasantCfg, iCost = 1, iCap = -1 });

			//// ddhj: second and third units out of the stitcher, from the recovered bandit.xcf /
			//// sword_hero.xcf frames (tools/gimp_xcf export -> tools/sprite_stitcher). Neither
			//// source had all 18 action names -- both configs alias every Attack/Idle/Death/Move
			//// name onto the one real idle loop + one real attack swing each unit actually has
			//// (same aliasing approach peasant.config.json used, just with fewer distinct source
			//// animations to alias from). Stats are first-pass placeholders, not yet balanced.
			TemplateCfgMaster cBanditCfg = new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Bandit\BanditArray", @"Sprite Data\Troopers\Bandit\Bandit"));
			cBanditCfg.cStats = new Stats { iAtkSpeed = 0, iMovement = 5, fHp = 80, iMaxHp = 80, iPower = 4, fCrit = .05f, iHealPoint = 25, iFleePoint = 12, iArmorClass = 1 };
			cBanditCfg.sTemplateName = "Bandit";
			_caSpawnTypes.Add(new SpawnType { sLabel = "Bandit", cCfg = cBanditCfg, iCost = 2, iCap = -1 });

			TemplateCfgMaster cSwordHeroCfg = new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\SwordHero\SwordHeroArray", @"Sprite Data\Troopers\SwordHero\SwordHero"));
			cSwordHeroCfg.cStats = new Stats { iAtkSpeed = 0, iMovement = 5, fHp = 90, iMaxHp = 90, iPower = 6, fCrit = .08f, iHealPoint = 25, iFleePoint = 10, iArmorClass = 1 };
			cSwordHeroCfg.sTemplateName = "SwordHero";
			_caSpawnTypes.Add(new SpawnType { sLabel = "Sword Hero", cCfg = cSwordHeroCfg, iCost = 3, iCap = -1 });

			// where each spawn type's survivors bank their experience after the hunt
			_cBankTargets.Clear();
			_cBankTargets[cData.cLSteward.cTemplates[0].sTemplateName] = cData.cLSteward.cTemplates[0];
			_cBankTargets[cCapCfg.sTemplateName] = cData.cLSteward.cCaptains[0];
			_cBankTargets[cPeasantCfg.sTemplateName] = cPeasantCfg;

			// current fibonacci levels from the banked exp (levels change only between hunts)
			foreach(SpawnType cType in _caSpawnTypes) {
				TemplateCfgMaster cBank;
				if(_cBankTargets.TryGetValue(cType.cCfg.sTemplateName, out cBank))
					cType.iLevel = LevelCurve.GetLevel(cBank.cExpData);
			}

			// the player's starting squad. Owner call: unlike the classic battleground, Kaiju
			// Hunt skips the scripted Wait+MoveToPoint march-to-formation entirely -- units
			// spawn already Ready and immediately charge the kaiju via the normal
			// ChooseOpponent/ChargeOpponent pursuit, which recomputes facing (bDir) toward the
			// real attack point every frame. The march's one-shot destination-based bDir could
			// end up frozen wrong for units assigned an attack point on the kaiju's far side
			// (anyone approaching a Right-side slot after a long, entirely-rightward march from
			// the left edge never got a reason to flip until arrival, and attack swings don't
			// update bDir at all) -- starting the real pursuit from spawn avoids that scripted
			// detour, not just the visual march.
			cTeam = _cBattleData.caTeams[0];
			for(int iCount = 0; iCount < iInitialSquad; ++iCount) {
				cTrooper = SpawnTrooper(cTeam, new Vector2(150, 100 + (iCount % 12) * 30), _caSpawnTypes[0].cCfg, bFaceRight: true);
				cTrooper.cAiData.eState = EBattleAiStates.Ready;
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
			}

			for(int iCount = 0; iCount < 2; ++iCount) {
				cHealer = cMgr.CreatePriest(new Vector2(65, 180 + 120 * iCount), cTeam, (int)(40f * 7 * 6), 7, 40f, 1.3333f, _cBattleData);
				cTeam.cHealerList.Add(cHealer.iId, cHealer);
				_cObjMgr.AddObject(cHealer);
			}

			// the kaiju: one monster on the opposing team
			cTeam = _cBattleData.caTeams[1];
			TemplateCfgMaster cKaijuCfg = new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsable"));
			//// ddhj: owner call -- armor 12 vs a Halberdier's iPower=5 base attack meant
			//// Trooper_Combatant.cs's DealDamage formula (iDamage - armor*rand(0.6,0.99)) was
			//// negative for basically every non-crit hit (12*0.6=7.2 alone already exceeds 5),
			//// which routes into the "attacking a monster" fallback: ceil(iDamage*.10) -- just
			//// 1 damage per hit, reading as "damage numbers are 0's." Armor 3 (the shipped
			//// CombatTuning.json default) keeps basic Halberdier hits in the normal formula's
			//// positive range (~2-3 dmg) instead of permanently degenerating to that floor, while
			//// still meaningfully soaking Peasant hits (power 3) and rewarding Captains (power 40)
			//// for their cost -- worth knowing before retuning armor much higher.
			cKaijuCfg.cStats = cTuning.Kaiju;
			cKaijuCfg.sTemplateName = "Kaiju";

			_cKaiju = new Kaiju(cMgr.iCurObjId, cTeam, cMgr.CreateTemplate(cKaijuCfg), cTuning.fKaijuScale);
			cTeam.cActiveList.Add(_cKaiju.iId, _cKaiju);
			cTeam.cMembers.Add(_cKaiju);
			_cKaiju.tPos = new Vector2(850, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);
			_cObjMgr.AddObject(_cKaiju);

			_cKaiju.cActionManager.AddAction(new Action(_cKaiju.Wait, 50, TimeSpan.Zero));
			_cKaiju.cActionManager.AddAction(new Action(_cKaiju.MoveToPoint, new Vector2(650, 260), null));
			_cKaiju.cActionManager.AddPermAction(new Action(_cKaiju.BasicBattleManager, _cBattleData, null));
		}

		protected override void OnBattleOver(int iLosingTeamId)
		{
			_bPlayerWon = iLosingTeamId == 1;

			if(_bPlayerWon) {
				_cBgm = DataManager.cInstance.CreateMusic("Minibossies_FinalFantasy6_VictoryFanfare");
				if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play(_cBgm);
			}

			// bank the survivors' exp into the persisted roster (a wipe banks nothing:
			// cActiveList is already empty)
			BankSurvivorExperience();
		}

		protected override void DrawHud(SpriteBatch cBatch)
		{
			if(_cKaiju != null)
				cBatch.DrawString(_cFont, string.Format("KAIJU HP: {0} / {1}", (int)Math.Max(0, _cKaiju.cStats.fHp), _cKaiju.cStats.iMaxHp),
					new Vector2(10, 10), Color.OrangeRed);
			cBatch.DrawString(_cFont, string.Format("Troopers: {0}    Reserves: {1}",
				_cBattleData.caTeams[0].cActiveList.Count, _iReserves), new Vector2(10, 30), Color.White);

			switch(_cBattleData.eState) {
				case EBattlegroundState.Battle: {
					SpawnType	cSel = _caSpawnTypes[_iSelSpawn];
					string		sCapNote = cSel.iCap >= 0 && cSel.iSpawned >= cSel.iCap ? "  (cap reached)"
									: _iReserves < cSel.iCost ? "  (not enough reserves)" : "";

					cBatch.DrawString(_cFont, "Left-click the field to send reinforcements", new Vector2(620, 10), Color.LightGray);
					cBatch.DrawString(_cFont, string.Format("Spawn [wheel]: {0}{1}  cost {2}{3}", cSel.sLabel,
						cSel.iLevel > 0 ? " L" + cSel.iLevel : "", cSel.iCost, sCapNote),
						new Vector2(620, 30), Color.Gold);
				} break;

				case EBattlegroundState.Victory:
					cBatch.DrawString(_cFont,
						_bPlayerWon ? "THE KAIJU HAS FALLEN -- right-click to withdraw"
									: "YOUR ARMY IS DEVOURED -- right-click to withdraw",
						new Vector2(330, 40), _bPlayerWon ? Color.Gold : Color.OrangeRed);
					if(_sBankReport.Length > 0)
						cBatch.DrawString(_cFont, _sBankReport, new Vector2(330, 60), Color.LightGreen);
				break;
			}
		}

		public override void RegisterHandlers()
		{
			base.RegisterHandlers();
			InputSystem.MouseDown += new MouseEventHandler(InputSystem_MouseDown);
			InputSystem.MouseWheel += new MouseEventHandler(InputSystem_MouseWheel);
		}

		public override void UnRegisterHandlers()
		{
			base.UnRegisterHandlers();
			InputSystem.MouseDown -= InputSystem_MouseDown;
			InputSystem.MouseWheel -= InputSystem_MouseWheel;
		}

	// Mode internals

		// create a player trooper from the given template, register it with team/manager, place it
		Trooper SpawnTrooper(Team cTeam, Vector2 tPos, TemplateCfgMaster cCfg, bool bFaceRight = true)
		{
			TrooperTemplate		cTemplate = DataManager.cInstance.CreateTemplate(cCfg);
			TemplateCfgMaster	cBank;

			// leveled roster: apply the fibonacci growth to the template stats BEFORE the
			// trooper snapshots them (so buff recalibration keeps the level bonus too)
			if(_cBankTargets.TryGetValue(cCfg.sTemplateName, out cBank))
				LevelCurve.ApplyLevel(cTemplate.cStats, LevelCurve.GetLevel(cBank.cExpData));

			Trooper	cTrooper = new Trooper(DataManager.cInstance.iCurObjId, cTeam, cTemplate);

			cTeam.cActiveList.Add(cTrooper.iId, cTrooper);
			cTeam.cMembers.Add(cTrooper);
			//// ddhj: kaiju -- bDir MUST be set before tPos: tPos's setter is what calls
			//// UpdateRefPoints, so setting bDir afterward left _tCenter computed against the
			//// pre-spawn default facing for a frame. TrooperUpkeep now refreshes ref points
			//// every frame regardless (see that fix), so this alone wouldn't stay wrong for
			//// long, but there is no reason to spawn wrong even briefly when getting the order
			//// right costs nothing.
			cTrooper.bDir = bFaceRight;
			cTrooper.tPos = tPos;
			_cObjMgr.AddObject(cTrooper);

			return cTrooper;
		}

		// roll each survivor's battle exp into its roster template and persist the campaign
		void BankSurvivorExperience()
		{
			int		iSurvivors = 0;
			TemplateCfgMaster	cBank;

			foreach(Combatant nCombatant in _cBattleData.caTeams[0].cActiveList.Values) {
				Trooper	cTrooper = (Trooper)nCombatant;

				if(!_cBankTargets.TryGetValue(cTrooper.sTemplateName, out cBank))
					continue;

				// ExperienceData.operator+ returns a fresh instance, so the bank never aliases
				// the battle-scoped accumulator
				cBank.cExpData = cBank.cExpData == null ? cTrooper.cExpData + new ExperienceData()
														: cBank.cExpData + cTrooper.cExpData;
				++iSurvivors;
			}

			DataStore.cInstance.SaveData();
			_sBankReport = string.Format("{0} survivor(s) banked experience to the roster -- saved", iSurvivors);
		}

		void InputSystem_MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			// battle over: right-click withdraws (battleground convention)
			if(_cBattleData.eState == EBattlegroundState.Victory && eMouseEvt.Button == MouseButton.Right) {
				BackToMenu();
				return;
			}

			IClickable nClickableObj = null;

			// mid-battle: left-click on the field sends the selected reinforcement toward the
			// click. The field-bounds check alone already excludes the HUD margins (stat bar,
			// hammer icon, and the buff-container row all sit outside this rect), but kill-drops
			// (buffs/hammers) land INSIDE the field during combat -- IsClickableAt makes sure a
			// click that's picking one of those up doesn't also spawn a trooper on top of it.
			if(_cBattleData.eState == EBattlegroundState.Battle && eMouseEvt.Button == MouseButton.Left
				&& eMouseEvt.X > 112 && eMouseEvt.X < 912 && eMouseEvt.Y > 70 && eMouseEvt.Y < 506) {
				if ((nClickableObj = _cObjMgr.IsClickableAt(eMouseEvt.Location)) == null)
				{
					SpawnType cSel = _caSpawnTypes[_iSelSpawn];

					// enough reserves, and under the type's cap?
					if (_iReserves < cSel.iCost || (cSel.iCap >= 0 && cSel.iSpawned >= cSel.iCap))
						return;
					_iReserves -= cSel.iCost;
					++cSel.iSpawned;

					// spawn right where the player clicked and charge immediately -- see the
					// initial-squad comment above for why Kaiju Hunt skips the scripted march
					int iX = Math.Min(Math.Max(eMouseEvt.X, 130), 890);
					int iY = Math.Min(Math.Max(eMouseEvt.Y, 80), 480);
					Trooper cTrooper = SpawnTrooper(_cBattleData.caTeams[0], new Vector2(iX, iY), cSel.cCfg, bFaceRight: iX < _cKaiju.tCenter.X);

					cTrooper.cAiData.eState = EBattleAiStates.Ready;
					cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				} else {
					nClickableObj.dlProcessClick(eMouseEvt.Button, nClickableObj);
                }
			}
		}

		void InputSystem_MouseWheel(object oSender, MouseEventArgs eMouseEvt)
		{
			if(_cBattleData.eState != EBattlegroundState.Battle || _caSpawnTypes.Count == 0)
				return;

			// wheel cycles the spawn roster (wraps both directions)
			_iSelSpawn = (_iSelSpawn + (eMouseEvt.Delta > 0 ? 1 : _caSpawnTypes.Count - 1)) % _caSpawnTypes.Count;
		}
	}
}
