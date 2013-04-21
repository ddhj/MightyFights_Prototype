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
		Texture2D		_cTrooperTex;
		AnimationData	_cTroopers;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		BasicSprite		_cPlayer1, 
						_cPlayer2,
						_cCursor,
						_cP1Card, 
						_cP2Card,
						_cToBattle;
		
		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			MouseState	cState = Mouse.GetState();
			Point		tPoint = new Point(cState.X, cState.Y);

			// set the position of the cursor
			_cCursor.tPos = new Vector2(cState.X, cState.Y);

			if(cState.LeftButton == ButtonState.Pressed) { 
				if(((IClickable)_cToBattle).ContainsPoint(tPoint)) { 
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

			_cBatch.Begin(); { 
				// draw the cards and the to battle items
				_cBatch.Draw(_cP1Card.cTexRef, _cP1Card.tPos, _cP1Card.cFrame.tRect, Color.White);
				_cBatch.Draw(_cP2Card.cTexRef, _cP2Card.tPos, _cP2Card.cFrame.tRect, Color.White);
				_cBatch.Draw(_cToBattle.cTexRef, _cToBattle.tPos, _cToBattle.cFrame.tRect, Color.White);

				// the troopers and their colors and all that will have to pay attention to the rotation and all of that 
				// troopers and or anything off a spritesheet might need their own draw method
				_cBatch.Draw(_cTrooperTex, _cPlayer1.tPos, _cPlayer1.cFrame.tRect, Color.White, 
					_cPlayer1.cFrame.bRot ? -(float)Math.PI/2 : 0, _cPlayer1.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
				_cBatch.Draw(_cTrooperTex, _cPlayer2.tPos, _cPlayer2.cFrame.tRect, Color.White, 
					_cPlayer2.cFrame.bRot ? -(float)Math.PI/2 : 0, _cPlayer2.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
			
				// draw cursor
				_cBatch.Draw(_cCursor.cTexRef, _cCursor.tPos, _cCursor.cFrame.tRect, Color.White);
			} _cBatch.End();
		}

		public bool Init()
		{
			try { 
				ContentManager cContent = DataStore.cInstance.cContent;
				_cGraphics = DataStore.cInstance.cGraphics;

				// since the placement of the card and the battle are relative to eachother and the veiwport 
				// just grab the texture first
				Texture2D	cCard1 = cContent.Load<Texture2D>(@"Out Game\Main Menu\green_card"), 
							cCard2 = cContent.Load<Texture2D>(@"Out Game\Main Menu\red_card"), 
							cBattle = cContent.Load<Texture2D>(@"Out Game\Main Menu\to_battle!-1");

				_cCursor = new BasicSprite();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cP1Card = new BasicSprite();
				_cP1Card.cTexRef = cCard1;
				_cP1Card.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				// 152 is the width of the to battle button
				_cP1Card.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - cBattle.Bounds.Width, 400);

				_cP2Card = new BasicSprite();
				_cP2Card.cTexRef = cCard2;
				_cP2Card.cFrame = new Frame(cCard2.Bounds, new Vector2(cCard2.Bounds.Width / 2, cCard2.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard2.Bounds.Width, cCard2.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP2Card.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + cBattle.Bounds.Width - cCard2.Bounds.Width, 400);
	
				_cToBattle = new BasicSprite();
				_cToBattle.cTexRef = cBattle;
				_cToBattle.cFrame = new Frame(cBattle.Bounds, new Vector2(cBattle.Bounds.Width / 2, cBattle.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cBattle.Width, cBattle.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cToBattle.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - (cBattle.Bounds.Width / 2), 
					400 + cCard1.Bounds.Height / 2 - (cBattle.Bounds.Height / 2));

				_cTrooperTex = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\Guys");
				_cTroopers = cContent.Load<AnimationData>(@"Sprite Data\Troopers\Halberd\Front Facing\GuysArray");

				_cPlayer1 = new BasicSprite();
				_cPlayer1.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"]["azure"].iStartIndex];
				_cPlayer1.cTexRef = _cTrooperTex;
				_cPlayer1.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - _cToBattle.cTexRef.Bounds.Width + 
												_cP1Card.cTexRef.Bounds.Width / 2 - _cPlayer1.cFrame.tRect.Width / 2, 400 + _cP1Card.cTexRef.Bounds.Width / 2 - 
												_cPlayer1.cFrame.tRect.Height / 2);

				_cPlayer2 = new BasicSprite();
				_cPlayer2.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"]["blood zombie"].iStartIndex];
				_cPlayer2.cTexRef = _cTrooperTex;
				_cPlayer2.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + _cToBattle.cTexRef.Bounds.Width - _cP2Card.cTexRef.Bounds.Width + 
												_cPlayer2.cFrame.tRect.Width / 2, 400 + _cP2Card.cTexRef.Bounds.Width / 2 - 
												_cPlayer2.cFrame.tRect.Height / 2);
				

				_cBatch = new SpriteBatch(DataStore.cInstance.cGraphics);

				return true;
			} catch(Exception xEx) { 
				return false;
			}
		}

		public void Unload()
		{
			_cP1Card.Dispose();
			_cP2Card.Dispose();
			_cToBattle.Dispose();
			_cTrooperTex.Dispose();
			_cTroopers.Dispose();
		}

		#endregion
	}
}
