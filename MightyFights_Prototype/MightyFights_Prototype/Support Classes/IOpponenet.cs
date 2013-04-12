using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public interface ICombatant
	{
		AiBattleData cAiData	{ get; set; }
		Vector2 tPos			{ get; set; }
		ICombatant nOpponent	{ get; set; }

		void DealDamage();

		// possible a list of attackers and their positions for the persuit method 
		// in order to choose where to run s
	}
}
