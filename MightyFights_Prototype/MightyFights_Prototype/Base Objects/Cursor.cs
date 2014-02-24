using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype
{
	public class Cursor : BasicSprite
	{
		#region IUpdate Members

		public void Update(Point tPoint)
		{
			// set the position of the cursor
			this.tPos = new Vector2(tPoint.X, tPoint.Y);
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White, 0, cFrame.tTopLeft, 1, SpriteEffects.None, 0);
		}

		#endregion
	}
}
