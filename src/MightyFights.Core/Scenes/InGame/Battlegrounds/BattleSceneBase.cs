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
	//// ddhj: 2026 consolidation (owner-directed) -- the shared battle-scene driver that
	//// BattleGround_Basic and KaijuHunt previously duplicated by copy. Owns the battle state
	//// machine, victory detection + tally, the drop economy (buff containers, hammer counter,
	//// SFX), slow-mo, cursor, music, the common draw frame (background/objects/animal effect/
	//// zone overlay/particles), and teardown. Modes override the hooks: SetupBattle (armies,
	//// scripts, mode fields), DrawHud (mode HUD), OnBattleOver (consequences), and
	//// ProcessVictoryState (post-battle input). This is the mechanics layering the original
	//// manager classes were building toward.
	public abstract class BattleSceneBase : IGameScene
	{
	// Data -- shared battle machinery (protected: the modes read and extend these)
		protected ESceneStates		_eState;
		protected SpriteBatch		_cSpriteBatch;
		protected Texture2D			_cBackground,
									_cHudBorder,
									_cBorder;
		protected SpriteFont		_cFont;
		protected Cursor			_cCursor;
		protected Song				_cBgm;
		protected Random			_cRand = DataStore.cInstance.cRand;

		protected BattlegroundData		_cBattleData = new BattlegroundData();
		protected BattleObjectManager	_cObjMgr = new BattleObjectManager();
		protected ExperienceTally		_cExpTally;

		protected Dictionary<EBuffEffects, BuffContainer>	_cBuffContainerList = new Dictionary<EBuffEffects, BuffContainer>();
		protected SoundEffectInstance	_cBuffSfx,
										_cHammerSfx;
		protected int					_iHammerCtr;

		protected bool			_bUpdate = true;
		TimeSpan				_tSlowMo = TimeSpan.Zero,
								_tSlowMoTrigger = TimeSpan.FromMilliseconds(50);

	// Mode hooks
		/// <summary> build the armies / mode-specific objects and their battle scripts;
		/// runs between the common pre-init (content, sfx, battle data wiring) and the common
		/// post-init (buff containers, hammer, cursor, music) </summary>
		protected abstract void SetupBattle();

		/// <summary> draw the mode's HUD inside the main sprite batch </summary>
		protected abstract void DrawHud(SpriteBatch cBatch);

		/// <summary> battle-over consequences (fanfare, banking, dumps); the base has already
		/// stopped the music and run the tally. iLosingTeamId is the wiped team. </summary>
		protected virtual void OnBattleOver(int iLosingTeamId) {}

		/// <summary> per-frame processing while in the Victory state (e.g. rematch polling) </summary>
		protected virtual void ProcessVictoryState() {}

		/// <summary> per-frame debug/bookkeeping after the battle processes (e.g. fps counters) </summary>
		protected virtual void UpdateDebug(GameTime cTime) {}

	// IGameScene
		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public bool Init()
		{
			DataStore		cData = DataStore.cInstance;
			ContentManager	cContent = cData.cContent;

			_cExpTally = new ExperienceTally();
			_iHammerCtr = 0;

			try {
				_cSpriteBatch = new SpriteBatch(cData.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass 800x436");
				_cHudBorder = cContent.Load<Texture2D>(@"In Game\Hud\border_jackson");
				_cFont = cContent.Load<SpriteFont>(@"Shared\TestFon");

				// the 1x1 white pixel used by lifebars/zone rects
				DataStore.cInstance.cBorder = _cBorder = new Texture2D(cData.cGraphics, 1, 1);
				_cBorder.SetData<Color>(new[] { Color.White });

				_cBattleData.cObjMgr = _cObjMgr;
				_cBattleData.dlBuffClick = ProcessBuffClick;
				_cBattleData.dlDropClick = ProcessHammerClick;
				_cObjMgr.CreateParticleManager();
				DataStore.cInstance.cBattleData = _cBattleData;

				_cBuffSfx = DataManager.cInstance.CreateSfx("Magic Wand Noise-SoundBible.com-375928671");
				_cHammerSfx = DataManager.cInstance.CreateSfx("Electronic_Chime-KevanGC-495939803");
				_cBuffSfx.Volume = _cHammerSfx.Volume = .4f;

				// the mode builds its armies and scripts
				SetupBattle();

				// the drop economy: buff containers in the HUD slots + the hammer icon
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

				// random battle track
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

		public void Update(GameTime cTime)
		{
			// set the datastore elapsed time for the action and heuristic processing
			DataStore.cInstance.tTime = cTime;

			// the effect is outside the slowmo loop
			if(_cBattleData.cAnimalEffect != null)
				if(!_cBattleData.cAnimalEffect.Process(cTime))
					_cBattleData.cAnimalEffect = null;

			//// debug slow down the game
			if(DataStore.cInstance.bSlowMo) {
				_tSlowMo += cTime.ElapsedGameTime;
				if(_tSlowMo < _tSlowMoTrigger)
					return;
			}

			if(_bUpdate) {
				// process the battle state
				switch(_cBattleData.eState) {
					case EBattlegroundState.Init:
						if(!WaitForBattleStart())
							_cBattleData.eState = EBattlegroundState.Battle;
					break;

					case EBattlegroundState.Battle:
						// check to see if the battle is over
						foreach(Team cTeam in _cBattleData.caTeams)
							if(cTeam.cActiveList.Count == 0) {
								_cBattleData.eState = EBattlegroundState.Victory;

								MediaPlayer.Stop();
								_cExpTally.Tally(_cBattleData);
								OnBattleOver(cTeam.iId);
								break;
							}
					break;

					case EBattlegroundState.Victory:
						ProcessVictoryState();
					break;
				}

				// process the battle actions
				if(_cBattleData.eState != EBattlegroundState.Dialog)
					_cObjMgr.Process(cTime);
			}

			UpdateDebug(cTime);
			_tSlowMo = TimeSpan.Zero;
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

				// check to see if we have an effect ( this may be replaced with a queue or stack of the effects )
				if(_cBattleData.cAnimalEffect != null)
					_cBattleData.cAnimalEffect.Draw(_cSpriteBatch);

				// the mode's HUD
				DrawHud(_cSpriteBatch);

				// the hammer tally next to its icon
				_cSpriteBatch.DrawString(_cFont, string.Format("x {0}", _iHammerCtr), new Vector2(955, 35), Color.White);

				// if we are going to draw the active zones
				if(DataStore.cInstance.bZoneDisplay) {
					Vector2	tZone;
					foreach(Dictionary<int, Zone> cZoneList in _cBattleData.caActiveZones) {
						foreach(Zone cZone in cZoneList.Values) {
							tZone.X = cZone.cPoint.iX * (int)EZoneData.ZoneColWidth + 112;
							tZone.Y = cZone.cPoint.iY * (int)EZoneData.ZoneRowHeight + 70;

							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y, 1, (int)EZoneData.ZoneRowHeight), Color.White);
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X + (int)EZoneData.ZoneColWidth, (int)tZone.Y, 1, (int)EZoneData.ZoneRowHeight), Color.White);
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y, (int)EZoneData.ZoneColWidth, 1), Color.White);
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y + (int)EZoneData.ZoneRowHeight, (int)EZoneData.ZoneColWidth, 1), Color.White);
						}
					}
				}

				//// ddhj: kaiju -- F2 (GameShell.Update) toggle. Red dot = this combatant's real
				//// tCenter (what zones/attack-points/damage-range actually use); green line =
				//// facing per bDir, drawn toward +X when true, -X when false, so a sprite that's
				//// visibly flipped one way while this line points the other is the bug, caught
				//// on camera instead of argued about from code.
				if(DataStore.cInstance.bDebugCenters) {
					foreach(Team cDbgTeam in _cBattleData.caTeams) {
						foreach(Combatant cDbgCom in cDbgTeam.cActiveList.Values) {
							Vector2	tC = cDbgCom.tCenter;
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tC.X - 2, (int)tC.Y - 2, 4, 4), Color.Red);

							bool	bFacing = (cDbgCom is Trooper) && ((Trooper)cDbgCom).bDir;
							int		iLineX = bFacing ? (int)tC.X : (int)tC.X - 14;
							_cSpriteBatch.Draw(_cBorder, new Rectangle(iLineX, (int)tC.Y - 1, 14, 2), Color.Lime);
						}
					}
				}
			} _cSpriteBatch.End();

			_cSpriteBatch.Begin(); {
				// draw all objects in the manager
				_cObjMgr.DrawParticles(_cSpriteBatch);
			} _cSpriteBatch.End();
		}

		public virtual void Unload()
		{
			MediaPlayer.Stop();
			CleanCommon();
		}

		public virtual void ToggleControls()
		{
		}

		public virtual void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
		}

		public virtual void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;
		}

	// Shared machinery
		protected void BackToMenu()
		{
			_eState = ESceneStates.Inactive;
			DataStore.cInstance.cSceneMgr.RemoveScene(this);
		}

		/// <summary> dispose/clear the common battle state (rematches and unload both use this) </summary>
		protected void CleanCommon()
		{
			_cSpriteBatch.Dispose();
			_cBattleData.Clear();
			_cObjMgr.Clear();
			_cBuffContainerList.Clear();
		}

		bool WaitForBattleStart()
		{
			bool bReady = false;

			foreach(Team cTeam in _cBattleData.caTeams)
				foreach(Combatant nCombatant in cTeam.cActiveList.Values)
					bReady |= ((Trooper)nCombatant).cAiData.eState != EBattleAiStates.Ready;

			return bReady;
		}

		// the classic 8 buff containers, in the HUD slot positions
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

		void InputSystem_MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			if(_cCursor != null)
				_cCursor.Update(eMouseEvt.Location);
		}
	}
}
