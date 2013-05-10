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
	class BattleGround_Basic : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cBackground;
		SpriteBatch		_cSpriteBatch;
		Dictionary<string, List<IDrawable>>		_cDrawList = new Dictionary<string,List<IDrawable>>();
		List<Trooper>							_cActiveList = new List<Trooper>();
		Dictionary<string, List<ICombatant>>	_cTrooperRef = new Dictionary<string,List<ICombatant>>();
		BattlegroundData	_cBattleData = new BattlegroundData();
		ObjectManagerInstance	_cObjMgr = new ObjectManagerInstance();

		TimeSpan			_tVictoryElapsed = TimeSpan.Zero,
							_tSlowMo = TimeSpan.Zero;

		Cursor			_cCursor;

		////ddhj: debug data
		TimeSpan		_cTime = TimeSpan.Zero;
		int				_iFrameRate = 0,
						_iFrameCtr = 0,
						_iAX, 
						_iAY,
						_iOX, 
						_iOY;
		SpriteFont		_cFont;
		Texture2D		_cBorder;
		bool			_bUpdate = true;

		System.Windows.Forms.CheckBox		_cToggleLifeBar,
											_cToggleDamageNumbers,
											_cToggleSlowMo;

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}
	
		#region IGameScene Members

		public void Update(GameTime cTime)
		{

			// set the datastore elapsed time for the action and huristic processing 
			DataStore.cInstance.cTime = cTime;

			// process the cursor
			_cCursor.Update(cTime);

			//// CBD, just a way to pause the screen for the moment
			{
				MouseState	cState = Mouse.GetState();
				if( cState.LeftButton == ButtonState.Pressed )
					_bUpdate = false;
				else	_bUpdate = true;
			}

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
						foreach(List<ICombatant> naRefList in _cBattleData.naMasterLists)
							if(naRefList.Count == 0) { 
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
				_cSpriteBatch.Draw(_cBackground, new Vector2(112, 84), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, 1); 
			
				// draw all objects in the manager
				_cBattleData.cObjMgr.Draw(_cSpriteBatch);	

				_cSpriteBatch.DrawString(_cFont, string.Format("fps:{0} : LeftArmy:{1} : RightArmy:{2}", _iFrameRate, _cBattleData.naArmyRef.Count, _cBattleData.naOpponentsRef.Count), 
					new Vector2(10, 10), Color.White);				

				//// ddhj: debug draw
				// lets draw the active zones 
				//Vector2	tZone;
				//foreach(Dictionary<IntPoint, Zone> cZoneList in _cBattleData.caActiveZones) { 
				//    foreach(IntPoint cZonePos in cZoneList.Keys) { 
				//        tZone.X = cZonePos.iX * (int)EZoneData.ZoneColWidth + 112;
				//        tZone.Y = cZonePos.iY * (int)EZoneData.ZoneRowHeight + 84;

				//        _cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y, 1, (int)EZoneData.ZoneRowHeight), Color.White);
				//        _cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X + (int)EZoneData.ZoneColWidth, (int)tZone.Y, 1, (int)EZoneData.ZoneRowHeight), Color.White);
				//        _cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y, (int)EZoneData.ZoneColWidth, 1), Color.White);
				//        _cSpriteBatch.Draw(_cBorder, new Rectangle((int)tZone.X, (int)tZone.Y + (int)EZoneData.ZoneRowHeight, (int)EZoneData.ZoneColWidth, 1), Color.White);
				//    }
				//}
			} _cSpriteBatch.End();
		}

		bool WaitForBattleStart()
		{
			bool bReady = false;

			foreach(Trooper cTrooper in _cActiveList)
			    bReady |= cTrooper.cAiData.eState != EBattleAiStates.Ready;

			return bReady;
		}

		void SetBattleStart()
		{
			List<ICombatant>	naTmpList = _cBattleData.naArmyRef,
								naTmpOpponents = new List<ICombatant>();
			int					iCount = 0,
								iIndex;
			Trooper				cTrooper;
			Random				cRand = new Random();

			for(int i = 0; i < _iAX; ++i)
				for(int j = 0; j < _iAY; ++j) { 
					iIndex = i*_iAY+j;
					cTrooper = (Trooper)naTmpList[iIndex];

					// set an initial script for the trooper
					cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
					cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(200 - i * 35 + 25 + 112, j * 25 + 84), null));

					// set the trooper for battle
					cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				}

			iCount = 0;
			naTmpList = _cBattleData.naOpponentsRef;
			for(int i = 0; i < _iOX; ++i)
				for(int j = 0; j < _iOY; ++j) { 
					cTrooper = (Trooper)naTmpList[i*_iOY+j];

					// set an initial script for the trooper
					cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
					cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(400 + i * 35 + 25 + 112, j * 25 + 84), null));

					// set the trooper for battle
					cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				}
		}

		public bool Init()
		{
			ContentManager	cContent = DataStore.cInstance.cContent;
			GraphicsDevice	cGraphics = DataStore.cInstance.cGraphics;
			Trooper			cTmpTrooper;
			TemplateConfig	cLeft = DataStore.cInstance.cLeftConfig, 
							cRight = DataStore.cInstance.cRightConfig;

			_cBattleData.cObjMgr = _cObjMgr;

			// this is for quick action
			Random cRand = new Random();
			_iAX = cRand.Next(7) + 2;
			_iAY = cRand.Next(13) + 8;
			_iOX = cRand.Next(7) + 2;
			_iOY = cRand.Next(13) + 8;

			try { 
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass_large");

				// set the battle data to the datastore for reference 
				DataStore.cInstance.cBattleData = _cBattleData;
				
				// make a block of troopers
				for(int i = 0; i < _iAX; ++i)
					for(int j = 0; j < _iAY; ++j) { 
						// add the newly created trooper to the active list and set some initial battle data
						_cActiveList.Add(cTmpTrooper = new Trooper(ObjectManager.cInstance.CreateTemplate(cLeft)));
						cTmpTrooper.tPos = new Vector2(25, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);
						cTmpTrooper.iArmyIndex = 0;
						cTmpTrooper.iOpponentIndex = 1;

						// add the new object to the object manager
						_cObjMgr.AddObject(cTmpTrooper);

						// add to the battle reference data
						_cBattleData.naArmyRef.Add(cTmpTrooper);
					}

				// make a block of opponents
				for(int i = 0; i < _iOX; ++i)
					for(int j = 0; j < _iOY; ++j) { 
						// set the opponents to the acitve list 
						_cActiveList.Add(cTmpTrooper = new Trooper(ObjectManager.cInstance.CreateTemplate(cRight)));
						cTmpTrooper.tPos = new Vector2(950, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);
						cTmpTrooper.iArmyIndex = 1; 
						cTmpTrooper.iOpponentIndex = 0;

						// add to the object manger
						_cObjMgr.AddObject(cTmpTrooper);

						_cBattleData.naOpponentsRef.Add(cTmpTrooper);
					}

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(_cCursor);

				//// ddhj: load in some debug data
				_cFont = cContent.Load<SpriteFont>(@"Shared\DebugFont");
				DataStore.cInstance.cBorder = _cBorder = new Texture2D(cGraphics, 1, 1);
				_cBorder.SetData<Color>(new[] { Color.White });

				//// ddhj: configrable items for the battle draw
				_cToggleLifeBar = new System.Windows.Forms.CheckBox();
				_cToggleLifeBar.Location = new System.Drawing.Point(0, 30);
				_cToggleLifeBar.Text = _cToggleLifeBar.Name = "Toggle Life Bar";
				_cToggleLifeBar.AutoSize = true;
				_cToggleLifeBar.CheckedChanged += new EventHandler(ToggleLifeBarChange);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleLifeBar);

				_cToggleDamageNumbers = new System.Windows.Forms.CheckBox();
				_cToggleDamageNumbers.Location = new System.Drawing.Point(0, 60);
				_cToggleDamageNumbers.Text = _cToggleLifeBar.Name = "Toggle Damage Numbers";
				_cToggleDamageNumbers.AutoSize = true;
				_cToggleDamageNumbers.CheckedChanged += new EventHandler(ToggleDamageNumbersChange);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleDamageNumbers);

				_cToggleSlowMo = new System.Windows.Forms.CheckBox();
				_cToggleSlowMo.Location = new System.Drawing.Point(0, 80);
				_cToggleSlowMo.Text = _cToggleLifeBar.Name = "Toggle Slow Mo";
				_cToggleSlowMo.AutoSize = true;
				_cToggleSlowMo.CheckedChanged += new EventHandler(ToggleSlowMoChange);
				System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleSlowMo);

				// set the start of battle
				SetBattleStart();
			} catch(Exception xEx) { 
				System.Windows.Forms.MessageBox.Show(xEx.ToString());
			}

			return true;
		}

		public void ToggleLifeBarChange(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bLifeBars = _cToggleLifeBar.Checked;
		}

		public void ToggleDamageNumbersChange(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bDamageNumbers = _cToggleDamageNumbers.Checked;
		}

		public void ToggleSlowMoChange(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bSlowMo = _cToggleSlowMo.Checked;
		}

		void BackToMenu()
		{
			_eState = ESceneStates.Inactive;
			DataStore.cInstance.cSceneMgr.RemoveScene(this);
		}

		void PartialClean()
		{
			_cSpriteBatch.Dispose();
			_cActiveList.Clear();
			_cTrooperRef.Clear();
			_cBattleData.Clear();
			_cObjMgr.Clear();
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cToggleLifeBar);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cToggleDamageNumbers);
			System.Windows.Forms.Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cToggleSlowMo);
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
			_cActiveList.Clear();
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
