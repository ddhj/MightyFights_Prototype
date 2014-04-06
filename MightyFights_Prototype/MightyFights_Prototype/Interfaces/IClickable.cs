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
		void MouseMove(object oSender, MouseEventArgs eMouseEvt);
		void MouseDown(object oSender, MouseEventArgs eMouseEvt);
		void MouseUp(object oSender, MouseEventArgs eMouseEvt);
		void MouseHover(object oSender, MouseEventArgs eMouseEvt);
		void MouseWheel(object oSender, MouseEventArgs eMouseEvt);
		void MouseDoubleClick(object oSender, MouseEventArgs eMouseEvt);
	}
}
