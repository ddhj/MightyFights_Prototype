using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Windows.Forms;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class Template : ClickableSprite, IMenuObj, IMouseInteractive
	{
		Slider			_cTopSlider,
						_cBottomSlider;
		ClickableSprite	_cDone,
						_cLargeLeftArrow,
						_cLargeRightArrow;

		StatDisplay		_cAtkPower, 
						_cAtkSpeed,
						_cHitPoints,
						_cMoveSpeed;

		BasicSprite		_cFrame,
						_cAtkCombo,
						_cActsOfProwess,
						_cBattleMastery;

		FrontGuys		_cFrontGuys;
		SpriteFont		_cFont;

		GraphicsDevice	_cGraphics;

		CampMenuManager	_cMgr;
		TemplateConfig	_cConfig;

		NameTextBox		_cName;
		bool			_bProcessPress= true;

		List<Ability>			_caAbilities = new List<Ability>();
		List<MenuControlBase>	_caControls = new List<MenuControlBase>();

		// these are % increase to the base value in the animation data file 
		static int[]	_iaAtkSpeedStats = new int[] { 0, 4, 10, 14, 18, 19, 20, 21, 22, 25, 30, 40, 50, 70 };
		// these are % increases to the base movement speed
		static int[]	_iaMoveSpeedStats = new int[] { 0, 4, 10, 14, 18, 19, 20, 21, 22, 25, 30, 40, 50, 70 };

		// these are just raw stats
		static int[]	_iaHitPointStats =	new int[] { 100, 110, 120, 150, 200, 400, 600, 800, 1200, 1600, 2000, 2200, 2400, 3000 };
		static int[]	_iaAtkPowerStats =	new int[] { 5, 10, 15, 20, 30, 35, 40, 50, 70, 90, 200, 300, 500, 800 };
		static int[]	_iaAC = 			new int[] { 3, 4, 5, 10, 15, 20, 25, 40, 45, 50, 100, 150, 200, 400 };
		static float[]	_fCrit = new float[] { .05f, .06f, .07f, .1f, .11f, .12f, .13f, .18f, .2f, .25f, .30f, .50f	};

		public object			oMenuObject	{ get { return this; }}
		public object			oResultData	{ get { return null; }}

		public Template(CampMenuManager cMgr, TemplateConfig cConfig)
		{
			_cConfig = cConfig;
			_cMgr = cMgr;
		}

		void PopulateConfig()
		{
			_cConfig.sColor = _cFrontGuys.sCurColor;
			_cConfig.cStats = new Stats();
			_cConfig.cStats.iAtkSpeed = _iaAtkSpeedStats[_cAtkSpeed.iCurFrame];
			_cConfig.cStats.fCrit = _fCrit[_cAtkSpeed.iCurFrame];
			_cConfig.cStats.fHp = _cConfig.cStats.iMaxHp = _iaHitPointStats[_cHitPoints.iCurFrame];
			_cConfig.cStats.iHealPoint = (int)(.4 * _cConfig.cStats.fHp);
			_cConfig.cStats.iFleePoint = (int)(.05 * _cConfig.cStats.fHp);
			_cConfig.cStats.iPower = _iaAtkPowerStats[_cAtkPower.iCurFrame];
			_cConfig.cStats.iMovement = _iaMoveSpeedStats[_cMoveSpeed.iCurFrame];
			_cConfig.cStats.iArmorClass = _iaAC[_cHitPoints.iCurFrame];
			_cConfig.iBottomLevel = _cBottomSlider.iLevel;
			_cConfig.iTopLevel = _cTopSlider.iLevel;
			_cConfig.sTemplateName = _cName.sName;
		}

		public void Process(GameTime cTime)
		{
			// this is a little wonky because its older code when the template was in the alpha state and I am just straight porting 
			// rather than rewriting it from scratch
			MouseState	cState = Mouse.GetState();
			Point		tPoint = new Point(cState.X, cState.Y);
			if(cState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { 
				if(_bProcessPress) { 
					if(_cTopSlider.ContainsPoint(tPoint)) { 
						_cTopSlider.bSelected = true;
					} else if(_cBottomSlider.ContainsPoint(tPoint)) { 
						_cBottomSlider.bSelected = true;
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

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
	
			_cAtkCombo.Draw(cBatch);
			_cFrame.Draw(cBatch);
			_cAtkPower.Draw(cBatch);
			_cAtkSpeed.Draw(cBatch);
			_cMoveSpeed.Draw(cBatch);
			_cHitPoints.Draw(cBatch);
			_cTopSlider.Draw(cBatch);
			_cBottomSlider.Draw(cBatch);
			_cFrontGuys.Draw(cBatch);
			_cName.Draw(cBatch);
			_cActsOfProwess.Draw(cBatch);
			_cBattleMastery.Draw(cBatch);

			_cLargeLeftArrow.Draw(cBatch);
			_cLargeRightArrow.Draw(cBatch);

			cBatch.DrawString(_cFont, _cName.sName, new Vector2(_cName.tPos.X + 30, _cName.tPos.Y + 10), Color.White);

			foreach(Ability cAbility in _caAbilities)
				cAbility.Draw(cBatch);
		}

		public bool InitMenu()
		{
			try { 
				ContentManager cContent = DataStore.cInstance.cContent;
				NameTextBox cTmpSpr = new NameTextBox(_cMgr);

				_cGraphics = DataStore.cInstance.cGraphics;
				_cFont = cContent.Load<SpriteFont>(@"Shared\TestFon");
		
				_cFrame = new BasicSprite();
				_cFrame.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\frame");
				_cFrame.tPos = new Vector2(tPos.X + 63, tPos.Y + 13);
				_cFrame.sTexName = @"In Game\Camp\Company Dialog\frame";
				_cFrame.fZRange = .5f;
				_cFrame.cFrame = new Frame(_cFrame.cTexRef.Bounds, 
					new Vector2(_cFrame.cTexRef.Bounds.Width / 2, _cFrame.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cFrame.cTexRef.Bounds.Width, _cFrame.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
					
				_cAtkCombo = new BasicSprite();
				_cAtkCombo.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\combinations");
				_cAtkCombo.tPos = new Vector2(tPos.X + 14, tPos.Y + 56);
				_cAtkCombo.sTexName = @"In Game\Camp\Company Dialog\frame";
				_cAtkCombo.fZRange = .51f;
				_cAtkCombo.cFrame = new Frame(_cAtkCombo.cTexRef.Bounds, 
					new Vector2(_cAtkCombo.cTexRef.Bounds.Width / 2, _cAtkCombo.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cAtkCombo.cTexRef.Bounds.Width, _cAtkCombo.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cActsOfProwess = new BasicSprite();
				_cActsOfProwess.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\acts of prowess");
				_cActsOfProwess.tPos = new Vector2(tPos.X + 336, tPos.Y + 40);
				_cActsOfProwess.sTexName = @"In Game\Camp\Company Dialog\acts of prowess";
				_cActsOfProwess.fZRange = .5f;
				_cActsOfProwess.cFrame = new Frame(_cActsOfProwess.cTexRef.Bounds, 
					new Vector2(_cActsOfProwess.cTexRef.Bounds.Width / 2, _cActsOfProwess.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cActsOfProwess.cTexRef.Bounds.Width, _cActsOfProwess.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cBattleMastery = new BasicSprite();
				_cBattleMastery.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\battle mastery");
				_cBattleMastery.tPos = new Vector2(tPos.X + 340, tPos.Y + 160);
				_cBattleMastery.sTexName = @"In Game\Camp\Company Dialog\frame";
				_cBattleMastery.fZRange = .5f;
				_cBattleMastery.cFrame = new Frame(_cBattleMastery.cTexRef.Bounds, 
					new Vector2(_cBattleMastery.cTexRef.Bounds.Width / 2, _cBattleMastery.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cBattleMastery.cTexRef.Bounds.Width, _cBattleMastery.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cName = cTmpSpr;
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\template_name");
				cTmpSpr.tPos = new Vector2(tPos.X + 115, tPos.Y + (cTexRef.Height - cTmpSpr.cTexRef.Height));
				cTmpSpr.sTexName = @"In Game\Camp\Company Dialog\company_name";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caControls.Add(cTmpSpr);

				_cAtkPower = new StatDisplay(cContent.Load<AnimationData>(@"In Game\Camp\Template\AtkPowerArray"), "blue");
				_cAtkPower.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\AtkPower");
				_cAtkPower.fZRange = .5f;
				_cAtkPower.tPos = new Vector2(tPos.X + 108, tPos.Y + 56);
				_cAtkPower.bLeft = true;

				_cAtkSpeed = new StatDisplay(cContent.Load<AnimationData>(@"In Game\Camp\Template\AtkSpeedArray"), "orange");
				_cAtkSpeed.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\AtkSpeed");
				_cAtkSpeed.fZRange = .5f;
				_cAtkSpeed.tPos = new Vector2(tPos.X + 208, tPos.Y + 56);

				_cHitPoints = new StatDisplay(cContent.Load<AnimationData>(@"In Game\Camp\Template\HitPointsArray"), "pink");
				_cHitPoints.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\HitPoints");
				_cHitPoints.fZRange = .5f;
				_cHitPoints.tPos = new Vector2(tPos.X + 108, tPos.Y + 138);
				_cHitPoints.bLeft = true;

				_cMoveSpeed = new StatDisplay(cContent.Load<AnimationData>(@"In Game\Camp\Template\MoveSpeedArray"), "green");
				_cMoveSpeed.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\MoveSpeed");
				_cMoveSpeed.fZRange = .5f;
				_cMoveSpeed.tPos = new Vector2(tPos.X + 208, tPos.Y + 138);

				_cTopSlider = new Slider(cContent.Load<AnimationData>(@"In Game\Camp\Template\TopSlidersArray"), "top");
				_cTopSlider.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\TopSliders");
				_cTopSlider.fZRange = .5f;
				_cTopSlider.tPos = new Vector2(tPos.X + 191, tPos.Y + 30);
				_cTopSlider.iCenter = (int)tPos.X + 204;
				_cTopSlider.iStartY = (int)_cTopSlider.tPos.Y;
				_cTopSlider.iMaxLeft = (int)_cAtkPower.tPos.X + 10;
				_cTopSlider.iMaxRight = (int)_cAtkSpeed.tPos.X + _cAtkSpeed.cFrame.tRect.Width - 10;
				_cTopSlider.SetLevel(0);

				_cBottomSlider = new Slider(cContent.Load<AnimationData>(@"In Game\Camp\Template\BottomSlidersArray"), "bottom");
				_cBottomSlider.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\BottomSliders");
				_cBottomSlider.fZRange = .5f;
				_cBottomSlider.tPos = new Vector2(tPos.X + 191, tPos.Y + 186);
				_cBottomSlider.iCenter = (int)tPos.X + 204;
				_cBottomSlider.iStartY = (int)_cBottomSlider.tPos.Y;
				_cBottomSlider.iMaxLeft = (int)_cAtkPower.tPos.X + 10;
				_cBottomSlider.iMaxRight = (int)_cAtkSpeed.tPos.X + _cAtkSpeed.cFrame.tRect.Width - 10;
				_cBottomSlider.SetLevel(0);

				_cLargeLeftArrow = new ClickableSprite();
				_cLargeLeftArrow.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\Left Arrow Large");
				_cLargeLeftArrow.fZRange = .5f;
				_cLargeLeftArrow.tPos = new Vector2(tPos.X + 161, tPos.Y + 153);
				_cLargeLeftArrow.cFrame = new Frame(_cLargeLeftArrow.cTexRef.Bounds, new Vector2(_cLargeLeftArrow.cTexRef.Bounds.Width / 2, _cLargeLeftArrow.cTexRef.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cLargeLeftArrow.cTexRef.Bounds.Width, _cLargeLeftArrow.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cLargeRightArrow = new ClickableSprite();
				_cLargeRightArrow.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\Right Arrow Large");
				_cLargeRightArrow.tPos = new Vector2(tPos.X + 227, tPos.Y + 153);
				_cLargeRightArrow.fZRange = .5f;
				_cLargeRightArrow.cFrame = new Frame(_cLargeRightArrow.cTexRef.Bounds, new Vector2(_cLargeRightArrow.cTexRef.Bounds.Width / 2, _cLargeRightArrow.cTexRef.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cLargeRightArrow.cTexRef.Bounds.Width, _cLargeRightArrow.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cFrontGuys = new FrontGuys(cContent.Load<AnimationData>(@"Sprite Data\Troopers\Halberd\Front Facing\frontarray"), _cConfig.sColor, new Dictionary<string, bool>());
				_cFrontGuys.cTexRef = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\front");
				_cFrontGuys.fZRange = .5f;
				_cFrontGuys.fScale = 2.0f;
				_cFrontGuys.tPos = new Vector2(tPos.X + 105, tPos.Y + 50);

// the ability icons 
				Ability cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\tremble and fear_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\tremble and fear");
				cTmp.tPos = new Vector2(tPos.X + 320, tPos.Y + 69);
				cTmp.sTexName = @"In Game\Camp\Template\tremble and fear";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\safe guard_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\safe guard");
				cTmp.tPos = new Vector2(tPos.X + 349, tPos.Y + 69);
				cTmp.sTexName = @"In Game\Camp\Template\safe guard";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\extra health_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\extra health");
				cTmp.tPos = new Vector2(tPos.X + 378, tPos.Y + 69);
				cTmp.sTexName = @"In Game\Camp\Template\extra health";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\brave heart_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\brave heart");
				cTmp.tPos = new Vector2(tPos.X + 407, tPos.Y + 69);
				cTmp.sTexName = @"In Game\Camp\Template\brave heart";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\eat my dust_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\eat my dust");
				cTmp.tPos = new Vector2(tPos.X + 436, tPos.Y + 69);
				cTmp.sTexName = @"In Game\Camp\Template\eat my dust";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\absorb damage_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\absorb damage");
				cTmp.tPos = new Vector2(tPos.X + 320, tPos.Y + 110);
				cTmp.sTexName = @"In Game\Camp\Template\absorb damage";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\tough enough_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\tough enough");
				cTmp.tPos = new Vector2(tPos.X + 349, tPos.Y + 110);
				cTmp.sTexName = @"In Game\Camp\Template\tough enough";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\counter attack_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\counter attack");
				cTmp.tPos = new Vector2(tPos.X + 378, tPos.Y + 110);
				cTmp.sTexName = @"In Game\Camp\Template\counter attack";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\chip blade_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\chip blade");
				cTmp.tPos = new Vector2(tPos.X + 407, tPos.Y + 110);
				cTmp.sTexName = @"In Game\Camp\Template\chip blade";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\big time_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\big time");
				cTmp.tPos = new Vector2(tPos.X + 436, tPos.Y + 110);
				cTmp.sTexName = @"In Game\Camp\Template\big time";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);

// battle mastery
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\bleed_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\bleed");
				cTmp.tPos = new Vector2(tPos.X + 360, tPos.Y + 185);
				cTmp.sTexName = @"In Game\Camp\Template\bleed";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\don't bleed_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\don't bleed");
				cTmp.tPos = new Vector2(tPos.X + 381, tPos.Y + 185);
				cTmp.sTexName = @"In Game\Camp\Template\don't bleed";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\flee!_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\flee!");
				cTmp.tPos = new Vector2(tPos.X + 401, tPos.Y + 185);
				cTmp.sTexName = @"In Game\Camp\Template\flee!";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\atkx2_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\atkx2");
				cTmp.tPos = new Vector2(tPos.X + 360, tPos.Y + 216);
				cTmp.sTexName = @"In Game\Camp\Template\atkx2";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\atkx3_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\atkx3");
				cTmp.tPos = new Vector2(tPos.X + 381, tPos.Y + 216);
				cTmp.sTexName = @"In Game\Camp\Template\atkx3";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				
				cTmp = new Ability();
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\crit chance up_gray");
				cTmp.cTexRefAlt = cContent.Load<Texture2D>(@"In Game\Camp\Template\crit chance up");
				cTmp.tPos = new Vector2(tPos.X + 401, tPos.Y + 216);
				cTmp.sTexName = @"In Game\Camp\Template\crit chance up";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caAbilities.Add(cTmp);
				return true;
			} catch { 
				return false;
			}
		}

		public void Unload()
		{
		}

		#region IMenuObj Members

		public void RegeisterEvents()
		{
			// the menu gets a click for close
			InputSystem.MouseUp += new Microsoft.Xna.Framework.Input.MouseEventHandler(MouseUp);
			InputSystem.MouseMove += new Microsoft.Xna.Framework.Input.MouseEventHandler(MouseMove);
			InputSystem.KeyDown += new Microsoft.Xna.Framework.Input.KeyEventHandler(_cName.KeyDown);
			InputSystem.KeyUp += new Microsoft.Xna.Framework.Input.KeyEventHandler(_cName.KeyUp);
		}

		public void UnRegisterEvents()
		{
			// the menu gets a click for close
			InputSystem.MouseUp -= MouseUp;
			InputSystem.MouseMove -= MouseMove;
			InputSystem.KeyDown -= _cName.KeyDown;
			InputSystem.KeyUp -= _cName.KeyUp;

			// save the config for the template
			PopulateConfig();
		}

		#endregion

		#region IMouseInteractive Members

		public void MouseMove(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			// check to see if we are 
		}

		public void MouseDown(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
		}

		public void MouseUp(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			if(!ContainsPoint(eMouseEvt.Location)) { 
				_cMgr.RemoveMenuObject(this);
				return;
			}

			if(eMouseEvt.Button == MouseButton.Right) { 
				if(_cTopSlider.ContainsPoint(eMouseEvt.Location)) _cTopSlider.dlProcessClick(null, null);
				if(_cBottomSlider.ContainsPoint(eMouseEvt.Location)) _cBottomSlider.dlProcessClick(null, null);
			}

			foreach(Ability cAbility in _caAbilities)
				if(cAbility.ContainsPoint(eMouseEvt.Location))
					cAbility.dlProcessClick(null, null);
		}

		public void MouseHover(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseWheel(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
