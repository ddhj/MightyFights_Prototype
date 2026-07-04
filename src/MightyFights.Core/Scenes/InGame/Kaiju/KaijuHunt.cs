// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: Kaiju Hunt demo mode (docs/DESIGN_DIRECTION.md). One huge monster versus the
	//// player's army: a starting squad deploys automatically, and the player left-clicks the
	//// field to send reinforcements from a limited reserve. Survivors bank experience through
	//// the same ExperienceTally the classic battleground uses. Built on the validated battle
	//// pipeline (BattlegroundData/BattleObjectManager/Trooper AI); the scene itself is a lean
	//// sibling of BattleGround_Basic. v1 kaiju art is a scaled-up fsable halberdier -- real
	//// kaiju sheets arrive via the sprite stitcher task.
	public class KaijuHunt : IGameScene
	{
		ESceneStates	_eState;
		SpriteBatch		_cSpriteBatch;
		Texture2D		_cBackground,
						_cHudBorder;
		SpriteFont		_cFont;
		Cursor			_cCursor;
		Song			_cBgm;
		Random			_cRand = DataStore.cInstance.cRand;

		BattlegroundData	_cBattleData = new BattlegroundData();
		BattleObjectManager	_cObjMgr = new BattleObjectManager();
		ExperienceTally		_cExpTally;

		//// ddhj: the classic battleground's drop economy, wired for real (owner: "when the
		//// hammers / buffs drop create effects etc") -- kill-drops route into the HUD buff
		//// containers for drag-apply, hammers collect with a chime.
		Dictionary<EBuffEffects, BuffContainer>	_cBuffContainerList = new Dictionary<EBuffEffects, BuffContainer>();
		SoundEffectInstance	_cBuffSfx,
							_cHammerSfx;
		int					_iHammerCtr;

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

		//// ddhj: step 2 of the kaiju build order -- survivors bank their battle exp into the
		//// persisted roster templates (template-level progression, the original design: see
		//// DESIGN_DIRECTION.md). Keyed by sTemplateName; the captain's clone banks into the
		//// REAL cCaptains entry, not the clone.
		Dictionary<string, TemplateCfgMaster>	_cBankTargets = new Dictionary<string, TemplateCfgMaster>();
		string			_sBankReport = "";

		const int		iInitialSquad = 12;
		const int		iReserveMax = 60;
		const float		fKaijuScale = 2.5f;

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			DataStore.cInstance.tTime = cTime;

			// process the battle state (same shape as BattleGround_Basic)
			switch(_cBattleData.eState) {
				case EBattlegroundState.Init:
					if(!WaitForBattleStart())
						_cBattleData.eState = EBattlegroundState.Battle;
				break;

				case EBattlegroundState.Battle:
					foreach(Team cTeam in _cBattleData.caTeams)
						if(cTeam.cActiveList.Count == 0) {
							_cBattleData.eState = EBattlegroundState.Victory;
							_bPlayerWon = cTeam.iId == 1;

							MediaPlayer.Stop();
							if(_bPlayerWon) {
								_cBgm = DataManager.cInstance.CreateMusic("Minibossies_FinalFantasy6_VictoryFanfare");
								if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play(_cBgm);
							}

							// aggregate the battle through the standard tally, then bank the
							// survivors' exp into the persisted roster (a wipe banks nothing:
							// cActiveList is already empty)
							_cExpTally.Tally(_cBattleData);
							BankSurvivorExperience();
							break;
						}
				break;

				case EBattlegroundState.Victory:
					// exit is handled by right-click in InputSystem_MouseDown
				break;
			}

			if(_cBattleData.eState != EBattlegroundState.Dialog)
				_cObjMgr.Process(cTime);
		}

		public void Draw(GameTime cTime)
		{
			_cSpriteBatch.Begin(SpriteSortMode.BackToFront, null); {
				if(DataStore.cInstance.bShowBg) {
					_cSpriteBatch.Draw(_cBackground, new Vector2(112, 70), null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);
					_cSpriteBatch.Draw(_cHudBorder, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, .99f);
				}

				// draw all objects in the manager
				_cObjMgr.Draw(_cSpriteBatch);

				//// hunt HUD
				if(_cKaiju != null)
					_cSpriteBatch.DrawString(_cFont, string.Format("KAIJU HP: {0} / {1}", (int)Math.Max(0, _cKaiju.cStats.fHp), _cKaiju.cStats.iMaxHp),
						new Vector2(10, 10), Color.OrangeRed);
				_cSpriteBatch.DrawString(_cFont, string.Format("Troopers: {0}    Reserves: {1}",
					_cBattleData.caTeams[0].cActiveList.Count, _iReserves), new Vector2(10, 30), Color.White);
				_cSpriteBatch.DrawString(_cFont, string.Format("x {0}", _iHammerCtr), new Vector2(955, 35), Color.White);

				switch(_cBattleData.eState) {
					case EBattlegroundState.Battle: {
						SpawnType	cSel = _caSpawnTypes[_iSelSpawn];
						string		sCapNote = cSel.iCap >= 0 && cSel.iSpawned >= cSel.iCap ? "  (cap reached)"
										: _iReserves < cSel.iCost ? "  (not enough reserves)" : "";

						_cSpriteBatch.DrawString(_cFont, "Left-click the field to send reinforcements", new Vector2(620, 10), Color.LightGray);
						_cSpriteBatch.DrawString(_cFont, string.Format("Spawn [wheel]: {0}{1}  cost {2}{3}", cSel.sLabel,
							cSel.iLevel > 0 ? " L" + cSel.iLevel : "", cSel.iCost, sCapNote),
							new Vector2(620, 30), Color.Gold);
					} break;

					case EBattlegroundState.Victory:
						_cSpriteBatch.DrawString(_cFont,
							_bPlayerWon ? "THE KAIJU HAS FALLEN -- right-click to withdraw"
										: "YOUR ARMY IS DEVOURED -- right-click to withdraw",
							new Vector2(330, 40), _bPlayerWon ? Color.Gold : Color.OrangeRed);
						if(_sBankReport.Length > 0)
							_cSpriteBatch.DrawString(_cFont, _sBankReport, new Vector2(330, 60), Color.LightGreen);
					break;
				}
			} _cSpriteBatch.End();

			_cSpriteBatch.Begin(); {
				_cObjMgr.DrawParticles(_cSpriteBatch);
			} _cSpriteBatch.End();
		}

		public bool Init()
		{
			DataStore		cData = DataStore.cInstance;
			ContentManager	cContent = cData.cContent;
			GraphicsDevice	cGraphics = cData.cGraphics;
			DataManager		cMgr = DataManager.cInstance;
			Team			cTeam;
			Trooper			cTrooper;
			Priest			cHealer;

			_cExpTally = new ExperienceTally();
			_bPlayerWon = false;
			_iReserves = iReserveMax;

			try {
				_cSpriteBatch = new SpriteBatch(cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass 800x436");
				_cHudBorder = cContent.Load<Texture2D>(@"In Game\Hud\border_jackson");
				_cFont = cContent.Load<SpriteFont>(@"Shared\TestFon");

				// the 1x1 white pixel used by lifebars/zone rects (priests draw theirs
				// unconditionally) -- BattleGround_Basic.Init creates this too
				DataStore.cInstance.cBorder = new Texture2D(cGraphics, 1, 1);
				DataStore.cInstance.cBorder.SetData<Color>(new[] { Color.White });

				_cBattleData.cObjMgr = _cObjMgr;
				_cBattleData.dlBuffClick = ProcessBuffClick;
				_cBattleData.dlDropClick = ProcessHammerClick;
				_cObjMgr.CreateParticleManager();

				_cBuffSfx = DataManager.cInstance.CreateSfx("Magic Wand Noise-SoundBible.com-375928671");
				_cHammerSfx = DataManager.cInstance.CreateSfx("Electronic_Chime-KevanGC-495939803");
				_cBuffSfx.Volume = _cHammerSfx.Volume = .4f;
				_iHammerCtr = 0;
				DataStore.cInstance.cBattleData = _cBattleData;

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
				_sBankReport = "";
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

				// the drop economy: buff containers in the HUD slots + the hammer icon
				_cBuffContainerList.Clear();
				_cBattleData.cBuffContainers = _cBuffContainerList;
				CreateBuffContainers();
				_cObjMgr.AddObject(DataManager.cInstance.CreateHammerIcon());

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2),
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(_cCursor);
				_cBattleData.cCursor = _cCursor;

				_cBattleData.eState = EBattlegroundState.Init;

				string[] saMusic = new string[] { @"Battle\battle1", @"Battle\battle2" };
				DataStore.cInstance.cBgm = _cBgm = DataManager.cInstance.CreateMusic(saMusic[_cRand.Next(2)]);
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Volume = .6f;
				if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play(_cBgm);
			} catch(Exception xEx) {
				System.Diagnostics.Debug.WriteLine(xEx.ToString());
				return false;
			}

			return true;
		}

		public void Unload()
		{
			MediaPlayer.Stop();
		}

		public void ToggleControls()
		{
		}

		public void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
			InputSystem.MouseDown += new MouseEventHandler(InputSystem_MouseDown);
			InputSystem.MouseWheel += new MouseEventHandler(InputSystem_MouseWheel);
		}

		public void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;
			InputSystem.MouseDown -= InputSystem_MouseDown;
			InputSystem.MouseWheel -= InputSystem_MouseWheel;
		}

		#endregion

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

		bool WaitForBattleStart()
		{
			bool bReady = false;

			foreach(Team cTeam in _cBattleData.caTeams)
				foreach(Combatant nCombatant in cTeam.cActiveList.Values)
					bReady |= ((Trooper)nCombatant).cAiData.eState != EBattleAiStates.Ready;

			return bReady;
		}

		// the classic battleground's 8 buff containers, same HUD slot positions
		void CreateBuffContainers()
		{
			ContentManager	cContent = DataStore.cInstance.cContent;
			BuffContainer	cTmpContainer;

			EBuffEffects[]	eaSlots = new EBuffEffects[] { EBuffEffects.Squirrel_Acorn, EBuffEffects.Crab_Claw,
				EBuffEffects.Wolf_Ear, EBuffEffects.Toad_Eye, EBuffEffects.Snake_Fang, EBuffEffects.Eagle_Feather,
				EBuffEffects.Lion_Paw, EBuffEffects.Dragon_Wing };

			for(int iCount = 0; iCount < eaSlots.Length; ++iCount) {
				_cBuffContainerList.Add(eaSlots[iCount], cTmpContainer = new BuffContainer(new Vector2(132 + iCount * 100, 519)));
				cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
				cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
				cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2),
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			}
		}

		void ProcessBuffClick(object oSender, object oArgs)
		{
			_cBuffContainerList[((BasicBuff)oArgs).eType].AddBuff((BasicBuff)oArgs);
			if(_cBuffSfx.State == SoundState.Playing)
				_cBuffSfx.Stop();
			_cBuffSfx.Play();
		}

		void ProcessHammerClick(object oSender, object oArgs)
		{
			++_iHammerCtr;
			if(_cHammerSfx.State == SoundState.Playing)
				_cHammerSfx.Stop();
			_cHammerSfx.Play();
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

		void InputSystem_MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			if(_cCursor != null)
				_cCursor.Update(eMouseEvt.Location);
		}

		void InputSystem_MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			// battle over: right-click withdraws (battleground convention)
			if(_cBattleData.eState == EBattlegroundState.Victory && eMouseEvt.Button == MouseButton.Right) {
				_eState = ESceneStates.Inactive;
				DataStore.cInstance.cSceneMgr.RemoveScene(this);
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
