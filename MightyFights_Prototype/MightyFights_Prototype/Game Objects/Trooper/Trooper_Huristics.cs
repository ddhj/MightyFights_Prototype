using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public partial class Trooper
	{
		public object Idle(BattlegroundData cData)
		{
			return null;
		}

		public object Attack_Basic(BattlegroundData cData)
		{
			Random	cRand = new Random();
			int		iAttckPercent;

			// check to see if our opponent is living 
			if(nOpponent.IsDead()) { 
				// null out my opponent because they are dying 
				nOpponent = null;
				_bAttacking = false;

				// move our state to ready which will choose another opponent
				cAiData.eState = EBattleAiStates.Ready;
				return null;
			}

			// get our percent for this attack
			////ddhj: good time for the other stuff for percent attack crit weighting and all that jaz
			iAttckPercent = cRand.Next(100);

			cAiData.eState = EBattleAiStates.Attacking;

			// we are going to crit
			if(iAttckPercent > 90) { 
				switch(cRand.Next(2)) { 
					case 0: _cAnimProc.SetAnimationCriteria("Attack", "Critical", "bigchop", 1); break;
					case 1: _cAnimProc.SetAnimationCriteria("Attack", "Critical", "lunge", 1); break;
				}
			// we are going to do a normal attack
			} else if(iAttckPercent > 10) { 
				switch(cRand.Next(3)) { 
					case 0: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "low", 1); break;
					case 1: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stab", 1); break;
					case 2: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stick", 1); break;
				}
			// we are going to parry
			} else { 
				cAiData.eState = EBattleAiStates.Defending;
				switch(cRand.Next(2)) { 
					case 0: _cAnimProc.SetAnimationCriteria("Defend", "Parry", "lp", 1); break;
					case 1: _cAnimProc.SetAnimationCriteria("Defend", "Parry", "sp", 1); break;
				}
			}

			return null;
		}

		public object Flee(BattlegroundData cData)
		{
			return null;
		}

		public object Persue(BattlegroundData cData)
		{
			// this is where we would put in some state changing code for 
			// stamina checks and checks to see if we are going to keep persuit

			// default is charge for ever

			return null;
		}

		public object Pant(BattlegroundData cData)
		{
			// check to see if we are still panting 
			if(cAnimationProcessor.bActive)
				_cStats.iHp += 5;
			else { 
				cAiData.eState = EBattleAiStates.Ready;
				_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
			}

			return null;
		}

		ICombatant ChooseZoneCombatantDst(List<ICombatant> naCombatants)
		{
			// lets grab the closest guy 
			////ddhj: this could technically be a huristic if we wanted 
			Vector2		tTarget;
			int			iTmp = int.MaxValue,
						iTmp2;
			ICombatant	nNewOpponent = null;

			foreach(ICombatant nCombatant in naCombatants) { 
				// get the vector to the combatant and test if its the shortest
				tTarget = nCombatant.tPos - _tPos;
				if((iTmp2 = (int)tTarget.LengthSquared()) < iTmp) { 
					nNewOpponent = nCombatant;
					iTmp = iTmp2;
				}
			}

			return nNewOpponent;
		}

		ICombatant ChooseZoneCombatantRand(List<ICombatant> naCombatants)
		{
			int			iIndex;
			Random		cRand = new Random();
			ICombatant	nCombatant;
								// copy list for removal
			List<ICombatant>	naTmpList = new List<ICombatant>( naCombatants );

			// while there are opponents in the list
			while(naTmpList.Count > 0) { 
				// remove testing combatant
				iIndex = cRand.Next(naTmpList.Count);
				nCombatant = naTmpList[iIndex];
				naTmpList.RemoveAt( iIndex );

				// check to see if there are any positions available
				if(nCombatant.bAvailablePos)
					return nCombatant;
			}

			return null;
		}

		public object ChooseOpponent(BattlegroundData cData)
		{
			int			iTmp = int.MaxValue,
						iOpponentIdx = this.iOpponentIndex, 
						iArmyIdx = this.iArmyIndex;

			// not sure if this is required or not but just in case 
			if(nOpponent != null)
				return null;

			// check to see if I have any attackers currently attacking me 
			if(_caAttackers.Count > 0) { 
				// check the weakest of my opponents and attack them
				foreach(ICombatant nCombatant in _caAttackers.Values) { 
					if(nCombatant.cStats.iHp < iTmp) { 
						nOpponent = nCombatant;
						iTmp = nCombatant.cStats.iHp;
					}
				}

				//// call the attack huristic because we already know we want to fight
				//cAiData.cHurisitics[EBattleHuristics.Attack](cData);

				//return null;
			// we dont have any attakers so lets check our current zone for an opponent			
			} else if(this.cZone.naCombatantLists[iOpponentIdx].Count > 0) 
				nOpponent = ChooseZoneCombatantRand(this.cZone.naCombatantLists[iOpponentIdx]);
			
			// there was either no dudes in my zone or they do not have available attack points
			if(nOpponent == null) { 
				// check our army for which zones they are in and if there are any opponents there
				// lets get the closest zone to our own
				Vector2 tZone = new Vector2(cZone.iX, cZone.iY),
						tNewZone;

				Dictionary<IntPoint, Zone>	caActiveZones = cData.caActiveZones[iOpponentIndex];
				SortedList<int, List<Zone>>	cClosestZones = new SortedList<int,List<Zone>>();
				List<Zone>	cZoneList = null;

				// walk through the active zones 
				foreach(KeyValuePair<IntPoint, Zone> tZoneData in caActiveZones) { 
					// check to see if we have any opponents in this zone 
					if(tZoneData.Value.naCombatantLists[iOpponentIndex].Count > 0) {
						// make a vecotor and store the zone in a sorted list by distance
						tNewZone = new Vector2(tZoneData.Value.cPoint.iX, tZoneData.Value.cPoint.iY);
						tNewZone = tZone - tNewZone;
						
						// get distnace and add to sorted list
						iTmp = (int)tNewZone.LengthSquared();
						if(!cClosestZones.TryGetValue(iTmp, out cZoneList))
							cClosestZones.Add(iTmp, cZoneList = new List<Zone>());
						
						// add zone to internal list for collision on distance
						cZoneList.Add(tZoneData.Value);
					}
				}

				// walk the sorted zones 
				foreach(List<Zone> caZones in cClosestZones.Values) { 
					foreach(Zone cNewZone in caZones) 
						if((nOpponent = ChooseZoneCombatantRand(cNewZone.naCombatantLists[iOpponentIndex])) != null) {
							// if we have an opponent at this point we need to charge them
							cActionManager.AddAction(new Action(ChargeOpponent, null, null));
							cAiData.eState = EBattleAiStates.Pursuit;
							return null;
						}						
				}
			}

			if(nOpponent != null) { 
				// if we have an opponent at this point we need to charge them
				cActionManager.AddAction(new Action(ChargeOpponent, null, null));
				cAiData.eState = EBattleAiStates.Pursuit;
			}

			return null;
		}
	}
}
