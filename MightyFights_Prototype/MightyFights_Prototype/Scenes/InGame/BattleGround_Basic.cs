using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public partial class BattleGround_Basic : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cBackground;
		SpriteBatch		_cSpriteBatch;
		Dictionary<string, List<IDrawable>>		_cDrawList = new Dictionary<string,List<IDrawable>>();
		Dictionary<string, List<ICombatant>>	_cTrooperRef = new Dictionary<string,List<ICombatant>>();
		BattlegroundData	_cBattleData = new BattlegroundData();
		ObjectManagerInstance	_cObjMgr = new ObjectManagerInstance();
		Dictionary<EBuffEffects, BuffContainer>		_cBuffContainerList = new Dictionary<EBuffEffects,BuffContainer>();

		TimeSpan			_tVictoryElapsed = TimeSpan.Zero,
							_tSlowMo = TimeSpan.Zero;

		Cursor			_cCursor;

		////ddhj: debug data
		TimeSpan		_cTime = TimeSpan.Zero;
		int				_iFrameRate = 0,
						_iFrameCtr = 0;
		SpriteFont		_cFont;
		Texture2D		_cBorder;
		bool			_bUpdate = true;

		System.Windows.Forms.CheckBox		_cToggleLifeBar,
											_cToggleDamageNumbers,
											_cToggleSlowMo,
											_cToggleZones,
											_cToggleHealSpots,
											_cAcornRadCb,
											_cClawRadCb,
											_cWolfRadCb, 
											_cToadRadCb,
											_cSnakeRadCb,
											_cEagleRadCb,
											_cLionRadCb,
											_cDragRadCb;

		System.Windows.Forms.NumericUpDown	_cAcornRad, 
											_cClawRad,
											_cWolfRad, 
											_cToadRad,
											_cSnakeRad,
											_cEagleRad,
											_cLionRad,
											_cDragRad;

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}
	
		#region IGameScene Members
		public void Update(GameTime cTime)
		{

			// set the datastore elapsed time for the action and heuristic processing 
			DataStore.cInstance.cTime = cTime;

			// process the cursor
			_cCursor.Update(cTime);

			//// CBD, just a way to pause the screen for the moment
//			{
//				MouseState	cState = Mouse.GetState();
//				if( cState.LeftButton == ButtonState.Pressed )
//					_bUpdate = false;
//				else	_bUpdate = true;
//			}

			//// debug slow down the game
			if(_cToggleSlowMo.Checked) { 
				_tSlowMo += cTime.ElapsedGameTime;
				if(_tSlowMo < TimeSpan.FromMilliseconds(50)) 
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
								break;
							}
					break;

					case EBattlegroundState.Victory: 
						// left click is restart battle
						if(Mouse.GetState().LeftButton == ButtonState.Pressed)
							ResetBattle();
						else if(Mouse.GetState().RightButton == ButtonState.Pressed)
							BackToMenu();

						_tVictoryElapsed += cTime.ElapsedGameTime;
						if(_tVictoryElapsed > TimeSpan.FromMilliseconds(2000)) { 
							ResetBattle();
						}
					break;
				}

				// process the battle actions
				_cBattleData.cObjMgr.Process(cTime);
			}

			// this is for the debug 
			_cTime += cTime.ElapsedGameTime;
			if(_cTime > TimeSpan.FromSeconds(1)) { 
				_cTime -= TimeSpan.FromSeconds(1);
				_iFrameRate = _iFrameCtr;
				_iFrameCtr = 0;
			}
				
			_tSlowMo = TimeSpan.Zero;
		}

		public void Draw(GameTime cTime)
		{
			++_iFrameCtr;

			_cSpriteBatch.Begin(SpriteSortMode.BackToFront, null); { 
				_cSpriteBatch.Draw(_cBackground, new Vector2(112, 70), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, 1); 
			
				// draw all objects in the manager
				_cBattleData.cObjMgr.Draw(_cSpriteBatch);	

				//// development interface
				{
					int		iLHp = 0,
							iRHp = 0,
							iLPow = 0,
							iRPow = 0,
							iLFlee = 0,
							iRFlee = 0,
							iLHeal = 0,
							iRHeal = 0;

					foreach( Team cTeam in _cBattleData.caTeams )
						foreach( Trooper cTrooper in cTeam.cActiveList.Values )
							if( cTrooper.cTeam.iId == 0 )
							{
								iLHp += (int)cTrooper.cStats.fHp;
								iLPow += cTrooper.cStats.iPower;
								switch( cTrooper.cAiData.eState )
								{
								case EBattleAiStates.Flee:		++iLFlee;	break;
								case EBattleAiStates.Panting:	++iLHeal;	break;
								}
							}
							else	{
								iRHp += (int)cTrooper.cStats.fHp;
								iRPow += cTrooper.cStats.iPower;
								switch( cTrooper.cAiData.eState )
								{
								case EBattleAiStates.Flee:		++iRFlee;	break;
								case EBattleAiStates.Panting:	++iRHeal;	break;
								}
							}

					_cSpriteBatch.DrawString(_cFont, string.Format("fps: {0}", _iFrameRate ), new Vector2(900, 10), Color.White);
					_cSpriteBatch.DrawString(_cFont, string.Format("LeftArmy: {0}    Left HP: {1}    Left Pow: {2}    Left Engaging: {3}   Left Fleeing: {4}   Left Healing: {5}",
							_cBattleData.caTeams[0].cActiveList.Count, iLHp, iLPow, _cBattleData.caTeams[0].cActiveList.Count - iLFlee - iLHeal, iLFlee, iLHeal ), new Vector2(10, 10), Color.White);
					_cSpriteBatch.DrawString(_cFont, string.Format("RightArmy: {0}  Right HP: {1}  Right Pow:{2}   Right Engaging: {3}  Right Fleeing: {4}  Right Healing: {5}",
							_cBattleData.caTeams[1].cActiveList.Count, iRHp, iRPow,  _cBattleData.caTeams[1].cActiveList.Count - iRFlee - iRHeal, iRFlee, iRHeal ), new Vector2(10, 30), Color.White);
				}

				// if we are going to draw the active zones
				if(_cToggleZones.Checked) { 
					Vector2	tZone;
					foreach(Dictionary<IntPoint, Zone> cZoneList in _cBattleData.caActiveZones) { 
						foreach(IntPoint cZonePos in cZoneList.Keys) { 
							tZone.X = cZonePos.iX * (int)EZoneData.ZoneColWidth + 112;
							tZone.Y = cZonePos.iY * (int)EZoneData.ZoneRowHeight + 70;

							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y, 1, (int)EZoneData.ZoneRowHeight), Color.White);
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X + (int)EZoneData.ZoneColWidth, (int)tZone.Y, 1, (int)EZoneData.ZoneRowHeight), Color.White);
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y, (int)EZoneData.ZoneColWidth, 1), Color.White);
							_cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y + (int)EZoneData.ZoneRowHeight, (int)EZoneData.ZoneColWidth, 1), Color.White);
						}
					}
				}
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

			foreach( ICombatant nCombatant in _cBattleData.caTeams[0].cActiveList.Values )
			{
				cTrooper = (Trooper)nCombatant;

				// set an initial script for the trooper
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(147 + iX * 35, iY * 36 + 100), null));

				// set the trooper for battle
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				
				// increment our grid counters
				++iY;
				if(iY > 9) { 
					iY = 0;
					--iX;
				}
			}

			// reset out grid counters
			iX = iY = 0;
			iCount = 0;

			foreach( ICombatant nCombatant in _cBattleData.caTeams[1].cActiveList.Values )
			{
				cTrooper = (Trooper)nCombatant;

				// set an initial script for the trooper
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(567 + iX * 35, iY * 36 + 100), null));

				// set the trooper for battle
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				
				// increment our grid counters
				++iY;
				if(iY > 9) { 
					iY = 0;
					++iX;
				}
			}
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
			// radius check
			_cAcornRadCb = new System.Windows.Forms.CheckBox();
			_cAcornRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cAcornRadCb.Text = _cAcornRadCb.Name = "Rad";
			_cAcornRadCb.AutoSize = true;
			_cAcornRadCb.Tag = cTmpContainer;
			_cAcornRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cAcornRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cAcornRadCb);
			// radius value
			_cAcornRad = new System.Windows.Forms.NumericUpDown();
			_cAcornRad.Size = new System.Drawing.Size(35, 20);
			_cAcornRad.Location = new System.Drawing.Point(_cAcornRadCb.Location.X + _cAcornRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cAcornRad.Minimum = 1;
			_cAcornRad.Maximum = 100;
			_cAcornRad.Tag = cTmpContainer;
			_cAcornRad.ValueChanged += new EventHandler(ProcessValChange);
			_cAcornRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cAcornRad);

			_cBuffContainerList.Add(EBuffEffects.Crab_Claw, cTmpContainer = new BuffContainer(new Vector2(232, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cClawRadCb = new System.Windows.Forms.CheckBox();
			_cClawRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cClawRadCb.Text = _cClawRadCb.Name = "Rad";
			_cClawRadCb.AutoSize = true;
			_cClawRadCb.Tag = cTmpContainer;
//			_cClawRadCb.Checked = true;
			_cClawRadCb.CheckedChanged += new EventHandler(ProcessCheck);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cClawRadCb);
			// radius value
			_cClawRad = new System.Windows.Forms.NumericUpDown();
			_cClawRad.Size = new System.Drawing.Size(35, 20);
			_cClawRad.Location = new System.Drawing.Point(_cClawRadCb.Location.X + _cClawRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cClawRad.Minimum = 1;
			_cClawRad.Maximum = 100;
			_cClawRad.Tag = cTmpContainer;
			_cClawRad.ValueChanged += new EventHandler(ProcessValChange);
			_cClawRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cClawRad);

			_cBuffContainerList.Add(EBuffEffects.Wolf_Ear, cTmpContainer = new BuffContainer(new Vector2(332, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cWolfRadCb = new System.Windows.Forms.CheckBox();
			_cWolfRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cWolfRadCb.Text = _cWolfRadCb.Name = "Rad";
			_cWolfRadCb.AutoSize = true;
			_cWolfRadCb.Tag = cTmpContainer;
			_cWolfRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cWolfRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cWolfRadCb);
			// radius value
			_cWolfRad = new System.Windows.Forms.NumericUpDown();
			_cWolfRad.Size = new System.Drawing.Size(35, 20);
			_cWolfRad.Location = new System.Drawing.Point(_cWolfRadCb.Location.X + _cWolfRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cWolfRad.Minimum = 1;
			_cWolfRad.Maximum = 100;
			_cWolfRad.Tag = cTmpContainer;
			_cWolfRad.ValueChanged += new EventHandler(ProcessValChange);
			_cWolfRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cWolfRad);

			_cBuffContainerList.Add(EBuffEffects.Toad_Eye, cTmpContainer = new BuffContainer(new Vector2(432, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cToadRadCb = new System.Windows.Forms.CheckBox();
			_cToadRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cToadRadCb.Text = _cToadRadCb.Name = "Rad";
			_cToadRadCb.AutoSize = true;
			_cToadRadCb.Tag = cTmpContainer;
			_cToadRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cToadRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToadRadCb);
			// radius value
			_cToadRad = new System.Windows.Forms.NumericUpDown();
			_cToadRad.Size = new System.Drawing.Size(35, 20);
			_cToadRad.Location = new System.Drawing.Point(_cToadRadCb.Location.X + _cToadRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cToadRad.Minimum = 1;
			_cToadRad.Maximum = 100;
			_cToadRad.Tag = cTmpContainer;
			_cToadRad.ValueChanged += new EventHandler(ProcessValChange);
			_cToadRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToadRad);

			_cBuffContainerList.Add(EBuffEffects.Snake_Fang, cTmpContainer = new BuffContainer(new Vector2(532, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cSnakeRadCb = new System.Windows.Forms.CheckBox();
			_cSnakeRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cSnakeRadCb.Text = _cSnakeRadCb.Name = "Rad";
			_cSnakeRadCb.AutoSize = true;
			_cSnakeRadCb.Tag = cTmpContainer;
			_cSnakeRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cSnakeRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cSnakeRadCb);
			// radius value
			_cSnakeRad = new System.Windows.Forms.NumericUpDown();
			_cSnakeRad.Size = new System.Drawing.Size(35, 20);
			_cSnakeRad.Location = new System.Drawing.Point(_cSnakeRadCb.Location.X + _cSnakeRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cSnakeRad.Minimum = 1;
			_cSnakeRad.Maximum = 100;
			_cSnakeRad.Tag = cTmpContainer;
			_cSnakeRad.ValueChanged += new EventHandler(ProcessValChange);
			_cSnakeRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cSnakeRad);

			_cBuffContainerList.Add(EBuffEffects.Eagle_Feather, cTmpContainer = new BuffContainer(new Vector2(632, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cEagleRadCb = new System.Windows.Forms.CheckBox();
			_cEagleRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cEagleRadCb.Text = _cEagleRadCb.Name = "Rad";
			_cEagleRadCb.AutoSize = true;
			_cEagleRadCb.Tag = cTmpContainer;
			_cEagleRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cEagleRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cEagleRadCb);
			// radius value
			_cEagleRad = new System.Windows.Forms.NumericUpDown();
			_cEagleRad.Size = new System.Drawing.Size(35, 20);
			_cEagleRad.Location = new System.Drawing.Point(_cEagleRadCb.Location.X + _cEagleRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cEagleRad.Minimum = 1;
			_cEagleRad.Maximum = 100;
			_cEagleRad.Tag = cTmpContainer;
			_cEagleRad.ValueChanged += new EventHandler(ProcessValChange);
			_cEagleRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cEagleRad);

			_cBuffContainerList.Add(EBuffEffects.Lion_Paw, cTmpContainer = new BuffContainer(new Vector2(732, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cLionRadCb = new System.Windows.Forms.CheckBox();
			_cLionRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cLionRadCb.Text = _cLionRadCb.Name = "Rad";
			_cLionRadCb.AutoSize = true;
			_cLionRadCb.Tag = cTmpContainer;
			_cLionRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cLionRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cLionRadCb);
			// radius value
			_cLionRad = new System.Windows.Forms.NumericUpDown();
			_cLionRad.Size = new System.Drawing.Size(35, 20);
			_cLionRad.Location = new System.Drawing.Point(_cLionRadCb.Location.X + _cLionRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cLionRad.Minimum = 1;
			_cLionRad.Maximum = 100;
			_cLionRad.Tag = cTmpContainer;
			_cLionRad.ValueChanged += new EventHandler(ProcessValChange);
			_cLionRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cLionRad);

			_cBuffContainerList.Add(EBuffEffects.Dragon_Wing, cTmpContainer = new BuffContainer(new Vector2(832, 519)));
			cTmpContainer.cTexRef = cContent.Load<Texture2D>(@"In Game\Buffs\ItemBox");
			cTmpContainer.sTexName = @"In Game\Buffs\ItemBox";
			cTmpContainer.cFrame = new Frame(cTmpContainer.cTexRef.Bounds, new Vector2(cTmpContainer.cTexRef.Bounds.Width / 2, cTmpContainer.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpContainer.cTexRef.Bounds.Width, cTmpContainer.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			_cObjMgr.AddClickObject(cTmpContainer, cTmpContainer.ProcessClick);
			// radius check
			_cDragRadCb = new System.Windows.Forms.CheckBox();
			_cDragRadCb.Location = new System.Drawing.Point((int)cTmpContainer.tPos.X - 10, (int)cTmpContainer.tPos.Y - 20);
			_cDragRadCb.Text = _cDragRadCb.Name = "Rad";
			_cDragRadCb.AutoSize = true;
			_cDragRadCb.Tag = cTmpContainer;
			_cDragRadCb.CheckedChanged += new EventHandler(ProcessCheck);
//			_cDragRadCb.Checked = true;
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cDragRadCb);
			// radius value
			_cDragRad = new System.Windows.Forms.NumericUpDown();
			_cDragRad.Size = new System.Drawing.Size(35, 20);
			_cDragRad.Location = new System.Drawing.Point(_cDragRadCb.Location.X + _cDragRadCb.Size.Width + 3, (int)cTmpContainer.tPos.Y - 20);
			_cDragRad.Minimum = 1;
			_cDragRad.Maximum = 100;
			_cDragRad.Tag = cTmpContainer;
			_cDragRad.ValueChanged += new EventHandler(ProcessValChange);
			_cDragRad.Value = 50;//cRand.Next(100);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cDragRad);
		}

		public bool Init()
		{
			DataStore		cData = DataStore.cInstance;
			ContentManager	cContent = cData.cContent;
			GraphicsDevice	cGraphics = cData.cGraphics;
			Trooper			cTmpTrooper = null;
			TemplateConfig	cLeft = cData.cLeftConfig, 
							cRight = cData.cRightConfig;
			ObjectManager	cObjMgr = ObjectManager.cInstance;
			Team			cTeam;

			_cBattleData.cObjMgr = _cObjMgr;
			_cBattleData.dlBuffClick = ProcessBuffClick;

			try { 
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass 800x436");

				// set the battle data to the datastore for reference 
				DataStore.cInstance.cBattleData = _cBattleData;

				cTeam = _cBattleData.caTeams[0];
				// make a block of troopers
				for(int i = 0; i < cLeft.iCount; ++i) { 
					// add the newly created trooper to the active list and set some initial battle data
					cTmpTrooper = new Trooper(cObjMgr.iCurObjId, cTeam, cObjMgr.CreateTemplate(cLeft));
					cTeam.cActiveList.Add(cTmpTrooper.iId, cTmpTrooper);
					cTmpTrooper.tPos = new Vector2(25, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);

					// add the new object to the object manager
					_cObjMgr.AddObject(cTmpTrooper);
				}
				for( int iCount = 0; iCount < 2; ++iCount )
				{
					Priest	cHealer = new Priest( cObjMgr.iCurObjId, 1500, 7, 1.6f, .1f, new Vector2( 50, 180 + 120 * iCount ), cTeam, cObjMgr.CreateTemplate(cLeft) );
					cTeam.cHealerList.Add( cHealer.iId, cHealer );
					_cObjMgr.AddObject(cHealer);
				}

				cTeam = _cBattleData.caTeams[1];
				// make a block of opponents
				for(int i = 0; i < cRight.iCount; ++i) { 
					// set the opponents to the acitve list 
					cTmpTrooper = new Trooper(cObjMgr.iCurObjId, cTeam, cObjMgr.CreateTemplate(cRight));
					cTeam.cActiveList.Add(cTmpTrooper.iId, cTmpTrooper);
					cTmpTrooper.tPos = new Vector2(950, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);

					// add to the object manger
					_cObjMgr.AddObject(cTmpTrooper);
				}
				for( int iCount = 0; iCount < 2; ++iCount )
				{
					Priest	cHealer = new Priest( cObjMgr.iCurObjId, 1500, 7, 1.6f, .1f, new Vector2( 880, 180 + 120 * iCount ), cTeam, cObjMgr.CreateTemplate(cRight) );
					cTeam.cHealerList.Add( cHealer.iId, cHealer );
					_cObjMgr.AddObject(cHealer);
				}

				// create the containers 
				_cBattleData.cBuffContainers = _cBuffContainerList;
				CreateBuffContainers();

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

				//// ddhj: configrable items for the battle draw
				_cToggleLifeBar = new System.Windows.Forms.CheckBox();
				_cToggleLifeBar.Location = new System.Drawing.Point(0, 60);
				_cToggleLifeBar.Text = _cToggleLifeBar.Name = "Toggle Life Bar";
				_cToggleLifeBar.AutoSize = true;
				_cToggleLifeBar.CheckedChanged += new EventHandler(ToggleLifeBarChange);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleLifeBar);

				_cToggleDamageNumbers = new System.Windows.Forms.CheckBox();
				_cToggleDamageNumbers.Location = new System.Drawing.Point(0, _cToggleLifeBar.Location.Y + 20);
				_cToggleDamageNumbers.Text = _cToggleDamageNumbers.Name = "Toggle Dmg #";
				_cToggleDamageNumbers.AutoSize = true;
				_cToggleDamageNumbers.CheckedChanged += new EventHandler(ToggleDamageNumbersChange);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleDamageNumbers);

				_cToggleSlowMo = new System.Windows.Forms.CheckBox();
				_cToggleSlowMo.Location = new System.Drawing.Point(0, _cToggleDamageNumbers.Location.Y + 20);
				_cToggleSlowMo.Text = _cToggleSlowMo.Name = "Toggle Slow Mo";
				_cToggleSlowMo.AutoSize = true;
				_cToggleSlowMo.CheckedChanged += new EventHandler(ToggleSlowMoChange);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleSlowMo);

				_cToggleZones = new System.Windows.Forms.CheckBox();
				_cToggleZones.Location = new System.Drawing.Point(0, _cToggleSlowMo.Location.Y + 20);
				_cToggleZones.Text = _cToggleZones.Name = "Toggle Zones";
				_cToggleZones.AutoSize = true;
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleZones);

				_cToggleHealSpots = new System.Windows.Forms.CheckBox();
				_cToggleHealSpots.Location = new System.Drawing.Point(0, _cToggleZones.Location.Y + 20);
				_cToggleHealSpots.Text = _cToggleHealSpots.Name = "Toggle Heal Loc";
				_cToggleHealSpots.AutoSize = true;
				_cToggleHealSpots.CheckedChanged += new EventHandler(ToggleHealSpots);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleHealSpots);

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
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cToggleLifeBar);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cToggleDamageNumbers);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cToggleSlowMo);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cToggleZones);
		}

		void ResetBattle()
		{
			PartialClean();
			Init();
			_cBattleData.eState = EBattlegroundState.Init;
			_tVictoryElapsed = TimeSpan.Zero;
		}

		void CleanData()
		{
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
