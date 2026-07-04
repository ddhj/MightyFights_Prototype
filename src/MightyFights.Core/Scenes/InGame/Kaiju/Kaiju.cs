// system includes
using System;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: Kaiju Hunt (docs/DESIGN_DIRECTION.md) -- the monster is a third combatant archetype
	//// built as a Trooper subclass so it rides the entire validated battle pipeline for free:
	//// zones, targeting, buffs, damage numbers, crits/parries, exp counters. v1 deliberately
	//// keeps it inside the zone system as one huge single-zone combatant (the plan's "outside
	//// the zone dicts" refinement plus AoE attacks and boss action patterns come with real kaiju
	//// art via the stitcher task). Scale rendering is the same mechanism captains already used.
	public class Kaiju : Trooper
	{
		public Kaiju(int iId, Team cTeam, TrooperTemplate cTemplate, float fScale)
			: base(iId, cTeam, cTemplate)
		{
			_fScale = fScale;

			// a monster this size can be swarmed from everywhere, and its reach matches its bulk
			_iAvailablePositions = 14;
			this.iWeaponRange = 30;
			this.iWeaponRngSq = this.iWeaponRange * this.iWeaponRange;
		}
	}
}
