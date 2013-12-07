using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public partial class Trooper 
	{
		public bool TrooperUpkeep(Action cAction, GameTime cTime)
		{
			if(_cStats.fHp <= 0) { 
				cAiData.eState = EBattleAiStates.Dying;
				_cActionMgr.cActionQueue.Clear();
				if(DataStore.cInstance.cRand.Next(2) == 1) 
					_cAnimProc.SetAnimationCriteria("Death", "Normal", "death", 1);
				else _cAnimProc.SetAnimationCriteria("Death", "Normal", "deathb", 1);

				// remove ourselves from our opponents attaking point
				if(this.nTarget != null)
					if(this.nTarget is Combatant)
					{
						if(_bAttacking)
							((Combatant)nTarget ).RemoveAttacker(_iAttackingPos);
					}
					else if( this.nTarget is Healer )
						((Healer)this.nTarget ).FreeSpot( this );

				// remove the dying trooper from the zone they are in 
				DataStore.cInstance.cBattleData.RemoveDeadCombatant(this);

				// remove this perm action from the list so it no longer processes
				cAction.bConditionNotMet = false;
				return false;
			}

			// call our flee heuristic
			cAiData.cHeurisitics[EBattleHeuristics.Flee](_cBattleDataRef);

			// sort the z order by y pos
			_fZorder = 1 - _tCenter.Y / 684;

			// update position in the battle zone
			if(_cBattleDataRef != null)
				_cBattleDataRef.SetZone(this);

			return true;
		}

		void ProcessBuffList()
		{
		}

		public bool BasicBattleManager(Action cAction, GameTime cTime)
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
							cAiData.cHeurisitics[EBattleHeuristics.Pant](_cBattleDataRef);
						break;

						// idle can be ready or pant
						case EBattleAiStates.Ready:
							// use the heuristic to check for an opponent
							cAiData.cHeurisitics[EBattleHeuristics.ChooseOpponent](_cBattleDataRef);
							if(_cAnimProc.sType != "Idle")
								_cAnimProc.SetAnimationCriteria("Idle", "Battle", "stance", -1);
						break;

						case EBattleAiStates.Defending:
						case EBattleAiStates.Attacking:
							// check to see if we are animating or not 
							if(!cAnimationProcessor.bActive) 
								cAiData.cHeurisitics[EBattleHeuristics.Attack](_cBattleDataRef);
						break;
						
						case EBattleAiStates.Pursuit:
							// run the persue heuristic
							cAiData.cHeurisitics[EBattleHeuristics.Persue](_cBattleDataRef);
						break;

						// we are dead we don't need to do more 
						case EBattleAiStates.Dying:
							if(!cAnimationProcessor.bActive)
								cAiData.eState = EBattleAiStates.Dead;
						break;

						case EBattleAiStates.Dead: { 
							// one in 10 chance we get to spawn a buff
							if(DataStore.cInstance.cRand.Next(10) == 1) 
								_cBattleDataRef.CreateDrop(this);

							// set so the object no longer is active
							_eObjState &= ~EObjectStates.Active;
							cAction.bConditionNotMet = false;
							
							return false;
						}
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

		public bool MoveToPoint(Action cAction, GameTime cTime)
		{
			Vector2		tDest = (Vector2)cAction.oData,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// make the direction vector and set the direction for the sprite
				tDirVect = tDest - _tCenter;
				bDir = tDirVect.X > 0;
				tDirVect.Normalize();
				cAction.oCanvas = tDirVect;
				_cAnimProc.SetAnimationCriteria("Move", "Walk", "walk", -1);

				cAction.bInit = false;
			} else tDirVect = (Vector2)cAction.oCanvas;

			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			Vector2 tOld = _tCenter;
			// move the sprite by the speed of walk (this data should come from the template)
			////ddhj Template add for speed of walk
			this.tPos += tDirVect * 1.5f;

			if((tDest - _tCenter).LengthSquared() < 4) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Battle", "ready", -1);
				cAction.bConditionNotMet = false;
				cAiData.eState = EBattleAiStates.Ready;

				return false;
			}

			return true;
		}

		public bool FleeToHealer(Action cAction, GameTime cTime)
		{
			Healer		nHealer = (Healer)this.nTarget;
			Vector2		tDest,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// set our animation to charge 
				_cAnimProc.SetAnimationCriteria("Move", "Flee", "flee", -1);
				_lFleeCurTime = 0;

				cAction.bInit = false;
			} 

			// its possible that the opponent will die before we get there so check to see if the 
			if(!nHealer.bActive || !nHealer.bAvailableSpots ) {
				Flee( );
				cAction.bConditionNotMet = false;
				return false;
			}

			tDest = nHealer.GetOpenLocation( );
			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			// move the sprite by the speed of walk (this data should come from the template)
		////ddhj Template add for speed of walk
			if( _lFleeCurTime < _lFleeRunStop )
				this.tPos += tDirVect * _fRunMovementSpeed;
			else	this.tPos += tDirVect * _fWalkMovementSpeed;
			_lFleeCurTime += cTime.ElapsedGameTime.Milliseconds;
			
			// we are within weapon range so switch our system to attack 
			if(((tDest - _tCenter).LengthSquared()) < 4) {  
				nHealer.TakeSpot( this );
				_cAnimProc.SetAnimationCriteria("Idle", "Pant", "pant", -1);
				cAction.bConditionNotMet = false;

				this.cAiData.eState = EBattleAiStates.Panting;
				return false;
			}

			return true;
		}

		public bool FleeToPoint(Action cAction, GameTime cTime)
		{
			Vector2		tDest = (Vector2)cAction.oData,
						tDirVect;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// make the direction vector and set the direction for the sprite
				tDirVect = tDest - _tCenter;
				this.bDir = tDirVect.X > 0;
				tDirVect.Normalize();
				cAction.oCanvas = tDirVect;
				_cAnimProc.SetAnimationCriteria("Move", "Flee", "flee", -1);
				_lFleeCurTime = 0;

				cAction.bInit = false;
			} else tDirVect = (Vector2)cAction.oCanvas;

			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			// move the sprite by the speed of walk (this data should come from the template)
		////ddhj Template add for speed of walk
			if( _lFleeCurTime < _lFleeRunStop )
				this.tPos += tDirVect * _fRunMovementSpeed;
			else	this.tPos += tDirVect * _fWalkMovementSpeed;
			_lFleeCurTime += cTime.ElapsedGameTime.Milliseconds;

			if((tDest - _tCenter).LengthSquared() < 4) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Pant", "pant", -1);
				cAction.bConditionNotMet = false;

				cAiData.eState = EBattleAiStates.Panting;
				return false;
			}

			return true;
		}

		public bool PersueOpponent(Action cAction, GameTime cTime)
		{
			Vector2		tDest,
						tDirVect;
			Combatant	nOpponent = (Combatant)this.nTarget,
						nPasserby;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// set our animation to charge 
				_cAnimProc.SetAnimationCriteria("Move", "Run", "run", -1);

				// set our state to persuit
				cAiData.eState = EBattleAiStates.Pursuit;

				cAction.bInit = false;
			} 

			// its possible that the opponent will die before we get there so check to see if the 
			if(nOpponent.IsDead() || !nOpponent.bAvailablePos) { 
				cAiData.eState = EBattleAiStates.Ready;
				nTarget = null;
				cAction.bConditionNotMet = false;
				return false;
			}


			tDest =	nOpponent.RequestAttackPoint( this, out _iAttackingPos );
			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			// move the sprite by the speed of run (this data should come from the template)
		////ddhj Template add for speed of run
			if(( tDest - _tCenter ).LengthSquared( ) < 60000 )
				this.tPos += tDirVect * _fRunMovementSpeed;
			else	this.tPos += tDirVect * _fWalkMovementSpeed;

			// we are within weapon range so switch our system to attack 
			if( InWeaponRange( false )) {
//				this.tPos = tDest;
				nOpponent.SetAttacker(this, out _iAttackingPos);
				// call the attack heuristic because we are within attack range for our weapon 
				//// ddhj this will need a tweek for weapon range 
				_bAttacking = true;
				cAiData.cHeurisitics[EBattleHeuristics.Attack](DataStore.cInstance.cBattleData);

				cAction.bConditionNotMet = false;
				return false;
			}

			// okay we're chasing a guy, but lets look around and see if we run across someone not running away
			nPasserby = ChooseZoneCombatantRand_NoFlee( this.cZone.naCombatantLists[_cTeam.iId ^ 1] );
			if( nPasserby == null )
				nPasserby = ChooseZoneCombatantRand_NearbyZones( this.cZone, _cTeam.iId ^ 1 );

			if( nPasserby != null && nPasserby.cAiData.eState != EBattleAiStates.Flee )
			{
				// if we have an opponent at this point we need to charge them
				cActionManager.AddAction(new Action(ChargeOpponent, null, null));
				nTarget = nPasserby;
				cAction.bConditionNotMet = false;
				return false;
			}

			return true;
		}

		public bool ChargeOpponent(Action cAction, GameTime cTime)
		{
			Vector2		tDest,
						tDirVect;
			Combatant	nOpponent = (Combatant)this.nTarget,
						nPasserby;
			
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
				nTarget = null;
				cAction.bConditionNotMet = false;
				return false;
			}

			// check to see if while running at the opponent he has filled up his attack quota
			if(!nOpponent.bAvailablePos) { 
				cAiData.eState = EBattleAiStates.Ready;
				nTarget = null;
				cAction.bConditionNotMet = false;
				return false;
			}

			tDest = nOpponent.RequestAttackPoint(this, out _iAttackingPos);
			tDirVect = tDest - _tCenter;
			bDir = tDirVect.X > 0 + float.Epsilon;
			tDirVect.Normalize();

			// move the sprite by the speed of run (this data should come from the template)
		////ddhj Template add for speed of run
			if(( tDest - _tCenter ).LengthSquared( ) < 60000 )
				this.tPos += tDirVect * _fRunMovementSpeed;
			else	this.tPos += tDirVect * _fWalkMovementSpeed;
			
			// we are within weapon range so switch our system to attack 
			if( InWeaponRange( false )) { 
//				this.tPos = tDest;
				nOpponent.SetAttacker(this, out _iAttackingPos);
				_bAttacking = true;

				// call the attack heuristic because we are within attack range for our weapon 
				//// ddhj this will need a tweek for weapon range 
				cAiData.cHeurisitics[EBattleHeuristics.Attack](DataStore.cInstance.cBattleData);

				cAction.bConditionNotMet = false;
				return false;
			}

			if( nOpponent.cAiData.eState == EBattleAiStates.Flee )
			{
				// okay we're chasing a guy, but lets look around and see if we run across someone not running away
				nPasserby = ChooseZoneCombatantRand_NoFlee( this.cZone.naCombatantLists[_cTeam.iId ^ 1] );
				if( nPasserby == null )
					nPasserby = ChooseZoneCombatantRand_NearbyZones( this.cZone, _cTeam.iId ^ 1 );

				if( nPasserby != null && nPasserby.cAiData.eState != EBattleAiStates.Flee )
				{
					// if we have an opponent at this point we need to charge them
					cActionManager.AddAction(new Action(ChargeOpponent, null, null));
					nTarget = nPasserby;
					cAction.bConditionNotMet = false;
					return false;
				}
			}
			return true;
		}

		public bool Wait(Action cAction, GameTime cTime)
		{
			// check to see if the canvas (time) is longer than the data in (time) 
			TimeSpan	tWaitTime = (TimeSpan)cAction.oCanvas;

			if(cAction.bInit) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Battle", "stance", -1);
				cAction.bInit = false;
			}

			cAction.oCanvas = tWaitTime += cTime.ElapsedGameTime;
			if(tWaitTime > TimeSpan.FromMilliseconds((int)cAction.oData)) { 
				cAction.bConditionNotMet = false;
				return false;
			}
			return true;
		}
	}
}
