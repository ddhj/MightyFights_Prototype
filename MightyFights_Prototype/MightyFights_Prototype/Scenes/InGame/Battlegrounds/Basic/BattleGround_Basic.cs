// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMercury;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public partial class BattleGround_Basic : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cBackground,
						_cHudBorder;
		SpriteBatch		_cSpriteBatch;
		BattleObjectManager	_cObjMgr = new BattleObjectManager();
		Cursor			_cCursor;
		BattlegroundData	_cBattleData = new BattlegroundData();

		Dictionary<string, List<IDrawable>>		_cDrawList = new Dictionary<string,List<IDrawable>>();
		Dictionary<string, List<Combatant>>	_cTrooperRef = new Dictionary<string,List<Combatant>>();
		Dictionary<EBuffEffects, BuffContainer>		_cBuffContainerList = new Dictionary<EBuffEffects,BuffContainer>();
		List<TrooperTemplate>	_cTmpList = new List<TrooperTemplate>();

		Random		_cRand = DataStore.cInstance.cRand;
		Song		_cBgm;
		SoundEffectInstance		_cBuffSfx,
								_cHammerSfx;

		////ddhj: debug data
		TimeSpan		_tTime = TimeSpan.Zero,
						_tVictoryElapsed = TimeSpan.Zero,
						_tSlowMo = TimeSpan.Zero,
						_tEllapsedTime,
						_tOneSecond = TimeSpan.FromSeconds( 1 ),
						_tSlowMoTrigger = TimeSpan.FromMilliseconds( 50 );

		int				_iFrameRate = 0,
						_iFrameCtr = 0,
						_iHammerCtr = 0;
		SpriteFont		_cFont;
		Texture2D		_cBorder;
		bool			_bUpdate = true;
		StatsDialog							_cDlg;
		DebugData							_cDebugDialog;
		System.Windows.Forms.Button			_cDebugButton;
		EBattlegroundState					_eOldState;

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}
	
		#region IGameScene Members
		public void Update(GameTime cTime)
		{

			// set the datastore elapsed time for the action and heuristic processing 
			DataStore.cInstance.tTime = cTime;

			// process the cursor
			_cCursor.Update(cTime);

			//// CBD, just a way to pause the screen for the moment
//			{
//				MouseState	cState = Mouse.GetState();
//				if( cState.LeftButton == ButtonState.Pressed )
//					_bUpdate = false;
//				else	_bUpdate = true;
//			}

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

			if( _bUpdate )
			{
				// process the battle state
				switch(_cBattleData.eState) { 
					case EBattlegroundState.Init:
						if(!WaitForBattleStart()) 
							_cBattleData.eState = EBattlegroundState.Battle;				
					break;

					case EBattlegroundState.Battle:
						// check to see if the battle is over 
						foreach( Team cTeam in _cBattleData.caTeams )
							if( cTeam.cActiveList.Count == 0 ) { 
								_cBattleData.eState = EBattlegroundState.Victory;

								MediaPlayer.Stop( );
								_cBgm = DataManager.cInstance.CreateMusic( "Minibossies_FinalFantasy6_VictoryFanfare" );
								if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play( _cBgm );

								_cDlg.InitGridData();

								// debug for exp 
								WriteExpData();
								break;
							}
					break;

					case EBattlegroundState.Victory: 
						// left click is restart battle
						if(Mouse.GetState().LeftButton == ButtonState.Pressed)
							ResetBattle();
						else if(Mouse.GetState().RightButton == ButtonState.Pressed)
							BackToMenu();
						else if(Mouse.GetState().MiddleButton == ButtonState.Pressed)
							_cDlg.ShowDialog();

						//_tVictoryElapsed += tTime.ElapsedGameTime;
						//if(_tVictoryElapsed > TimeSpan.FromMilliseconds(2000)) { 
						//    ResetBattle();
						//}
					break;
				}

				// process the battle actions
				if(_cBattleData.eState != EBattlegroundState.Dialog) 
					_cBattleData.cObjMgr.Process(cTime);
			}

			// this is for the debug 
			_tTime += cTime.ElapsedGameTime;
			if(_tTime > _tOneSecond) { 
				_tTime -= _tOneSecond;
				_iFrameRate = _iFrameCtr;
				_iFrameCtr = 0;
			}
				
			_tSlowMo = TimeSpan.Zero;
		}

		public void Draw(GameTime cTime)
		{
			++_iFrameCtr;
			if( _cBattleData.eState == EBattlegroundState.Battle )
				_tEllapsedTime += cTime.ElapsedGameTime;

			_cSpriteBatch.Begin(SpriteSortMode.BackToFront, null); { 
				if(DataStore.cInstance.bShowBg) { 
					_cSpriteBatch.Draw(_cBackground, new Vector2(112, 70), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, 1); 
					_cSpriteBatch.Draw(_cHudBorder, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, .99f);
				}
			
				// draw all objects in the manager
				_cBattleData.cObjMgr.Draw(_cSpriteBatch);	

				// check to see if we have an effect ( this may be replaced with a queue or stack of the effects )
				if(_cBattleData.cAnimalEffect != null) 
					_cBattleData.cAnimalEffect.Draw(_cSpriteBatch);

				//// development interface
				{
					int		iLHp = 0,
							iRHp = 0,
							iLPow = 0,
							iRPow = 0,
							iLAc = 0,
							iRAc = 0;

					foreach( Team cTeam in _cBattleData.caTeams )
						foreach( Trooper cTrooper in cTeam.cActiveList.Values )
							if( cTrooper.cTeam.iId == 0 )
							{
								iLHp += (int)cTrooper.cStats.fHp;
								iLPow += cTrooper.cStats.iPower;
								iLAc += cTrooper.cStats.iArmorClass;
							}
							else	{
								iRHp += (int)cTrooper.cStats.fHp;
								iRPow += cTrooper.cStats.iPower;
								iRAc += cTrooper.cStats.iArmorClass;
							}

					_cSpriteBatch.DrawString(_cFont, string.Format("LeftArmy: {0}    Left HP: {1}    Left Pow: {2}    Left AC: {3}",
							_cBattleData.caTeams[0].cActiveList.Count, iLHp, iLPow, iLAc ), new Vector2(10, 10), Color.White);
					_cSpriteBatch.DrawString(_cFont, string.Format("RightArmy: {0}  Right HP: {1}  Right Pow:{2}   Right AC: {3}",
							_cBattleData.caTeams[1].cActiveList.Count, iRHp, iRPow, iRAc ), new Vector2(10, 30), Color.White);
					_cSpriteBatch.DrawString(_cFont, string.Format("fps: {0}", _iFrameRate ), new Vector2(820, 10), Color.White);
					_cSpriteBatch.DrawString(_cFont, string.Format("Time: {0}", _tEllapsedTime.ToString( "c" )), new Vector2(860, 10), Color.White);
					_cSpriteBatch.DrawString(_cFont, string.Format("x {0}", _iHammerCtr), new Vector2(955, 35), Color.White);
				}

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
			} _cSpriteBatch.End();

			_cSpriteBatch.Begin(); { 
				// draw all objects in the manager
				_cBattleData.cObjMgr.DrawParticles(_cSpriteBatch);
			} _cSpriteBatch.End();
		}

		bool WaitForBattleStart()
		{
			bool bReady = false;

			foreach( Team cTeam in _cBattleData.caTeams )
				foreach( Trooper cTrooper in cTeam.cActiveList.Values )
					bReady |= cTrooper.cAiData.eState != EBattleAiStates.Ready;

			return bReady;
		}

		void SetBattleStart()
		{
			int		iCount = 0,
					iX = 9, iY = 0;
			Trooper	cTrooper;

			foreach( Combatant nCombatant in _cBattleData.caTeams[0].cActiveList.Values )
			{
				cTrooper = (Trooper)nCombatant;

				// set an initial script for the trooper
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(147 + iX * 17, iY * 18 + 100), null));

				// set the trooper for battle
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				
				// increment our grid counters
				++iY;
				if(iY > 19) { 
					iY = 0;
					--iX;

					if(( iX & 1 ) == 1 )
						iX -= 2;
				}
			}

			// reset out grid counters
			iX = iY = 0;
			iCount = 0;

			foreach( Combatant nCombatant in _cBattleData.caTeams[1].cActiveList.Values )
			{
				cTrooper = (Trooper)nCombatant;

				// set an initial script for the trooper
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(717 + iX * 17, iY * 18 + 100), null));

				// set the trooper for battle
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				
				// increment our grid counters
				++iY;
				if(iY > 19) { 
					iY = 0;
					if(( iX & 1 ) == 1 )
						iX += 2;
					++iX;
				}
			}

			string[] saMusic = new string[] { "Final_Fantasy_4_The_Flying_Machine_OC_ReMix", "Final_Fantasy_4_Treason_OC_ReMix", "Final_Fantasy_6_Desertion_OC_ReMix", "Final_Fantasy_6_Smooth_Alexander_OC_ReMix",
																				"Final_Fantasy_7_Fight_On_OC_ReMix",  "Final_Fantasy_Duque_Battle_OC_ReMix", "Final_Fantasy_Hostility_OC_ReMix", "Final_Fantasy_The_Beginning_of_a_Legacy_OC_ReMix" };
			//DataStore.cInstance.cBgm = 
			//    _cBgm = ObjectCreationManager.cInstance.CreateMusic( saMusic[DataStore.cInstance.cRand.Next( saMusic.Length )] );
			DataStore.cInstance.cBgm = 
			    _cBgm = DataManager.cInstance.CreateMusic( "battle1" );
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = .6f;
			if(DataStore.cInstance.bPlayMusic)	MediaPlayer.Play( _cBgm );
		}

		void CreateBuffContainers()
		{
			ContentManager	cContent = DataStore.cInstance.cContent;
			BuffContainer	cTmpContainer;

			_cBuffContainerList.Add(EBuffEffects.Squirrel_Acorn, cTmpContainer = new BuffContainer(new Vector2(132, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Crab_Claw, cTmpContainer = new BuffContainer(new Vector2(232, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Wolf_Ear, cTmpContainer = new BuffContainer(new Vector2(332, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Toad_Eye, cTmpContainer = new BuffContainer(new Vector2(432, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Snake_Fang, cTmpContainer = new BuffContainer(new Vector2(532, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Eagle_Feather, cTmpContainer = new BuffContainer(new Vector2(632, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Lion_Paw, cTmpContainer = new BuffContainer(new Vector2(732, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);

			_cBuffContainerList.Add(EBuffEffects.Dragon_Wing, cTmpContainer = new BuffContainer(new Vector2(832, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
		}
		
		void InitTemplateList(List<TemplateCfgMaster> cList)
		{
			_cTmpList.Clear();
			foreach(TemplateCfgMaster cTmplateCfg in cList)
				for(int i = 0; i < cTmplateCfg.iCount; ++i)
					_cTmpList.Add(DataManager.cInstance.CreateTemplate(cTmplateCfg));
		}

		bool GetTrooperTemplate(out TrooperTemplate cTemplate)
		{
			cTemplate = null;
			if(_cTmpList.Count == 0)
				return false;

			int iIndex = _cRand.Next(_cTmpList.Count);
			cTemplate = _cTmpList[iIndex];
			_cTmpList.RemoveAt(iIndex);
			return true;
		}

		public bool Init()
		{
			DataStore		cData = DataStore.cInstance;
			ContentManager	cContent = cData.cContent;
			GraphicsDevice	cGraphics = cData.cGraphics;
			Trooper			cTmpTrooper = null;
			DataManager	cObjMgr = DataManager.cInstance;
			Team			cTeam;
			Priest			cHealer;
			TrooperTemplate cTemplate;

			_cBattleData.cObjMgr = _cObjMgr;
			_cBattleData.dlBuffClick = ProcessBuffClick;
			_cBattleData.dlDropClick = ProcessHammerClick;

			// set some events for dialog processing
			_cDlg = new StatsDialog();
			_cDebugDialog = new DebugData();
			_cDlg.FormClosed += new System.Windows.Forms.FormClosedEventHandler(_cDlg_FormClosed);
			_cDlg.Shown += new EventHandler(_cDlg_Shown);
			_cDebugDialog.FormClosed += new System.Windows.Forms.FormClosedEventHandler(_cDlg_FormClosed);
			_cDebugDialog.Shown += new EventHandler(_cDlg_Shown);
			_cDebugButton = new System.Windows.Forms.Button();
			_cDebugButton.Click += new EventHandler(_cDebugButton_Click);
			_cDebugButton.Location = new System.Drawing.Point(10, 550);
			_cDebugButton.Text = "Debug";
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cDebugButton);

			_tEllapsedTime = TimeSpan.Zero;
			try { 
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass 800x436");
				_cHudBorder = cContent.Load<Texture2D>(@"In Game\Hud\border_jackson");


				_cObjMgr.CreateParticleManager( );

				_cBuffSfx = DataManager.cInstance.CreateSfx( "Magic Wand Noise-SoundBible.com-375928671" );
				_cHammerSfx = DataManager.cInstance.CreateSfx( "Electronic_Chime-KevanGC-495939803" );
				_cBuffSfx.Volume = _cHammerSfx.Volume = .4f;


				// set the battle data to the datastore for reference 
				DataStore.cInstance.cBattleData = _cBattleData;

				cTeam = _cBattleData.caTeams[0];
				// make a block of troopers
				InitTemplateList(cData.cLSteward.cTemplates);
				while(GetTrooperTemplate(out cTemplate)) { 
					// add the newly created trooper to the active list and set some initial battle data
					cTmpTrooper = new Trooper(cObjMgr.iCurObjId, cTeam, cTemplate);
					cTeam.cActiveList.Add(cTmpTrooper.iId, cTmpTrooper);
					// add to the member list just for debug maybe
					cTeam.cMembers.Add(cTmpTrooper);
					cTmpTrooper.tPos = new Vector2(25, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);

					// add the new object to the object manager
					_cObjMgr.AddObject(cTmpTrooper);
				}

				for( int iCount = 0; iCount < 2; ++iCount )
				{
					cHealer = cObjMgr.CreatePriest(new Vector2( 65, 180 + 120 * iCount ), cTeam,   (int)( 40f * 7 * 6 ), 7, 40f, 1.3333f, _cBattleData);
					cTeam.cHealerList.Add( cHealer.iId, cHealer );
					_cObjMgr.AddObject(cHealer);
				}

				cTeam = _cBattleData.caTeams[1];
				// make a block of opponents
				InitTemplateList(cData.cRSteward.cTemplates);
				while(GetTrooperTemplate(out cTemplate)) { 
					// set the opponents to the acitve list 
					cTmpTrooper = new Trooper(cObjMgr.iCurObjId, cTeam, cTemplate);
					cTeam.cActiveList.Add(cTmpTrooper.iId, cTmpTrooper);
					// add to the member list just for debug maybe
					cTeam.cMembers.Add(cTmpTrooper);
					cTmpTrooper.tPos = new Vector2(950, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);

					// add to the object manger
					_cObjMgr.AddObject(cTmpTrooper);
				}
				for( int iCount = 0; iCount < 2; ++iCount )
				{
					cHealer = cObjMgr.CreatePriest(new Vector2( 830, 180 + 120 * iCount ), cTeam,  (int)( 40f * 7 * 6 ), 7, 40f, 1.3333f, _cBattleData);
					cTeam.cHealerList.Add( cHealer.iId, cHealer );
					_cObjMgr.AddObject(cHealer);
				}

				// create the containers 
				_cBattleData.cBuffContainers = _cBuffContainerList;
				CreateBuffContainers();

				// add the hammer
				_cObjMgr.AddObject(DataManager.cInstance.CreateHammerIcon());

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(_cCursor);
				_cBattleData.cCursor = _cCursor;

				//// ddhj: load in some debug data
				_cFont = cContent.Load<SpriteFont>(@"Shared\DebugFont");
				DataStore.cInstance.cBorder = _cBorder = new Texture2D(cGraphics, 1, 1);
				_cBorder.SetData<Color>(new[] { Color.White });

				// set the start of battle
				SetBattleStart();
			} catch(Exception xEx) { 
				System.Windows.Forms.MessageBox.Show(xEx.ToString());
			}

			return true;
		}

		void BackToMenu()
		{
			_eState = ESceneStates.Inactive;
			DataStore.cInstance.cSceneMgr.RemoveScene(this);
		}

		void PartialClean()
		{
			_cSpriteBatch.Dispose();
			_cTrooperRef.Clear();
			_cBattleData.Clear();
			_cObjMgr.Clear();
			_cBuffContainerList.Clear();
			_cDlg = null;
			_cDebugDialog = null;

			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cDebugButton);
		}

		void ResetBattle()
		{
			MediaPlayer.Stop( );
			PartialClean();
			Init();
			_cBattleData.eState = EBattlegroundState.Init;
			_tVictoryElapsed = TimeSpan.Zero;
		}

		void CleanData()
		{
			MediaPlayer.Stop( );
			_cSpriteBatch.Dispose();
			_cDrawList.Clear();
			_cTrooperRef.Clear();
			_cBattleData.Clear();
			_cObjMgr.Clear();
		}

		public void Unload()
		{
			CleanData();
		}

		public void ToggleControls()
		{

		}

		#endregion
	}
}
