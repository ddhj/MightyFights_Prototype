using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	interface IOpponent
	{
		void DealDamage();
		Vector2 tPos		{ get; set; }

		// possible a list of attackers and their positions for the persuit method 
		// in order to choose where to run s
	}
}
