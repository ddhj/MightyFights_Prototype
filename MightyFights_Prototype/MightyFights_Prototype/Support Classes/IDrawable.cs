using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	interface IDrawable
	{
		Texture2D	cTexRef		{ get; set; }
		Vector2		tPos		{ get; set; }
		Frame		cFrame		{ get; set; }
	}
}
