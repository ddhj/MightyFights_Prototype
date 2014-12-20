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
	public class KnightMenu : ClickableSprite, IMenuObj, IMouseInteractive
	{
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

				HighlightButton cTmpSpr = new HighlightButton();
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

				cTmpSpr = new HighlightButton();
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

				cTmpSpr = new HighlightButton();
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

				cTmpSpr = new HighlightButton();
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\template_button");
				cTmpSpr.tPos = new Vector2(166, 259);
				cTmpSpr.sTexName = @"In Game\Camp\KnightMenu\template_button";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				cTmpSpr.dlProcessClick = TemplateClick;
				_naButtons.Add(cTmpSpr);
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
			// draw the buttons
			foreach(HighlightButton cButton in _naButtons)
				cButton.Draw(cBatch);
		}

		void ProcessClick()
		{
			// loop through the buttons 

		}

		void TemplateClick(object oSender, object oArgs)
		{
			Template		cTemplate = new Template(_cMgr, DataStore.cInstance.cLSteward.cTemplates[0]);
			Viewport		cView = DataStore.cInstance.cGraphics.Viewport;

			cTemplate.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Template\template_window");
			cTemplate.tPos = new Vector2(cView.Width / 2 - cTemplate.cTexRef.Width / 2, cView.Height / 2 - cTemplate.cTexRef.Height / 2);
			cTemplate.sTexName = @"In Game\Camp\Company Dialog\company_window";
			cTemplate.cFrame = new Frame(cTemplate.cTexRef.Bounds, 
				new Vector2(cTemplate.cTexRef.Bounds.Width / 2, cTemplate.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), 
				new Vector2(cTemplate.cTexRef.Bounds.Width, cTemplate.cTexRef.Bounds.Height), 
				new Vector2(0, 0), new Vector2(0, 0), null, false, false);

			// set the background sprite first because everything in the menu will be based on the position of the top left point
			if(cTemplate.InitMenu())
				_cMgr.AddMenuObject(cTemplate);
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
			if(cCompany.InitMenu())
				_cMgr.AddMenuObject(cCompany);
		}

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			if(!ContainsPoint(eMouseEvt.Location))
				_cMgr.RemoveMenuObject(this);
		}

		public void MouseHover(object oSender, MouseEventArgs eMouseEvt)
		{

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
			foreach(IMouseInteractive nButton in _naButtons) { 
				InputSystem.MouseUp += new MouseEventHandler(nButton.MouseUp);
				InputSystem.MouseMove += new MouseEventHandler(nButton.MouseMove);
			}

			// the menu gets a click for close
			InputSystem.MouseUp += new MouseEventHandler(MouseUp);
		}

		public void UnRegisterEvents()
		{
			foreach(IMouseInteractive nButton in _naButtons) { 
				InputSystem.MouseUp -= nButton.MouseUp;
				InputSystem.MouseHover -= nButton.MouseHover;
			}

			// the menu gets a click for close
			InputSystem.MouseUp -= MouseUp;
		}

		#endregion
	}
}