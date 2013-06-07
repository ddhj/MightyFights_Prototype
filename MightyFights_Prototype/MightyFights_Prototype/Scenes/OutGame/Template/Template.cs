using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Windows.Forms;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class Template : IGameScene
	{
		ESceneStates	_eState;
		Cursor			_cCursor;
		Slider			_cTopSlider,
						_cBottomSlider;
		ClickableSprite		_cDone,
						_cLargeLeftArrow,
						_cLargeRightArrow,
						_cSmallLeftArrow,
						_cSmallRightArrow;
		StatDisplay		_cAtkPower, 
						_cAtkSpeed,
						_cHitPoints,
						_cMoveSpeed;

		FrontGuys		_cFrontGuys;

		Texture2D		_cFrame;

		GraphicsDevice	_cGraphics;
		SpriteBatch		_cBatch;
		ComboBox		_cTopLevels,
						_cBottomLevels;

		TemplateConfig	_cConfig;
		Dictionary<string, bool>	_cTakenColors;

		bool			_bProcessPress= true;

		// these are % increase to the base value in the animation data file 
		static int[]	_iaAtkSpeedStats = new int[] { 0, 4, 10, 14, 18, 19, 20, 21, 22, 25, 30, 40, 50, 70 };
		// these are % increases to the base movement speed
		static int[]	_iaMoveSpeedStats = new int[] { 0, 4, 10, 14, 18, 19, 20, 21, 22, 25, 30, 40, 50, 70 };

		// these are just raw stats
		static int[]	_iaHitPointStats = new int[] { 100, 110, 120, 150, 200, 400, 600, 800, 1200, 1600, 2000, 2200, 2400, 3000 };
		static int[]	_iaAtkPowerStats = new int[] { 5, 10, 15, 20, 30, 35, 40, 50, 70, 90, 200, 300, 500, 800 };

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public Template(TemplateConfig cConfig, Dictionary<string, bool> cTakenColors)
		{
			_cConfig = cConfig;
			_cTakenColors = cTakenColors;
		}

		void PopulateConfig()
		{
			_cConfig.sColor = _cFrontGuys.sCurColor;
			_cConfig.cStats = new Stats();
			_cConfig.cStats.iAtkSpeed = _iaAtkSpeedStats[_cAtkSpeed.iCurFrame];
			_cConfig.cStats.fHp = _iaHitPointStats[_cHitPoints.iCurFrame];
			_cConfig.cStats.iPower = _iaAtkPowerStats[_cAtkPower.iCurFrame];
			_cConfig.cStats.iMovement = _iaMoveSpeedStats[_cMoveSpeed.iCurFrame];
			_cConfig.iBottomLevel = _cBottomLevels.SelectedIndex;
			_cConfig.iTopLevel = _cTopLevels.SelectedIndex;
		}

		public void Update(GameTime cTime)
		{
			MouseState	cState = Mouse.GetState();
			Point		tPoint = new Point(cState.X, cState.Y);
			_cCursor.Update(cTime);
			if(cState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { 
				if(_bProcessPress) { 
					if(_cTopSlider.ContainsPoint(tPoint)) { 
						_cTopSlider.bSelected = true;
					} else if(_cBottomSlider.ContainsPoint(tPoint)) { 
						_cBottomSlider.bSelected = true;
					} else if(((IClickable)_cDone).ContainsPoint(_cCursor.tPos)) { 
						PopulateConfig();
						_eState = ESceneStates.Inactive;
						DataStore.cInstance.cSceneMgr.RemoveScene(this);
					} else if(_cLargeLeftArrow.ContainsPoint(tPoint)) { 
						_cFrontGuys.DecrementColor();
						if(_cFrontGuys.iFrameIdx > 12)
							_cFrontGuys.ToString();
					} else if(_cLargeRightArrow.ContainsPoint(tPoint)) { 
						_cFrontGuys.IncrementColor();
						if(_cFrontGuys.iFrameIdx > 12)
							_cFrontGuys.ToString();
					}
				}

				_bProcessPress = false;
			} else { 
				_cTopSlider.bSelected = _cBottomSlider.bSelected = false;
				_bProcessPress = true;
			}

			_cTopSlider.Update(cTime);
			_cBottomSlider.Update(cTime);

			_cAtkPower.SetDisplay(_cTopSlider.iLeftPos);
			_cAtkSpeed.SetDisplay(_cTopSlider.iRightPos);
			_cHitPoints.SetDisplay(_cBottomSlider.iLeftPos);
			_cMoveSpeed.SetDisplay(_cBottomSlider.iRightPos);
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Color.Black);
			_cBatch.Begin(SpriteSortMode.Immediate, null); { 
				_cBatch.Draw(_cFrame, new Vector2(_cGraphics.Viewport.Width/2 - _cFrame.Bounds.Width/2, _cGraphics.Viewport.Height/2 - _cFrame.Bounds.Height/2), _cFrame.Bounds, Color.White);
				_cDone.Draw(_cBatch);
				_cAtkPower.Draw(_cBatch);
				_cAtkSpeed.Draw(_cBatch);
				_cMoveSpeed.Draw(_cBatch);
				_cHitPoints.Draw(_cBatch);
				_cTopSlider.Draw(_cBatch);
				_cBottomSlider.Draw(_cBatch);
				_cFrontGuys.Draw(_cBatch);
				_cLargeLeftArrow.Draw(_cBatch);
				_cLargeRightArrow.Draw(_cBatch);
				_cCursor.Draw(_cBatch);
			} _cBatch.End();	
		}

		public bool Init()
		{
			try { 
				ContentManager cContent = DataStore.cInstance.cContent;
				_cGraphics = DataStore.cInstance.cGraphics;
				_cBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
					
				_cFrame = cContent.Load<Texture2D>(@"Out Game\Template\frame");

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cDone = new ClickableSprite();
				_cDone.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\done");
				_cDone.sTexName = @"Out Game\Template\done";
				_cDone.cFrame = new Frame(_cDone.cTexRef.Bounds, new Vector2(_cDone.cTexRef.Bounds.Width / 2, _cDone.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cDone.cTexRef.Bounds.Width, _cDone.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cDone.tPos = new Vector2(_cGraphics.Viewport.Width/2 - _cDone.cTexRef.Bounds.Width / 2, _cGraphics.Viewport.Height / 2 - _cDone.cTexRef.Bounds.Height / 2  + 140);

				_cAtkPower = new StatDisplay(cContent.Load<AnimationData>(@"Out Game\Template\AtkPowerArray"), "blue");
				_cAtkPower.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\AtkPower");
				_cAtkPower.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - _cAtkPower.cFrame.tRect.Width - 5, _cGraphics.Viewport.Height / 2 - 62);
				_cAtkPower.bLeft = true;

				_cAtkSpeed = new StatDisplay(cContent.Load<AnimationData>(@"Out Game\Template\AtkSpeedArray"), "orange");
				_cAtkSpeed.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\AtkSpeed");
				_cAtkSpeed.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 2, _cGraphics.Viewport.Height / 2 - 62);

				_cHitPoints = new StatDisplay(cContent.Load<AnimationData>(@"Out Game\Template\HitPointsArray"), "pink");
				_cHitPoints.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\HitPoints");
				_cHitPoints.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - _cHitPoints.cFrame.tRect.Width - 5, _cGraphics.Viewport.Height / 2 + 21);
				_cHitPoints.bLeft = true;

				_cMoveSpeed = new StatDisplay(cContent.Load<AnimationData>(@"Out Game\Template\MoveSpeedArray"), "green");
				_cMoveSpeed.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\MoveSpeed");
				_cMoveSpeed.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 2, _cGraphics.Viewport.Height / 2 + 21);

				_cTopSlider = new Slider(cContent.Load<AnimationData>(@"Out Game\Template\TopSlidersArray"), "top");
				_cTopSlider.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\TopSliders");
				_cTopSlider.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - _cTopSlider.cFrame.tRect.Width / 2 - 2, _cGraphics.Viewport.Height / 2 - _cFrame.Bounds.Height / 2 + 18);
				_cTopSlider.iCenter = _cGraphics.Viewport.Width / 2 - 2;
				_cTopSlider.iStartY = (int)_cTopSlider.tPos.Y;
				_cTopSlider.iMaxLeft = (int)_cAtkPower.tPos.X + 10;
				_cTopSlider.iMaxRight = (int)_cAtkSpeed.tPos.X + _cAtkSpeed.cFrame.tRect.Width - 10;
				_cTopSlider.SetLevel(0);

				_cBottomSlider = new Slider(cContent.Load<AnimationData>(@"Out Game\Template\BottomSlidersArray"), "bottom");
				_cBottomSlider.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\BottomSliders");
				_cBottomSlider.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - _cTopSlider.cFrame.tRect.Width / 2 - 2, _cGraphics.Viewport.Height / 2 + _cFrame.Bounds.Height / 2 - 37);
				_cBottomSlider.iCenter = _cGraphics.Viewport.Width / 2 - 2;
				_cBottomSlider.iStartY = (int)_cBottomSlider.tPos.Y;
				_cBottomSlider.iMaxLeft = (int)_cAtkPower.tPos.X + 10;
				_cBottomSlider.iMaxRight = (int)_cAtkSpeed.tPos.X + _cAtkSpeed.cFrame.tRect.Width - 10;
				_cBottomSlider.SetLevel(0);

				_cTopLevels = new ComboBox();
				_cTopLevels.Location = new System.Drawing.Point(_cGraphics.Viewport.Width / 2 + _cFrame.Bounds.Width / 2 + 20, _cGraphics.Viewport.Height / 2 - _cFrame.Bounds.Height / 2);
				_cTopLevels.Items.AddRange(new object[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", 
					"Level 6", "Level 7", "Level 8", "Level 9", "Level 10", "Level 11", "Level 12", "Level 13", "Level 14" });

				_cTopLevels.SelectedIndexChanged += new System.EventHandler(SelectChange);
				_cTopLevels.SelectedIndex = _cConfig.iTopLevel;
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cTopLevels);

				_cBottomLevels = new ComboBox();
				_cBottomLevels.Location = new System.Drawing.Point(_cGraphics.Viewport.Width / 2 + _cFrame.Bounds.Width / 2 + 20, _cGraphics.Viewport.Height / 2 + _cFrame.Bounds.Height / 2);
				_cBottomLevels.Items.AddRange(new object[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", 
					"Level 6", "Level 7", "Level 8", "Level 9", "Level 10", "Level 11", "Level 12", "Level 13", "Level 14" });

				_cBottomLevels.SelectedIndexChanged += new System.EventHandler(SelectChange);
				_cBottomLevels.SelectedIndex = _cConfig.iBottomLevel;
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cBottomLevels);

				_cLargeLeftArrow = new ClickableSprite();
				_cLargeLeftArrow.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\Left Arrow Large");
				_cLargeLeftArrow.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - 45, _cGraphics.Viewport.Height / 2 + 35);
				_cLargeLeftArrow.cFrame = new Frame(_cLargeLeftArrow.cTexRef.Bounds, new Vector2(_cLargeLeftArrow.cTexRef.Bounds.Width / 2, _cLargeLeftArrow.cTexRef.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cLargeLeftArrow.cTexRef.Bounds.Width, _cLargeLeftArrow.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cLargeRightArrow = new ClickableSprite();
				_cLargeRightArrow.cTexRef = cContent.Load<Texture2D>(@"Out Game\Template\Right Arrow Large");
				_cLargeRightArrow.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + 20, _cGraphics.Viewport.Height / 2 + 35);
				_cLargeRightArrow.cFrame = new Frame(_cLargeRightArrow.cTexRef.Bounds, new Vector2(_cLargeRightArrow.cTexRef.Bounds.Width / 2, _cLargeRightArrow.cTexRef.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cLargeRightArrow.cTexRef.Bounds.Width, _cLargeRightArrow.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cFrontGuys = new FrontGuys(cContent.Load<AnimationData>(@"Sprite Data\Troopers\Halberd\Front Facing\frontarray"), _cConfig.sColor, _cTakenColors);
				_cFrontGuys.cTexRef = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\front");
				_cFrontGuys.tPos = new Vector2(_cLargeLeftArrow.tPos.X - 8, _cGraphics.Viewport.Height / 2 - 5);

				return true;
			} catch { 
				return false;
			}
		}

		public void SelectChange(object oSender, EventArgs eEvtArgs) 
		{
			if(oSender == _cTopLevels)
				_cTopSlider.SetLevel(_cTopLevels.SelectedIndex);
			else _cBottomSlider.SetLevel(_cBottomLevels.SelectedIndex);
		}

		public void Unload()
		{
			Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cBottomLevels);
			Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Remove(_cTopLevels);
		}

		public void ToggleControls()
		{

		}

		#endregion
	}
}
