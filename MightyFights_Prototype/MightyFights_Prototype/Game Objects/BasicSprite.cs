using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class BasicSprite : IDrawable
	{
		public Texture2D cTexRef { get; set; }
		public Vector2 tPos { get; set; }
		public Frame cFrame { get; set; }
	}
}
