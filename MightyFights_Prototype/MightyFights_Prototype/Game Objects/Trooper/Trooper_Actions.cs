using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public partial class Trooper 
	{
		public bool TrooperUpkeep(Action cAction)
		{

			return true;
		}

		public bool BasicBattleManager(Action cAction)
		{
			if(cAction.bInit) { 
				
				cAction.bInit = false;
			}

			if(((BattlegroundData)cAction.oData).eState == EBattlegroundState.Battle) { 
				// check to see if we are in the middle of an animation, the only one 
				// we care about is the ready state
				switch(cAiData.eState) { 
					// idle can be ready or pant
					case EBattleAiStates.Ready:
						// run the pant huristic 
						cAiData.cHurisitics[EBattleHuristics.ChooseOpponent]((BattlegroundData)cAction.oData);
					break;

					case EBattleAiStates.Attacking:
						// check to see if we are animating or not 
						if(!cAnimationProcessor.bActive) 
							cAiData.cHurisitics[EBattleHuristics.Attack]((BattlegroundData)cAction.oData);
					break;
						
					// we are dead we don't need to do more 
					case EBattleAiStates.Dead: 
						return false;
				}
			}

			return true;
		}

		public bool MoveToPoint(Action cAction)
		{
			Vector2		tDest = (Vector2)cAction.oData,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// make the direction vector and set the direction for the sprite
				tDirVect = tDest - _tPos;
				bDir = tDirVect.X > 0;
				tDirVect.Normalize();
				cAction.oCanvas = tDirVect;
				_cAnimProc.SetAnimationCriteria("Move", "Walk", "walk", -1);

				cAction.bInit = false;
			} else tDirVect = (Vector2)cAction.oCanvas;

			// move the sprite by the speed of walk (this data should come from the template)
			////ddhj Template add for speed of walk
			_tPos += tDirVect * 2.5f;
			
			if((tDest - _tPos).LengthSquared() < 2) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
				cAction.bConditionNotMet = false;
				_tPos = tDest;

				cAiData.eState = EBattleAiStates.Ready;
				return false;
			}

			return true;
		}

		public bool ChargeOpponent(Action cAction)
		{
			Vector2		tDest = nOpponent.tPos,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// set our animation to charge 
				_cAnimProc.SetAnimationCriteria("Move", "Run", "run", -1);

				// set our state to persuit
				cAiData.eState = EBattleAiStates.Pursuit;

				cAction.bInit = false;
			} 

			tDirVect = tDest - _tPos;
			bDir = tDirVect.X > 0;
			tDirVect.Normalize();

			// move the sprite by the speed of walk (this data should come from the template)
			////ddhj Template add for speed of run
			_tPos += tDirVect * 2.5f;
			
			// we are within weapon range so switch our system to attack 
			if((tDest - _tPos).LengthSquared() < 25) { 
				// call the attack huristic because we are within attack range for our weapon 
				//// ddhj this will need a tweek for weapon range 
				cAiData.cHurisitics[EBattleHuristics.Attack](DataStore.cInstance.cBattleData);

				cAction.bConditionNotMet = false;
				_tPos = tDest;

				cAiData.eState = EBattleAiStates.Ready;
				return false;
			}

			return true;
		}

		public bool Wait(Action cAction)
		{
			// check to see if the canvas (time) is longer than the data in (time) 
			TimeSpan	tTime = (TimeSpan)cAction.oCanvas;

			if(cAction.bInit) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
				cAction.bInit = false;
			}

			cAction.oCanvas = tTime += DataStore.cInstance.cTime.ElapsedGameTime;
			if(tTime > TimeSpan.FromMilliseconds((double)(int)cAction.oData)) { 
				cAction.bConditionNotMet = false;
				return false;
			}
			return true;
		}
	}
}
