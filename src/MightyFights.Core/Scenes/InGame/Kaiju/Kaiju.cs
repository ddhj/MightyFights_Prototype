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
		//// ddhj: 2026 -- owner call: the drop mechanic everywhere else in the game
		//// (Trooper_Actions.cs's Dead-state handling, untouched) fires on death, which is fine
		//// when armies are dozens of disposable troopers. A kaiju dies exactly once, at the very
		//// end of a whole hunt, so a death-only drop would mean zero rewards for the entire
		//// fight. Layered on top here, scoped to Kaiju alone: a drop fires every time the
		//// kaiju's HP crosses down through another 10% band. The base Trooper death-drop (a
		//// separate 1-in-10 roll) still fires normally on the final kill, on top of this.
		const int	iDropThresholdStep = 10;
		int			_iNextDropThresholdPct = 100 - iDropThresholdStep;

		public Kaiju(int iId, Team cTeam, TrooperTemplate cTemplate, float fScale)
			: base(iId, cTeam, cTemplate)
		{
			_fScale = fScale;

			// a monster this size can be swarmed from everywhere, and its reach matches its bulk
			_iAvailablePositions = 14;
			this.iWeaponRange = 30;
			this.iWeaponRngSq = this.iWeaponRange * this.iWeaponRange;
		}

		public override void DealDamage(Combatant nOpponent, int iDamage, bool bCrit)
		{
			base.DealDamage(nOpponent, iDamage, bCrit);

			// a single big hit (e.g. a crit) can cross more than one band at once; award one
			// drop per band crossed rather than capping at one per hit
			float	fHpPct = cStats.fHp / cStats.iMaxHp * 100f;
			while(_iNextDropThresholdPct > 0 && fHpPct <= _iNextDropThresholdPct) {
				DataStore.cInstance.cBattleData.CreateDrop(this);
				_iNextDropThresholdPct -= iDropThresholdStep;
			}
		}
	}
}
