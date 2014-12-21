using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class Ability : ClickableSprite
	{
		public Texture2D	cTexRefAlt { get; set; }
		
		public Ability() : base()
		{
			dlProcessClick = ProcessClick;
		}

		void ProcessClick(object oSender, object oArgs)
		{
			Texture2D cTmp = this.cTexRef;
			this.cTexRef = cTexRefAlt;
			cTexRefAlt = cTmp;
		}
	}
}
