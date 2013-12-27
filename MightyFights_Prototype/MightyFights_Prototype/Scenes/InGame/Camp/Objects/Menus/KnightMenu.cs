using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype
{
	public class KnightMenu : ClickableSprite, IMenuObj
	{
		ClickableSprite		_cCaptains,
							_cCompanyDialog,
							_cTemplates,
							_cDialogue;

		bool				_bProcessClick;

		public object		oMenuObject	{ get { return this; }}
		public object		oResultData	{ get { return null; }}

		CampMenuManager		_cMgr;

		public KnightMenu(CampMenuManager cMgr) : base()
		{
			_cMgr = cMgr;
		}
		
		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			MouseState			cMouseState = Mouse.GetState();
			Point				tPoint = new Point(cMouseState.X, cMouseState.Y);
			ClickableSprite		cActiveSprite = null;

			// check to see if the click was out of the bounds of the main menu object for the knight
			// if so then we are done
			if(this.ContainsPoint(tPoint)) { 
				// test the four buttons for which one is active
				if(_cCaptains.ContainsPoint(tPoint))
					

				if(cMouseState.LeftButton == ButtonState.Pressed) { 
					if(_bProcessClick == true) { 
						ProcessClick();
									
						_bProcessClick = false;
					}
				} else _bProcessClick = true;
				// check the mouse position 
			} 
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
		}

		void ProcessClick()
		{
			// loop through the buttons 

		}

		void TemplateClick(object oSender, object oArgs)
		{

		}
	}
}