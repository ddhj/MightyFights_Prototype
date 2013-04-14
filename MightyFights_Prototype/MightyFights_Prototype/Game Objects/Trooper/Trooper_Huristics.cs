using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MightyFights_Prototype
{
	public partial class Trooper
	{
		public void Idle(BattlegroundData cData)
		{

		}

		public void Attack_Basic(BattlegroundData cData)
		{
			Random	cRand = new Random();
			int		iAttckPercent;

			// check to see if our opponent is living 
			if(nOpponent.IsDead()) { 
				// null out my opponent because they are dying 
				nOpponent = null;

				// move our state to ready which will choose another opponent
				cAiData.eState = EBattleAiStates.Ready;
				return;
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
				switch(cRand.Next(2)) { 
					case 0: _cAnimProc.SetAnimationCriteria("Defend", "Parry", "lp", 1); break;
					case 1: _cAnimProc.SetAnimationCriteria("Defend", "Parry", "sp", 1); break;
				}
			}
		}

		public void Flee(BattlegroundData cData)
		{

		}

		public void Persue(BattlegroundData cData)
		{
			// this is where we would put in some state changing code for 
			// stamina checks and checks to see if we are going to keep persuit

			// default is charge for ever
		}

		public void Pant(BattlegroundData cData)
		{

		}

		public void ChooseOpponent(BattlegroundData cData)
		{
			if(nOpponent == null) { 
				// check the battle data for another dude
				if(cData.naOpponents.Count > 0) { 
					// lets get another one is some magic way 
					List<ICombatant> naOpponents =  cData.caBattleLists[iOpponentIndex];
					nOpponent = naOpponents[new Random().Next(naOpponents.Count)];
					// check to see if this chosen opponent is in a dying state
					if(nOpponent.IsDead()) { 
						// walk the list of opponents one by one and choose a non dead or dying opponent
						nOpponent = null;
						foreach(ICombatant nNextOp in naOpponents)
							if(!nNextOp.IsDead())
								nOpponent = nNextOp;
							
						if(nOpponent == null) { 
							_cAnimProc.SetAnimationCriteria("Idle", "Victory", "victory", -1);
							cAiData.eState = EBattleAiStates.Idle;
							return;
						}
					}

					// there is going to be checking and position code and all of that comming 
				// we are in a victory situation
				} else { 
					_cAnimProc.SetAnimationCriteria("Idle", "Victory", "victory", -1);
					cAiData.eState = EBattleAiStates.Idle;
					return;
				}
			}

			cActionManager.AddAction(new Action(ChargeOpponent, null, null));
			cAiData.eState = EBattleAiStates.Pursuit;
		}
	}
}
