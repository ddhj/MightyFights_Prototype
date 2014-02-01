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

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class KnightMenu : ClickableSprite, IMenuObj
	{
		TemplateButton		_cActive;

		bool				_bProcessClick;

		public object		oMenuObject	{ get { return this; }}
		public object		oResultData	{ get { return null; }}

		CampMenuManager		_cMgr;

		List<IMouseInteractive>		_naButtons = new List<IMouseInteractive>();

		public KnightMenu(CampMenuManager cMgr) : base()
		{
			_cMgr = cMgr;
		}
		
		public bool InitMenu()
		{
			try { 
				DataStore		cData = DataStore.cInstance;
				ContentManager	cContent = cData.cContent;

				TemplateButton cTmpSpr = new TemplateButton();
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\captain_button");
				cTmpSpr.tPos = new Vector2(92, 223);
				cTmpSpr.sTexName = @"In Game\Camp\KnightMenu\captain_button";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSpr.dlProcessClick = CaptainsClick;
				_naButtons.Add(cTmpSpr);

				cTmpSpr = new TemplateButton();
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\company_button");
				cTmpSpr.tPos = new Vector2(81, 252);
				cTmpSpr.sTexName = @"In Game\Camp\KnightMenu\company_button";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSpr.dlProcessClick = CompanyClick;
				_naButtons.Add(cTmpSpr);

				cTmpSpr = new TemplateButton();
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\dialogue_button");
				cTmpSpr.tPos = new Vector2(143, 241);
				cTmpSpr.sTexName = @"In Game\Camp\KnightMenu\dialogue_button";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSpr.dlProcessClick = CaptainsClick;
				_naButtons.Add(cTmpSpr);

				cTmpSpr = new TemplateButton();
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\template_button");
				cTmpSpr.tPos = new Vector2(166, 259);
				cTmpSpr.sTexName = @"In Game\Camp\KnightMenu\template_button";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSpr.dlProcessClick = CaptainsClick;
				_naButtons.Add(cTmpSpr);
			} catch { 
				return false;
			}
			
			return true;
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			MouseState			cMouseState = Mouse.GetState();
			Point				tPoint = new Point(cMouseState.X, cMouseState.Y);

			// walk the mouse interactive objects
			_cActive = null;
			foreach(IMouseInteractive nObj in _naButtons)
				if(nObj.MouseMove(cMouseState))
					_cActive = (TemplateButton)nObj;

			if(cMouseState.LeftButton == ButtonState.Pressed) { 
				if(this.ContainsPoint(tPoint)) { 
					// test the four buttons for which one is active
					if(_cActive != null)	_cActive.dlProcessClick(this, null);

					if(_bProcessClick == true) { 
						ProcessClick();
									
						_bProcessClick = false;
					} else _bProcessClick = true;
				} else _cMgr.RemoveMenuObject(this);
			} 
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

			// check to see if we have an active button
			if(_cActive != null)	_cActive.Draw(cBatch);
		}

		void ProcessClick()
		{
			// loop through the buttons 

		}

		void TemplateClick(object oSender, object oArgs)
		{

		}

		void CaptainsClick(object oSender, object oArgs)
		{

		}

		void CompanyClick(object oSender, object oArgs) 
		{
			CompanyMenu		cCompany = new CompanyMenu(_cMgr);
			Viewport		cView = DataStore.cInstance.cGraphics.Viewport;

			cCompany.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\company_window");
			cCompany.tPos = new Vector2(cView.Width / 2 - cCompany.cTexRef.Width / 2, cView.Height / 2 - cCompany.cTexRef.Height / 2);
			cCompany.sTexName = @"In Game\Camp\Company Dialog\company_window";
			cCompany.cFrame = new Frame(cCompany.cTexRef.Bounds, 
				new Vector2(cCompany.cTexRef.Bounds.Width / 2, cCompany.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), 
				new Vector2(cCompany.cTexRef.Bounds.Width, cCompany.cTexRef.Bounds.Height), 
				new Vector2(0, 0), new Vector2(0, 0), null, false, false);

			// set the background sprite first because everything in the menu will be based on the position of the top left point
			cCompany.InitMenu();
			_cMgr.AddMenuObject(cCompany);
		}
	}
}