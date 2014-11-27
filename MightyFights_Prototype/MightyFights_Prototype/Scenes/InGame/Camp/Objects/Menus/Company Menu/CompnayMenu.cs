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
	public class CompanyMenu : ClickableSprite, IMenuObj, IMouseInteractive
	{
		public object			oMenuObject	{ get { return this; }}
		public object			oResultData	{ get { return null; }}

		CampMenuManager			_cMgr;

		List<MenuControlBase>	_caControls = new List<MenuControlBase>();
		List<MenuButton>		_caButtons = new List<MenuButton>();
		List<BasicSprite>		_caBasicSprites = new List<BasicSprite>();
		SpriteFont				_cFont;
		NameTextBox				_cName;

		bool					_bNew;

		public CompanyMenu(CampMenuManager cMgr) : base()
		{
			_cMgr = cMgr;
		}
		
		public bool InitMenu()
		{
			try { 
				DataStore		cData = DataStore.cInstance;
				ContentManager	cContent = cData.cContent;
				int				iCompanyPos = (int)tPos.Y + 40;
				
				NameTextBox cTmpSpr = new NameTextBox(_cMgr);
				_cName = cTmpSpr;
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\company_name");
				cTmpSpr.tPos = new Vector2(tPos.X + 66, tPos.Y + 7);
				cTmpSpr.sTexName = @"In Game\Camp\Company Dialog\company_name";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caControls.Add(cTmpSpr);

				BasicSprite cTmpSprite = new BasicSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\size_box");
				cTmpSprite.tPos = new Vector2(tPos.X + 234, tPos.Y + 17);
				cTmpSprite.sTexName = @"In Game\Camp\Company Dialog\size_box";
				cTmpSprite.fZRange = .5f;
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caBasicSprites.Add(cTmpSprite);

				cTmpSprite = new BasicSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\click_to_drag");
				cTmpSprite.tPos = new Vector2(tPos.X + 113, tPos.Y + 131);
				cTmpSprite.sTexName = @"In Game\Camp\Company Dialog\click_to_drag";
				cTmpSprite.fZRange = .5f;
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caBasicSprites.Add(cTmpSprite);

				MenuButton	cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"Shared\company_arrowupx");
				cTmpBtn.tPos = new Vector2(tPos.X + 28, tPos.Y + 10);
				cTmpBtn.sTexName = @"Shared\company_arrowupx";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\reset_gray");
				cTmpBtn.tPos = new Vector2(tPos.X + 278, tPos.Y + 21);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\reset_gray";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\template_arrow_left");
				cTmpBtn.tPos = new Vector2(tPos.X + 76, tPos.Y + 77);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\template_arrow_left";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\template_arrow_right");
				cTmpBtn.tPos = new Vector2(tPos.X + 316, tPos.Y + 77);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\template_arrow_right";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\grid_arrow_left");
				cTmpBtn.tPos = new Vector2(tPos.X + 76, tPos.Y + 205);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\grid_arrow_left";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\grid_arrow_right");
				cTmpBtn.tPos = new Vector2(tPos.X + 316, tPos.Y + 205);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\grid_arrow_right";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

// company boxes along the left side				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanySelect");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanyBox");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos += 30);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanyBox");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos += 30);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanyBox");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos += 30);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanyBox");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos += 30);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanyBox");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos += 30);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanyBox");
				cTmpBtn.tPos = new Vector2(tPos.X + 10, iCompanyPos += 30);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\CompanySelect";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);
				
// bottom arrow for the company select
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"Shared\company_arrowdownx");
				cTmpBtn.tPos = new Vector2(tPos.X + 28, tPos.Y + 249);
				cTmpBtn.sTexName = @"Shared\company_arrowdownx";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);


				_cFont = cContent.Load<SpriteFont>(@"Shared\TestFon");
			} catch { 
				return false;
			}
			
			return true;
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{	
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

			// draw the managable controls (i think its basically just the name dialog)
			foreach(ClickableSprite cButton in _caControls) cButton.Draw(cBatch);
			
			// draw all the other buttons
			foreach(ClickableSprite cButton in _caButtons) cButton.Draw(cBatch);

			// draw all the non interactive sprites
			foreach(BasicSprite cSprite in _caBasicSprites) cSprite.Draw(cBatch);

			// draw the string in the name dialog
			cBatch.DrawString(_cFont, _cName.sName, new Vector2(tPos.X + 110, tPos.Y + 18), Color.White);
		}

		void ProcessClick()
		{
			// check to see if we are off the menu

			// loop through the buttons 
		}

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			// check to see if we are 
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			if(!ContainsPoint(eMouseEvt.Location)) { 
				_cMgr.RemoveMenuObject(this);
				return;
			}

			// check to see if we have clicked on the name and we are in add mode
			if(_bNew) { 
			}
		}

		public void MouseHover(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseWheel(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IMenuObj Members


		public void RegeisterEvents()
		{
			/* register all the buttons in the company menu 
			foreach(IMouseInteractive nButton in _caControls) { 
				InputSystem.MouseUp += new MouseEventHandler(nButton.MouseUp);
				InputSystem.MouseMove += new MouseEventHandler(nButton.MouseMove);
			}*/

			// the menu gets a click for close
			InputSystem.MouseUp += new MouseEventHandler(MouseUp);
			InputSystem.MouseMove += new MouseEventHandler(MouseMove);
			InputSystem.KeyDown += new KeyEventHandler(_cName.KeyDown);
			InputSystem.KeyUp += new KeyEventHandler(_cName.KeyUp);
		}

		public void UnRegisterEvents()
		{
			/* unregister the buttons in the company menu
			foreach(IMouseInteractive nButton in _caControls) { 
				InputSystem.MouseUp -= new MouseEventHandler(nButton.MouseUp);
				InputSystem.MouseMove -= new MouseEventHandler(nButton.MouseMove);
			}*/

			// the menu gets a click for close
			InputSystem.MouseUp -= MouseUp;
			InputSystem.MouseMove -= MouseMove;
			InputSystem.KeyDown -= _cName.KeyDown;
			InputSystem.KeyUp -= _cName.KeyUp;
		}

		#endregion
	}
}