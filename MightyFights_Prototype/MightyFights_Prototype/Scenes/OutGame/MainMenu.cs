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
	public class MainMenu : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cCardP1, 
						_cCardP2,
						_cToBattle,
						_cTrooperTex;
		AnimationData	_cTroopers;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		BasicSprite		_cPlayer1, 
						_cPlayer2,
						_cCursor;
		
		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			MouseState	cState = Mouse.GetState();
			bool		bRes;
			Point		tPoint = new Point(cState.X, cState.Y);
			_cCursor.tPos = new Vector2(cState.X, cState.Y);

			if(cState.LeftButton == ButtonState.Pressed) { 
				// check to see if we collide with our to battle
				Rectangle tToBattle = new Rectangle();
				tToBattle.X = _cGraphics.Viewport.Width / 2 - (_cToBattle.Bounds.Width / 2);
				tToBattle.Y = 400 + _cCardP1.Bounds.Height / 2 - (_cToBattle.Bounds.Height / 2);
				tToBattle.Width = _cToBattle.Bounds.Width;
				tToBattle.Height = _cToBattle.Bounds.Height;

				tToBattle.Contains(ref tPoint, out bRes);
				if(bRes) { 
					IGameScene nBattleGround = new BattleGround_Basic();
					if(nBattleGround.Init()) { 
						DataStore.cInstance.cSceneMgr.AddScene(nBattleGround);
					}
				}
			}
			
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Color.Black);

			_cBatch.Begin();
				_cBatch.Draw(_cCardP1, new Vector2(_cGraphics.Viewport.Width / 2 - _cToBattle.Bounds.Width, 400), _cCardP1.Bounds, Color.White);
				_cBatch.Draw(_cCardP2, new Vector2(_cGraphics.Viewport.Width / 2 + _cToBattle.Bounds.Width - _cCardP2.Width, 400), _cCardP2.Bounds, Color.White);
				_cBatch.Draw(_cToBattle, new Vector2(_cGraphics.Viewport.Width / 2 - (_cToBattle.Bounds.Width / 2), 400 + _cCardP1.Bounds.Height / 2 - (_cToBattle.Bounds.Height / 2)), _cToBattle.Bounds, Color.White);
				//spriteBatch.Draw(_cSpriteSheet, tVector, cFrame.tRect, Color.White, 
				//    cFrame.bRot ? -(float)Math.PI/2 : 0, cFrame.tTopLeft, 1, SpriteEffects.None, 0);

				_cBatch.Draw(_cTrooperTex, _cPlayer1.tPos, _cPlayer1.cFrame.tRect, Color.White, 
					_cPlayer1.cFrame.bRot ? -(float)Math.PI/2 : 0, _cPlayer1.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
				_cBatch.Draw(_cTrooperTex, _cPlayer2.tPos, _cPlayer2.cFrame.tRect, Color.White, 
					_cPlayer2.cFrame.bRot ? -(float)Math.PI/2 : 0, _cPlayer2.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
			
				// draw cursor
				_cBatch.Draw(_cCursor.cTexRef, _cCursor.tPos, _cCursor.cFrame.tRect, Color.White);

			_cBatch.End();
		}

		public bool Init()
		{
			try { 
				ContentManager cContent = DataStore.cInstance.cContent;
				_cGraphics = DataStore.cInstance.cGraphics;

				_cCardP1 = cContent.Load<Texture2D>(@"Out Game\Main Menu\green_card");
				_cCardP2 = cContent.Load<Texture2D>(@"Out Game\Main Menu\red_card");
				_cToBattle = cContent.Load<Texture2D>(@"Out Game\Main Menu\to_battle!-1");
				_cTrooperTex = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\Guys");
			
				_cTroopers = cContent.Load<AnimationData>(@"Sprite Data\Troopers\Halberd\Front Facing\GuysArray");

				_cPlayer1 = new BasicSprite();
				_cPlayer1.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["FrontFacing"]["Colors"]["azure"].iStartIndex];
				_cPlayer1.cTexRef = _cTrooperTex;
				_cPlayer1.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - _cToBattle.Bounds.Width + 
												_cCardP1.Width / 2 - _cPlayer1.cFrame.tRect.Width / 2, 400 + _cCardP1.Bounds.Width / 2 - 
												_cPlayer1.cFrame.tRect.Height / 2);

				_cPlayer2 = new BasicSprite();
				_cPlayer2.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["FrontFacing"]["Colors"]["blood zombie"].iStartIndex];
				_cPlayer2.cTexRef = _cTrooperTex;
				_cPlayer2.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + _cToBattle.Bounds.Width - _cCardP2.Width + 
												_cPlayer2.cFrame.tRect.Width / 2, 400 + _cCardP2.Bounds.Width / 2 - 
												_cPlayer2.cFrame.tRect.Height / 2);
				
				_cCursor = new BasicSprite();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), null, false, false);

				_cBatch = new SpriteBatch(DataStore.cInstance.cGraphics);

				return true;
			} catch(Exception xEx) { 
				return false;
			}
		}

		public void Unload()
		{
			_cCardP1.Dispose();
			_cCardP2.Dispose();
			_cToBattle.Dispose();
			_cTrooperTex.Dispose();
			
			_cTroopers.Dispose();
		}

		#endregion
	}
}
