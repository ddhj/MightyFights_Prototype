using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	public class ClickableKnight : ClickableSprite, IActiveBasic
	{
		public void ProcessClick(object oSender, object oArgs)
		{

		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		#endregion
	}
}