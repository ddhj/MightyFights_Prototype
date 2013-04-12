using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype
{
	public class Cursor : BasicSprite, IUpdate
	{
		#region IUpdate Members

		public void Update(GameTime cTime)
		{
			MouseState	cState = Mouse.GetState();
			Point		tPoint = new Point(cState.X, cState.Y);

			// set the position of the cursor
			_tPos = new Vector2(cState.X, cState.Y);
		}

		#endregion
	}
}
