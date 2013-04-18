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

		ICombatant ChooseZoneCombatant(List<ICombatant> naCombatants)
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

		public object ChooseOpponent(BattlegroundData cData)
		{
			int			iTmp = int.MaxValue;

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

				// call the attack huristic because we already know we want to fight
				cAiData.cHurisitics[EBattleHuristics.Attack](cData);
				return null;
			}
		
			// check if there are any opponents left at all
			if(cData.naMasterLists[this.iOpponentIndex].Count == 0) { 
				// there are no more guys so we are in a victory scenario 
				_cAnimProc.SetAnimationCriteria("Idle", "Victory", "victory", -1);
				cAiData.eState = EBattleAiStates.Idle;
				return null;
			}

			// we dont have any attakers so lets check our current zone for an opponent
			if(this.tZone.naCombatantLists[this.iOpponentIndex].Count > 0) { 
				nOpponent = ChooseZoneCombatant(this.tZone.naCombatantLists[this.iOpponentIndex]);
			// there are no dudes in our area so we are going to check surrounding areas 
			} else { 
				Random cRand = new Random();
				List<ICombatant> cTmpList = cData.naMasterLists[this.iOpponentIndex];
				nOpponent = cTmpList[cRand.Next(cTmpList.Count)];

				////// this should be a battle search huristic
				//Dictionary<IntPoint, bool>	cPointList = new Dictionary<IntPoint, bool>();
				//int		iY = this.tZone.iY, 
				//        iMaxY = this.tZone.iY,
				//        iX = this.tZone.iX, 
				//        iMaxX = this.tZone.iX;
				//Zone	tTmpZone;
				//IntPoint cTmpPoint;
				//while(nOpponent == null) { 
				//    --iY;
				//    --iX; 
				//    ++iMaxY;
				//    ++iMaxX;
				//    for(int i = iX; i < iMaxX && nOpponent == null; ++i) 
				//        for(int j = iY; j < iMaxY; ++j) { 
				//            // make a point
				//            cTmpPoint = new IntPoint(iX, iY);
				//            // check if we have already hit this point 
				//            if(!cPointList.ContainsKey(cTmpPoint)) { 
				//                cPointList.Add(cTmpPoint, true);
				//                tTmpZone = cData.GetZoneByCoords(iX, iY);
				//                if(tTmpZone != null)
				//                    if(tTmpZone.naCombatantLists[this.iOpponentIndex].Count > 0) { 
				//                        nOpponent = ChooseZoneCombatant(tTmpZone.naCombatantLists[this.iOpponentIndex]);
				//                        break;
				//                    }
				//            }		
				//        }	
				//}
			}

			// if we have an opponent at this point we need to charge them
			cActionManager.AddAction(new Action(ChargeOpponent, null, null));
			cAiData.eState = EBattleAiStates.Pursuit;

			return null;
		}
	}
}
