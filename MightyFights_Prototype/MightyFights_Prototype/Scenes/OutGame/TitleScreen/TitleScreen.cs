using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class TitleScreen : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cScreen;
		GraphicsDevice	_cGraphics;
		SpriteBatch		_cBatch;

		Cursor			_cCursor;
		BasicSprite		_cSword;
		ClickableSprite			_cExit;
		ClickableSprite[]		_cOptions = new ClickableSprite[2],
								_cPlay = new ClickableSprite[2],
								_cView = new ClickableSprite[2];

		Song			_cMusic;
		int				_iPlay,
						_iOptions,
						_iView;
		bool			_bProcessState;

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			MouseState	cState = Mouse.GetState();
			Microsoft.Xna.Framework.Point		tPoint = new Microsoft.Xna.Framework.Point(cState.X, cState.Y);

			// set the position of the cursor
			_cCursor.Update(cTime);	

			// check the position of the mouse over each of the items so see which one is selected 
			if(_cPlay[0].ContainsPoint(tPoint)) { 
				_iOptions = _iView = 0;
				_iPlay = 1;
			} else if(_cOptions[0].ContainsPoint(tPoint)) { 
				_iPlay = _iView = 0;
				_iOptions = 1;
			} else if(_cView[0].ContainsPoint(tPoint)) { 
				_iOptions = _iPlay = 0;
				_iView = 1;
			}

			// check if they are clicking 
			if(cState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { 
				if(_bProcessState) { 
					if(_cPlay[0].ContainsPoint(tPoint)) { 
						IGameScene nCamp = new Camp();
						if(nCamp.Init()) 
							DataStore.cInstance.cSceneMgr.AddScene(nCamp);
					} else if(_cExit.ContainsPoint(tPoint)) { 
						DataStore.cInstance.cGame.Exit();
					}

					_bProcessState = false;
				}
			} else _bProcessState = true;			
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Microsoft.Xna.Framework.Color.Black);

			_cBatch.Begin(); { 
				_cBatch.Draw(_cScreen, new Vector2(0, 0), null, Microsoft.Xna.Framework.Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, .99f);

				// check the state of play 
				if(_iPlay == 1) { 
					_cPlay[1].Draw(_cBatch);
					_cSword.tPos = new Vector2(_cPlay[1].tPos.X - 100, _cPlay[1].tPos.Y);
				} else _cPlay[0].Draw(_cBatch);

				// check the state of options
				if(_iOptions == 1) { 
					_cOptions[1].Draw(_cBatch);
					_cSword.tPos = new Vector2(_cOptions[1].tPos.X - 100, _cOptions[1].tPos.Y);
				} else _cOptions[0].Draw(_cBatch);

				// check the state of View
				if(_iView == 1) { 
					_cView[1].Draw(_cBatch);
					_cSword.tPos = new Vector2(_cView[1].tPos.X - 100, _cView[1].tPos.Y);
				} else _cView[0].Draw(_cBatch);

				_cSword.Draw(_cBatch);
				_cExit.Draw(_cBatch);
				_cCursor.Draw(_cBatch);
			} _cBatch.End();
			
		}

		public bool Init()
		{
			try { 
				DataStore		cData = DataStore.cInstance;
				ContentManager	cContent = cData.cContent;
				ClickableSprite	cTmpSprite;

				_cGraphics = DataStore.cInstance.cGraphics;
				_cMusic = DataManager.cInstance.CreateMusic( "10 WV" );
				_cBatch = new SpriteBatch(_cGraphics);

				_cScreen = cContent.Load<Texture2D>(@"Out Game\TitleScreen\wodyn_title");

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				// play button
				_iPlay = 1;
				cTmpSprite = new ClickableSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\play_game00");
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSprite.tPos = new Vector2(_cGraphics.Viewport.Width / 2, 350);
				_cPlay[0] = cTmpSprite;
				cTmpSprite = new ClickableSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\play_game01");
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSprite.tPos = new Vector2(_cGraphics.Viewport.Width / 2, 350);
				_cPlay[1] = cTmpSprite;

				// options
				cTmpSprite = new ClickableSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\options00");
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSprite.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 50, 400);
				_cOptions[0] = cTmpSprite;
				cTmpSprite = new ClickableSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\options01");
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSprite.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 50, 400);
				_cOptions[1] = cTmpSprite;

				// view 
				cTmpSprite = new ClickableSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\view_legends00");
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSprite.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 100, 450);
				_cView[0] = cTmpSprite;
				cTmpSprite = new ClickableSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\view_legends01");
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSprite.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 100, 450);
				_cView[1] = cTmpSprite;

				// exit 
				_cExit = new ClickableSprite();
				_cExit.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\quit_exit");
				_cExit.cFrame = new Frame(_cExit.cTexRef.Bounds, new Vector2(_cExit.cTexRef.Bounds.Width / 2, _cExit.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cExit.cTexRef.Bounds.Width, _cExit.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cExit.tPos = new Vector2(10, 500);
				
				// sword
				_cSword = new BasicSprite();
				_cSword.cTexRef = cContent.Load<Texture2D>(@"Out Game\TitleScreen\sword");
				_cSword.cFrame = new Frame(_cSword.cTexRef.Bounds, new Vector2(_cSword.cTexRef.Bounds.Width / 2, _cSword.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cSword.cTexRef.Bounds.Width, _cSword.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cSword.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - 100, 350);


				if(DataStore.cInstance.bPlayMusic)	MediaPlayer.Play(_cMusic);
			} catch(Exception xEx) { 
				MessageBox.Show(xEx.ToString());
				return false;
			}
			return true;
		}

		public void Unload()	
		{
			
		}

		public void ToggleControls()
		{
			
		}

		#endregion
	}
}