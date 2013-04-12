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

		Cursor			_cCursor;

		// fps battle ground debug
		TimeSpan		_cTime = TimeSpan.Zero;
		int				_iFrameRate = 0,
						_iFrameCtr = 0;
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

			// if they 
			foreach(Trooper cTrooper in cRemoveList) { 
				// remove from the draw list
				_cDrawList[cTrooper.cTexRef.Name].Remove(cTrooper);

				// remove from the processing list
				_cActiveList.Remove(cTrooper);

				// remove the trooper from either the opponents or the army 
				_cTrooperRef[cTrooper.cTexRef.Name].Remove(cTrooper);
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
					if(_cBattleData.naOpponents.Count == 0 || _cBattleData.naArmy.Count == 0)
						_cBattleData.eState = EBattlegroundState.Victory;
				break;

				case EBattlegroundState.Victory: 
					// do the victory thing (probably a button for yay done?
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

			_cSpriteBatch.Begin(); { 
				_cSpriteBatch.Draw(_cBackground, _cBackground.Bounds, Color.White);

				// draw all the troopers based on their texture 
				foreach(KeyValuePair<string, List<IDrawable>> tTrooperList in _cDrawList)
					foreach(IDrawable nSprite in tTrooperList.Value)
						nSprite.Draw(_cSpriteBatch);

				// the frame rate debug statement
				_cSpriteBatch.DrawString(_cFont, string.Format("fps:{0}", _iFrameRate), 
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
			List<ICombatant>	naTmpList = _cBattleData.naArmy;
			int		iCount = 0;
			Trooper cTrooper = (Trooper)naTmpList[0];

			for(int i = 0; i < 8; ++i)
				for(int j = 0; j < 20; ++j) { 
					cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
					cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(200 - i * 35 + 25, j * 25), null));

					// init the initial opponent
					cTrooper.nOpponent = _cBattleData.naOpponents[i*20+j];
					_cBattleData.naOpponents[i*20+j].nOpponent = cTrooper;
				}

			iCount = 0;
			for(int i = 0; i < 8; ++i)
				for(int j = 0; j < 20; ++j) { 
					cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
					cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(400 + i * 35 + 25, j * 25), null));
				}
		}

		public bool Init()
		{
			try { 
				ContentManager	cContent = DataStore.cInstance.cContent;
				GraphicsDevice	cGraphics = DataStore.cInstance.cGraphics;
				List<IDrawable>	naDrawList;
				Trooper			cP1;
				TemplateConfig	cTemplate;

				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass_large");
				_cBattleData.naArmy = new List<ICombatant>();
				_cBattleData.naOpponents = new List<ICombatant>();
				_cTrooperRef.Add(@"Sprite Data\Troopers\Halberd\Textures\fazure", _cBattleData.naArmy);
				_cTrooperRef.Add(@"Sprite Data\Troopers\Halberd\Textures\fmidnight", _cBattleData.naOpponents);
				DataStore.cInstance.cBattleData = _cBattleData;
				
				// make a temp 160 block of troopers
				for(int i = 0; i < 8; ++i)
					for(int j = 0; j < 20; ++j) { 
						cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fazure");
						_cActiveList.Add(cP1 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate)));
						cP1.tPos = new Vector2(-105, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);

						if(!_cDrawList.TryGetValue(cP1.cTexRef.Name, out naDrawList))
							_cDrawList.Add(cP1.cTexRef.Name, naDrawList = new List<IDrawable>());

						naDrawList.Add(cP1);
						_cBattleData.naArmy.Add(cP1);
					}

				// make a temp 160 block of opponents
				for(int i = 0; i < 8; ++i)
					for(int j = 0; j < 20; ++j) { 
						cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fmidnight");
						_cActiveList.Add(cP1 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate)));
						cP1.tPos = new Vector2(805, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);

						if(!_cDrawList.TryGetValue(cP1.cTexRef.Name, out naDrawList))
							_cDrawList.Add(cP1.cTexRef.Name, naDrawList = new List<IDrawable>());

						naDrawList.Add(cP1);
						_cBattleData.naOpponents.Add(cP1);
					}

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), null, false, false);

				if(!_cDrawList.TryGetValue(_cCursor.cTexRef.Name, out naDrawList))
					_cDrawList.Add(_cCursor.cTexRef.Name, naDrawList = new List<IDrawable>());

				naDrawList.Add(_cCursor);

				_cFont = cContent.Load<SpriteFont>(@"Shared\DebugFont");

				// set the start of battle
				SetBattleStart();
			} catch(Exception xEx) { 
				return false;
			}

			return true;
		}

		public void Unload()
		{
			_cBackground.Dispose();
		}

		#endregion
	}
}
