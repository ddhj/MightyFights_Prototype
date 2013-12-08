using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class ClickableKnight : ClickableSprite, IActiveBasic
	{
		public void ProcessClick(object oSender, object oArgs)
		{
			CampMenuManager cMgr = ((Camp)oSender).cMenuMgr;
			KnightMenu		cKnight = new KnightMenu();

			cKnight.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\knight_box");
			cKnight.tPos = new Vector2(83, 223);
			cKnight.sTexName = @"In Game\Camp\KnightMenu\knight_box";
			cKnight.cFrame = new Frame(cKnight.cTexRef.Bounds, 
				new Vector2(cKnight.cTexRef.Bounds.Width / 2, cKnight.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), 
				new Vector2(cKnight.cTexRef.Bounds.Width, cKnight.cTexRef.Bounds.Height), 
				new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			cMgr.AddMenuObject(cKnight);
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
		}

		#endregion
	}

	public class ClickableSmith : ClickableSprite, IActiveBasic
	{
		public void ProcessClick(object oSender, object oArgs)
		{

		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
		}

		#endregion
	}
}