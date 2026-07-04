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

			//// ddhj: owner call -- with the radial attack-slot rewrite, raising this actually
			//// creates that many real, evenly-spaced rendezvous points (see
			//// Trooper_Combatant.cs), not crowding. 14 was still too tight -- most of a swarm
			//// stood around waiting for a slot. Set high enough that "a whole army surrounds
			//// the kaiju" is basically never artificially capped; iReserveMax (KaijuHunt.cs) is
			//// the real ceiling on how many troopers can ever exist at once anyway.
			_iAvailablePositions = 200;
			this.iWeaponRange = 30;
			this.iWeaponRngSq = this.iWeaponRange * this.iWeaponRange;
            cAiData.cHeurisitics[EBattleHeuristics.Attack] = Attack_Kaiju;

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

        public object Attack_Kaiju(BattlegroundData cData)
        {
            Random cRand = DataStore.cInstance.cRand;
            int iAttackPercent,
                        iCritChance;
            Combatant nOpponent = (Combatant)this.cTarget;

            // check to see if our opponent is living 
            if (nOpponent.IsDead())
            {
                // null out my opponent because they are dying 
                cTarget = null;
                base._bAttacking = false;

                // move our state to ready which will choose another opponent
                cAiData.eState = EBattleAiStates.Ready;
                return null;
            }

            // check to see if our opponent is running by distance check
            if (nOpponent.cAiData.eState == EBattleAiStates.Flee || !InWeaponRange(true))
            {
                // one in three chance to persue rather than attack somone else 
                nOpponent.RemoveAttacker(_iAttackingPos);
                cTarget = null;
                _bAttacking = false;
                cAiData.eState = EBattleAiStates.Ready;
                return null;
            }

            // get our percent for this attack
            // roll two tens 
            iAttackPercent = cRand.Next(100);
            iCritChance = 100 - (int)(100 * _cStats.fCrit);

            cAiData.eState = EBattleAiStates.Attacking;

            // we are going to crit
            if (iAttackPercent > iCritChance)
            {
                switch (cRand.Next(2))
                {
                    case 0: _cAnimProc.SetAnimationCriteria("Attack", "Critical", "bigchop", 1); break;
                    case 1: _cAnimProc.SetAnimationCriteria("Attack", "Critical", "lunge", 1); break;
                }
            }

            // kaiju defend 10% of the time
            if(cRand.Next(100) < 10)
            {
                ++_cExpData.iDefenceAttempts;
                cAiData.eState = EBattleAiStates.Defending;
                _cAnimProc.SetAnimationCriteria("Defend", "Parry", "parry", 1);
            }
            else
            {
                switch (cRand.Next(5))
                {
                    case 0: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "low", 1); break;
                    case 1: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stab", 1); break;
                    case 2: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stick", 1); break;
                    case 3: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "chop", 1); break;
                    case 4: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "chopb", 1); break;
                }
            }

            return null;
        }
    }
}
