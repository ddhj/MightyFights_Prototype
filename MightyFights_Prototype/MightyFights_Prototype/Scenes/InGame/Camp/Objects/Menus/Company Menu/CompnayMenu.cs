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
		public object		oMenuObject	{ get { return this; }}
		public object		oResultData	{ get { return null; }}

		CampMenuManager		_cMgr;

		List<MenuControlBase>		_caControls = new List<MenuControlBase>();

		public CompanyMenu(CampMenuManager cMgr) : base()
		{
			_cMgr = cMgr;
		}
		
		public bool InitMenu()
		{
			try { 
				DataStore		cData = DataStore.cInstance;
				ContentManager	cContent = cData.cContent;
				
				MenuControlKeyboard cTmpSpr = new MenuControlKeyboard(_cMgr);
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\company_name");
				cTmpSpr.tPos = new Vector2(tPos.X - cTmpSpr.cTexRef.Width / 2, tPos.Y + 20);
				cTmpSpr.sTexName = @"In Game\Camp\Company Dialog\company_name";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caControls.Add(cTmpSpr);

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

			foreach(ClickableSprite cButton in _caControls) { 
				cButton.Draw(cBatch);
			}
		}

		void ProcessClick()
		{
			// loop through the buttons 

		}

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
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
			// walk the controls
	
		}

		public void UnRegisterEvents()
		{
		
		}

		#endregion
	}
}