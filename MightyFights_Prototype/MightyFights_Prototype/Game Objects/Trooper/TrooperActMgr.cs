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
			_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
		}

		void ProcessKeyFrame()
		{
			ICombatant	nOpponent = (ICombatant)cData.nTarget;

			switch(_cKeyFrame.Type) { 
				case "Collision":
					if( cData.InWeaponRange( ))
						if( _cAnimProc.sSubType == "Critical" )
							nOpponent.DealDamage( cData, cData.cStats.iPower * 3, true );
						else	nOpponent.DealDamage( cData, cData.cStats.iPower, false );
				break;
			}
		}

		public override void Process(GameTime cTime)
		{
			List<Action>	cTmpActionList = new List<Action>();
			Action			cCurAction; 

			foreach(Action cAction in cPerminantActions)
				cTmpActionList.Add(cAction);

			// do the perminant actions (they are removable but the conditions are much longer term
			foreach(Action cAction in cTmpActionList) {
				if(cAction.bConditionNotMet)
					cAction.dHeuristic(cAction, cTime);
				else cPerminantActions.Remove(cAction);
			}

			// do the last item in the action list 
			if(cActionQueue.Count > 0) { 
				cCurAction = cActionQueue[0];
				if(cCurAction.bConditionNotMet)
					cCurAction.dHeuristic(cCurAction, cTime);
				else cActionQueue.RemoveAt(0);
			}
			
			// handle the animation 
			//// ddhj: not sure I want this to be here ... but it is specific to the object type so it might be a good place for it ... 
			if((_cKeyFrame = _cAnimProc.Process(cTime)) != null) 
				ProcessKeyFrame();
		}
	}
}
