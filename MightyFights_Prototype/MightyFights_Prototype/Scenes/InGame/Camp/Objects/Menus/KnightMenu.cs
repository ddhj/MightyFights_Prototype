using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	public class KnightMenu : ClickableSprite, IActiveBasic, IMenuObj
	{
		ClickableSprite		_cCaptains,
							_cCompanyDialog,
							_cTemplates,
							_cDialogue;

		public object		oMenuObject	{ get { return this; }}
		public IMenuObj		nParent		{ get; set; }
		public object		oResultData	{ get { return null; }}

		public KnightMenu(IMenuObj nMenuParent) : base()
		{
			nParent = nMenuParent;

		}
		
		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			throw new NotImplementedException();
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