using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public delegate void DProcessClick(object oSender, object oArgs);

	interface IClickable
	{
		bool ContainsPoint(Point tPoint);
		bool ContainsPoint(Vector2 tLocation);
		DProcessClick dlProcessClick	{ get; set; }
	}
}
