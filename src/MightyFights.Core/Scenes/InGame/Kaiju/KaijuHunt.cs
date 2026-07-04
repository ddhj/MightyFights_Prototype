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

		const int		iInitialSquad = 12;
		const int		iReserveMax = 60;
		const float		fKaijuScale = 2.5f;

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

			// the wheel-selectable spawn roster (step 4 of the build order adds the Peasant here)
			_caSpawnTypes.Clear();
			_iSelSpawn = 0;
			_caSpawnTypes.Add(new SpawnType { sLabel = "Halberdier", cCfg = cData.cLSteward.cTemplates[0], iCost = 2, iCap = -1 });
			TemplateCfgMaster cCapCfg = new TemplateCfgMaster(cData.cLSteward.cCaptains[0]);
			// the copy ctor doesn't carry the name; "Cap" in the name drives the 1.4x scale
			cCapCfg.sTemplateName = "Captain " + cData.cLSteward.cCaptains[0].sTemplateName;
			_caSpawnTypes.Add(new SpawnType { sLabel = cCapCfg.sTemplateName, cCfg = cCapCfg, iCost = 6, iCap = 2 });

			// where each spawn type's survivors bank their experience after the hunt
			_cBankTargets.Clear();
			_cBankTargets[cData.cLSteward.cTemplates[0].sTemplateName] = cData.cLSteward.cTemplates[0];
			_cBankTargets[cCapCfg.sTemplateName] = cData.cLSteward.cCaptains[0];

			// current fibonacci levels from the banked exp (levels change only between hunts)
			foreach(SpawnType cType in _caSpawnTypes) {
				TemplateCfgMaster cBank;
				if(_cBankTargets.TryGetValue(cType.cCfg.sTemplateName, out cBank))
					cType.iLevel = LevelCurve.GetLevel(cBank.cExpData);
			}

			// the player's starting squad, spawned exactly the way the classic battleground does
			cTeam = _cBattleData.caTeams[0];
			for(int iCount = 0; iCount < iInitialSquad; ++iCount) {
				cTrooper = SpawnTrooper(cTeam, new Vector2(25, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2), _caSpawnTypes[0].cCfg);
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, 50 * iCount, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(200, 100 + (iCount % 12) * 30), null));
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
			cKaijuCfg.cStats = new Stats { iAtkSpeed = 25, iMovement = 5, fHp = 6000, iMaxHp = 6000, iPower = 55, fCrit = .15f, iHealPoint = 0, iFleePoint = 0, iArmorClass = 12 };
			cKaijuCfg.sTemplateName = "Kaiju";

			_cKaiju = new Kaiju(cMgr.iCurObjId, cTeam, cMgr.CreateTemplate(cKaijuCfg), fKaijuScale);
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
		Trooper SpawnTrooper(Team cTeam, Vector2 tPos, TemplateCfgMaster cCfg)
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

			// mid-battle: left-click on the field sends the selected reinforcement toward the click
			if(_cBattleData.eState == EBattlegroundState.Battle && eMouseEvt.Button == MouseButton.Left
				&& eMouseEvt.X > 112 && eMouseEvt.X < 912 && eMouseEvt.Y > 70 && eMouseEvt.Y < 506) {
				SpawnType	cSel = _caSpawnTypes[_iSelSpawn];

				// enough reserves, and under the type's cap?
				if(_iReserves < cSel.iCost || (cSel.iCap >= 0 && cSel.iSpawned >= cSel.iCap))
					return;
				_iReserves -= cSel.iCost;
				++cSel.iSpawned;

				int		iY = Math.Min(Math.Max(eMouseEvt.Y, 80), 480);
				Trooper	cTrooper = SpawnTrooper(_cBattleData.caTeams[0], new Vector2(25, iY), cSel.cCfg);

				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(Math.Min(eMouseEvt.X, 860), iY), null));
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
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
