using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	interface IActiveBasic
	{
		void Process(GameTime cTime);
		bool bActive { get; set; }
		int iActiveIdx { get; set; }
	}

	interface IActive<T> : IActiveBasic
	{
		ActionManager<T> cActionManager		{ get; set; }
	}
}
