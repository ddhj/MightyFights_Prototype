using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	interface IActive<T>
	{
		void Process(GameTime cTime);
		bool bActive { get; set; }
		ActionManager<T> cActionManager		{ get; set; }
	}
}
