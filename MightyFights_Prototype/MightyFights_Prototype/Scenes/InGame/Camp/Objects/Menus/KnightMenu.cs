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
			
			// check to see if the click was out of the bounds of the main menu object for the knight
			// if so then we are done
			if(this.ContainsPoint(tPoint)) { 
				// check the mouse position 
			} else _cMgr.RemoveMenuObject(this);
			
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
		}

		public void ProcessClick(object oSender, object oArgs)
		{
			CampMenuManager		cMenuMgr = ((Camp)oSender).cMenuMgr;

			// loop through the buttons 

		}

	}
}