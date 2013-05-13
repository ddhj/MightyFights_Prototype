using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	interface IClickable
	{
		bool ContainsPoint(Point tPoint);
		bool ContainsPoint(Vector2 tLocation);
	}
}
