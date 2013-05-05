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
			if(_cStats.iHp <= 0) { 
				cAiData.eState = EBattleAiStates.Dying;
				_cActionMgr.cActionQueue.Clear();
				_cAnimProc.SetAnimationCriteria("Death", "Normal", "death", 1);

				// remove ourselves from our opponents attaking point
				if(nOpponent != null)
					if(_bAttacking)
						nOpponent.RemoveAttacker(_eAttackingPos);

				// remove the dying trooper from the zone they are in 
				DataStore.cInstance.cBattleData.RemoveDeadCombatant(this);

				// remove this perm action from the list so it no longer processes
				cAction.bConditionNotMet = false;
				return false;
			}

			// call our flee huristic
			cAiData.cHurisitics[EBattleHuristics.Flee](_cBattleDataRef);

			// sort the z order by y pos
			_fZorder = 1 - _tCenter.Y / 684;

			// update position in the battle zone
			if(_cBattleDataRef != null)
				_cBattleDataRef.SetZone(this);

			return true;
		}

		public bool BasicBattleManager(Action cAction)
		{
			if(cAction.bInit) { 
				_cBattleDataRef = (BattlegroundData)cAction.oData;
				cAction.bInit = false;
			}

			switch(_cBattleDataRef.eState) {
 				case EBattlegroundState.Battle:
					// check to see if we are in the middle of an animation, the only one 
					// we care about is the ready state
					switch(cAiData.eState) { 
						case EBattleAiStates.Idle:

						break;

						case EBattleAiStates.Panting:
							// yep, we are going to do all the stuff for panting
							cAiData.cHurisitics[EBattleHuristics.Pant](_cBattleDataRef);
						break;

						// idle can be ready or pant
						case EBattleAiStates.Ready:
							// use the huristic to check for an opponent
							cAiData.cHurisitics[EBattleHuristics.ChooseOpponent](_cBattleDataRef);
						break;

						case EBattleAiStates.Defending:
						case EBattleAiStates.Attacking:
							// check to see if we are animating or not 
							if(!cAnimationProcessor.bActive) 
								cAiData.cHurisitics[EBattleHuristics.Attack](_cBattleDataRef);
						break;
						
						case EBattleAiStates.Pursuit:
							// run the persue huristic
							cAiData.cHurisitics[EBattleHuristics.Persue](_cBattleDataRef);
						break;

						// we are dead we don't need to do more 
						case EBattleAiStates.Dying:
							if(!cAnimationProcessor.bActive)
								cAiData.eState = EBattleAiStates.Dead;
						break;

						case EBattleAiStates.Dead: 
							bActive = false;
							cAction.bConditionNotMet = false;
							return false;
					}
				break;

				case EBattlegroundState.Victory: 
					if(cAiData.eState != EBattleAiStates.Dying) { 
						_cActionMgr.cActionQueue.Clear();
						_cAnimProc.SetAnimationCriteria("Idle", "Victory", "victory", -1);
						cAction.bConditionNotMet = false;
					}
				break;
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
			this.tPos += tDirVect * 1.5f;
			
			if((tDest - _tPos).LengthSquared() < 2) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
				cAction.bConditionNotMet = false;
				_tPos = tDest;

				cAiData.eState = EBattleAiStates.Ready;
				return false;
			}

			return true;
		}

		public bool FleeToPoint(Action cAction)
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
				_cAnimProc.SetAnimationCriteria("Move", "Flee", "retreat", -1);

				cAction.bInit = false;
			} else tDirVect = (Vector2)cAction.oCanvas;

			// move the sprite by the speed of walk (this data should come from the template)
			////ddhj Template add for speed of walk
			this.tPos += tDirVect * _fFinalMovementSpeed;
			
			if((tDest - _tPos).LengthSquared() < 2) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Pant", "pant", -1);
				cAction.bConditionNotMet = false;
				_tPos = tDest;
				
				cAiData.eState = EBattleAiStates.Panting;
				return false;
			}

			return true;
		}

		public bool PersueOpponent(Action cAction)
		{
			Vector2		tDest,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// set our animation to charge 
				_cAnimProc.SetAnimationCriteria("Move", "Run", "run", -1);

				// set our state to persuit
				cAiData.eState = EBattleAiStates.Pursuit;

				cAction.bInit = false;
			} 

			// its possible that the opponent will die before we get there so check to see if the 
			if(nOpponent.IsDead()) { 
				cAiData.eState = EBattleAiStates.Ready;
				nOpponent = null;
				cAction.bConditionNotMet = false;
				return false;
			}

			tDest = nOpponent.RequestPersuitPoint(_eAttackingPos);
			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			// move the sprite by the speed of run (this data should come from the template)
			////ddhj Template add for speed of run
			this.tPos += tDirVect * _fFinalMovementSpeed;
			
			// we are within weapon range so switch our system to attack 
			if(((tDest - _tCenter).LengthSquared()) < ( this.iWeaponRange * this.iWeaponRange )) { 
				// call the attack huristic because we are within attack range for our weapon 
				//// ddhj this will need a tweek for weapon range 
				_bAttacking = true;
				cAiData.cHurisitics[EBattleHuristics.Attack](DataStore.cInstance.cBattleData);

				cAction.bConditionNotMet = false;
				return false;
			}

			return true;
		}

		public bool ChargeOpponent(Action cAction)
		{
			Vector2		tDest,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// set our animation to charge 
				_cAnimProc.SetAnimationCriteria("Move", "Run", "run", -1);

				// set our state to persuit
				cAiData.eState = EBattleAiStates.Pursuit;

				cAction.bInit = false;
			} 

			// its possible that the opponent will die before we get there so check to see if the 
			if(nOpponent.IsDead()) { 
				cAiData.eState = EBattleAiStates.Ready;
				nOpponent = null;
				cAction.bConditionNotMet = false;
				return false;
			}

			// check to see if while running at the opponent he has filled up his attack quota
			if(!nOpponent.bAvailablePos) { 
				cAiData.eState = EBattleAiStates.Ready;
				nOpponent = null;
				cAction.bConditionNotMet = false;
				return false;
			}

			tDest = nOpponent.RequestAttackPoint(this, out _eAttackingPos);
			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			// move the sprite by the speed of run (this data should come from the template)
			////ddhj Template add for speed of run
			this.tPos += tDirVect * _fFinalMovementSpeed;
			
			// we are within weapon range so switch our system to attack 
			if(((tDest - _tCenter).LengthSquared()) < ( this.iWeaponRange * this.iWeaponRange )) { 
				nOpponent.SetAttacker(this, out _eAttackingPos);
				_bAttacking = true;

				// call the attack huristic because we are within attack range for our weapon 
				//// ddhj this will need a tweek for weapon range 
				cAiData.cHurisitics[EBattleHuristics.Attack](DataStore.cInstance.cBattleData);

				cAction.bConditionNotMet = false;
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

		public bool DamageObj(Action cAction)
		{
			if(cAction.bInit) { 
				cAction.oCanvas = new AnimatingDamage((int)cAction.oData, _tCenter);
				_caDamageList.Add((AnimatingDamage)cAction.oCanvas);
				cAction.oData = _caDamageList.Count - 1;
				cAction.bInit = false;
				return true;
			}

			AnimatingDamage cDmg = (AnimatingDamage)cAction.oCanvas;
			cDmg.Update(DataStore.cInstance.cTime);
			if(cDmg.bActive == false) { 
				_caDamageList.RemoveAt((int)cAction.oData);
				cAction.bConditionNotMet = false;
				return false;
			}

			return true;
		}
	}
}
