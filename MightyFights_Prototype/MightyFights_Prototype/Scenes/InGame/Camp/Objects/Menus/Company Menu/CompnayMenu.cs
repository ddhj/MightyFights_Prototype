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
	public class CompanyMenu : ClickableSprite, IMenuObj
	{
		bool				_bProcessClick;
		public object		oMenuObject	{ get { return this; }}
		public object		oResultData	{ get { return null; }}

		CampMenuManager		_cMgr;

		List<IMouseInteractive>		_naButtons = new List<IMouseInteractive>();

		public CompanyMenu(CampMenuManager cMgr) : base()
		{
			_cMgr = cMgr;
		}
		
		public bool InitMenu()
		{
			try { 
				DataStore		cData = DataStore.cInstance;
				ContentManager	cContent = cData.cContent;
				
				TemplateButton cTmpSpr = new TemplateButton();
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\company_name");
				cTmpSpr.tPos = new Vector2(tPos.X - cTmpSpr.cTexRef.Width / 2, tPos.Y + 20);
				cTmpSpr.sTexName = @"In Game\Camp\Company Dialog\company_name";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				
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
			ClickableSprite		cActiveSprite = null;

			// check to see if the click was out of the bounds of the main menu object for the knight
			// if so then we are done
			if(cMouseState.LeftButton == ButtonState.Pressed) { 
				if(this.ContainsPoint(tPoint)) { 
					// test the four buttons for which one is active
					if(cActiveSprite != null)	cActiveSprite.dlProcessClick(this, null);

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

			foreach(IMouseInteractive nButton in _naButtons) { 
				((TemplateButton)nButton).Draw(cBatch);
			}
		}

		void ProcessClick()
		{
			// loop through the buttons 

		}
	}
}