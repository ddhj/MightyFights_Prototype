using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype
{
	public class Cursor : BasicSprite, IUpdate
	{
		#region IUpdate Members

		public void Update(GameTime tTime)
		{
			MouseState	cState = Mouse.GetState();
			Point		tPoint = new Point(cState.X, cState.Y);

			// set the position of the cursor
			this.tPos = new Vector2(cState.X, cState.Y);
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White, 0, cFrame.tTopLeft, 1, SpriteEffects.None, 0);
		}

		#endregion
	}
}
