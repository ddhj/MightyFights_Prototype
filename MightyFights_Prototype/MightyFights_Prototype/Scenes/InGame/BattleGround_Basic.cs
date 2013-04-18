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
		TimeSpan			_tVictoryElapsed = TimeSpan.Zero;

		Cursor			_cCursor;

		// fps battle ground debug
		TimeSpan		_cTime = TimeSpan.Zero;
		int				_iFrameRate = 0,
						_iFrameCtr = 0,
						_iX, 
						_iY;
		SpriteFont		_cFont;

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}
	
		#region IGameScene Members

		void ProcessBattle(GameTime cTime)
		{
			List<Trooper>	cRemoveList = new List<Trooper>();

			// process all the active troopers
			foreach(Trooper cTrooper in _cActiveList)
				if(cTrooper.bActive) { 
					cTrooper.Process(cTime);
				} else cRemoveList.Add(cTrooper);

			// if they are no longer active remove them from all the reference lists
			foreach(Trooper cTrooper in cRemoveList) { 
				// remove from the draw list
				//_cDrawList[cTrooper.sTexName].Remove(cTrooper);

				// remove from the processing list
				_cActiveList.Remove(cTrooper);
			}
		}

		public void Update(GameTime cTime)
		{

			// set the datastore elapsed time for the action and huristic processing 
			DataStore.cInstance.cTime = cTime;

			// process the cursor
			_cCursor.Update(cTime);

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
			ProcessBattle(cTime);

			// this is for the debug 
			_cTime += cTime.ElapsedGameTime;
			if(_cTime > TimeSpan.FromSeconds(1)) { 
				_cTime -= TimeSpan.FromSeconds(1);
				_iFrameRate = _iFrameCtr;
				_iFrameCtr = 0;
			}
		}

		public void Draw(GameTime cTime)
		{
			++_iFrameCtr;

			_cSpriteBatch.Begin(SpriteSortMode.BackToFront, null); { 
				_cSpriteBatch.Draw(_cBackground, new Vector2(112, 84), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, 1); 
			
				// draw all the troopers based on their texture 
				foreach(KeyValuePair<string, List<IDrawable>> tTrooperList in _cDrawList)
					foreach(IDrawable nSprite in tTrooperList.Value)
						nSprite.Draw(_cSpriteBatch);

				// the frame rate debug statement
				_cSpriteBatch.DrawString(_cFont, string.Format("fps:{0} : LeftArmy:{1} : RightArmy:{2}", _iFrameRate, _cBattleData.naArmyRef.Count, _cBattleData.naOpponentsRef.Count), 
					new Vector2(10, 10), Color.White);				

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

			for(int i = 0; i < _iX; ++i)
				for(int j = 0; j < _iY; ++j) { 
					iIndex = i*_iY+j;
					cTrooper = (Trooper)naTmpList[iIndex];

					// set an initial script for the trooper
					cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
					cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(200 - i * 35 + 25 + 112, j * 25 + 84), null));

					// set the trooper for battle
					cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				}

			iCount = 0;
			naTmpList = _cBattleData.naOpponentsRef;
			for(int i = 0; i < _iX; ++i)
				for(int j = 0; j < _iY; ++j) { 
					cTrooper = (Trooper)naTmpList[i*_iY+j];

					// set an initial script for the trooper
					cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
					cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(400 + i * 35 + 25 + 112, j * 25 + 84), null));

					// set the trooper for battle
					cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				}

			//// this is the old setup for the opponent choosing on start 
			// this is a temp block setup for the inital opponent 
			//ICombatant			nTmpTrooper,
			//						nTmpOpponent;
			//naTmpList = new List<ICombatant>();
			//foreach(ICombatant nCombatant in _cBattleData.naOpponentsRef)
			//    naTmpOpponents.Add(nCombatant);
			//foreach(ICombatant nCombatant in _cBattleData.naArmy)
			//    naTmpList.Add(nCombatant);

			//// radomly assign opponents
			//for(int iCombatant = 0; iCombatant < _cBattleData.naArmy.Count; ++iCombatant) { 
			//    nTmpOpponent = naTmpOpponents[cRand.Next(naTmpList.Count)];
			//    nTmpTrooper = naTmpList[cRand.Next(naTmpList.Count)]; 
			//    naTmpList.Remove(nTmpTrooper);
			//    naTmpOpponents.Remove(nTmpOpponent);

			//    nTmpOpponent.nOpponent = nTmpTrooper;
			//    nTmpTrooper.nOpponent = nTmpOpponent;
			//}

		}

		public bool Init()
		{
			ContentManager	cContent = DataStore.cInstance.cContent;
			GraphicsDevice	cGraphics = DataStore.cInstance.cGraphics;
			List<IDrawable>	naDrawList;
			Trooper			cP1;
			TemplateConfig	cTemplate;
			string			sTexPath = @"Sprite Data\Troopers\Halberd\Textures\",
							sTex1, 
							sTex2;

			string[] saTextures = new string[] { "fazure", "fbrown", "fcrimson", "fgrey", "fgules", 
				"fmidnight", "fpurple", "frust", "fsable", "fsteel", "fstorm", "ftenne", "fvert", "fvert2", 
				"fzombie" };

			// this is for quick action
			Random cRand = new Random();
			_iX = cRand.Next(8) + 1;
			_iY = cRand.Next(20) + 1;

			try { 
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass_large");
				
				//// ddhj pre-template texture assignments
				sTex1 = sTexPath + saTextures[cRand.Next(saTextures.Length)];
				sTex2 = sTexPath + saTextures[cRand.Next(saTextures.Length)];

				// set the battle data to the datastore for reference 
				DataStore.cInstance.cBattleData = _cBattleData;
				
				// make a temp 160 block of troopers
				for(int i = 0; i < _iX; ++i)
					for(int j = 0; j < _iY; ++j) { 
						// create a template config for the sprite
						//// ddhj: this is temp before the template code is written
						cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", sTex1);
						cTemplate.cStats = new Stats();

						// add the newly created trooper to the active list and set some initial battle data
						_cActiveList.Add(cP1 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate)));
						cP1.tPos = new Vector2(25, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);
						cP1.iArmyIndex = 0;
						cP1.iOpponentIndex = 1;

						// set it up in the draw list by texture name so we only draw the linked texture items at a time
						if(!_cDrawList.TryGetValue(cP1.sTexName, out naDrawList))
							_cDrawList.Add(cP1.sTexName, naDrawList = new List<IDrawable>());

						naDrawList.Add(cP1);
						_cBattleData.naArmyRef.Add(cP1);
					}

				// make a temp 160 block of opponents
				for(int i = 0; i < _iX; ++i)
					for(int j = 0; j < _iY; ++j) { 
						// this is the same as above, templates will replace this
						cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", sTex2);
						cTemplate.cStats = new Stats();

						// set the opponents to the acitve list 
						_cActiveList.Add(cP1 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate)));
						cP1.tPos = new Vector2(950, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);
						cP1.iArmyIndex = 1; 
						cP1.iOpponentIndex = 0;

						if(!_cDrawList.TryGetValue(cP1.sTexName, out naDrawList))
							_cDrawList.Add(cP1.sTexName, naDrawList = new List<IDrawable>());

						naDrawList.Add(cP1);
						_cBattleData.naOpponentsRef.Add(cP1);
					}

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), null, false, false);

				if(!_cDrawList.TryGetValue(_cCursor.sTexName, out naDrawList))
					_cDrawList.Add(_cCursor.sTexName, naDrawList = new List<IDrawable>());

				naDrawList.Add(_cCursor);

				_cFont = cContent.Load<SpriteFont>(@"Shared\DebugFont");

				// set the start of battle
				SetBattleStart();
			} catch(Exception xEx) { 
				return false;
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
			_cDrawList.Clear();
			_cActiveList.Clear();
			_cTrooperRef.Clear();
			_cBattleData.Clear();
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
		}

		public void Unload()
		{
			CleanData();
		}

		#endregion
	}
}
