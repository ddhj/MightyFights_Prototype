using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype
{
	public delegate void DProcessClick(object oSender, object oArgs);

	public interface IClickable
	{
		bool ContainsPoint(Point tPoint);
		bool ContainsPoint(Vector2 tLocation);
		DProcessClick dlProcessClick	{ get; set; }
	}

	public delegate void DProcessMouseEvent(object oSender);
	
	public interface IMouseInteractive : IClickable
	{
		DProcessMouseEvent	dlMouseIn		{ get; set; }
		DProcessMouseEvent	dlMouseOut		{ get; set; }
		bool MouseMove(MouseState tMouseState);
	}
}
