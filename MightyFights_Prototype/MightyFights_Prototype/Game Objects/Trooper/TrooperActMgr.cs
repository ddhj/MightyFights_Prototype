using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class TrooperActMgr : ActionManager<Trooper>
	{
		AnimationProcessor	_cAnimProc;
		KeyFrame			_cKeyFrame;

		public TrooperActMgr(AnimationProcessor cAnimProc)
		{
			_cAnimProc = cAnimProc;
			_cAnimProc.SetAnimationCriteria("Idle", "Normal", "idle", -1);
		}

		void ProcessKeyFrame()
		{
			switch(_cKeyFrame.Type) { 
				case "Collision": { 
					ICombatant	nOpponent = (ICombatant)this.cData.nTarget;

					if( !nOpponent.IsDead( ))
					{
						if( this.cData.InWeaponRange( true )) { 
							++this.cData.cExpData.iAttacks;

							if( _cAnimProc.sSubType == "Critical" )
								nOpponent.DealDamage( cData, cData.cStats.iPower * 3, true );
							else	nOpponent.DealDamage( cData, cData.cStats.iPower, false );
						}
					}
				} break;

				case "SelfHeal": {
					if( this.cData.nTarget == null || !( this.cData.nTarget is IHealer )) {
						this.cData.Heal(Convert.ToSingle(_cKeyFrame.oData));
					}
				} break; 
			}
		}

		public override void Process(GameTime cTime)
		{
			List<Action>	cTmpActionList = new List<Action>();
			Action			cCurAction; 

			cTmpActionList.AddRange( cPerminantActions );

			// do the perminant actions (they are removable but the conditions are much longer term
			foreach(Action cAction in cTmpActionList) {
				if(cAction.bConditionNotMet)
					cAction.dHeuristic(cAction, cTime);

				if(!cAction.bConditionNotMet)
					cPerminantActions.Remove(cAction);
			}

			// do the last item in the action list 
			if(cActionQueue.Count > 0) { 
				cCurAction = cActionQueue[0];
				if(cCurAction.bConditionNotMet)
					cCurAction.dHeuristic(cCurAction, cTime);

				if(!cCurAction.bConditionNotMet)
					cActionQueue.RemoveAt(0);
			}
			
			// handle the animation 
			//// ddhj: not sure I want this to be here ... but it is specific to the object type so it might be a good place for it ... 
			 if((_cKeyFrame = _cAnimProc.Process(cTime)) != null) 
				ProcessKeyFrame();
		}
	}
}
