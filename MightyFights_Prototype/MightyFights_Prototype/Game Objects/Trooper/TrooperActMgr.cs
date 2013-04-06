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
		EBattleAiStates		_eState;
		Trooper				_cTrooper;
		IOpponent			_nOpponent;
		AnimationProcessor	_cAnimProc;
		Vector2				_tDir;
		KeyFrame			_cKeyFrame;

		public TrooperActMgr(AnimationProcessor cAnimProc)
		{
			_cAnimProc = cAnimProc;
			_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
		}

		void ProcessKeyFrame()
		{
		}

		public override void Process(GameTime cTime)
		{
			List<Action>	cTmpActionList = new List<Action>();
			Action			cCurAction; 

			foreach(Action cAction in _cPerminantActions)
				cTmpActionList.Add(cAction);

			// do the perminant actions (they are removable but the conditions are much longer term
			foreach(Action cAction in cTmpActionList) {
				if(cAction.bConditionNotMet)
					cAction.dHuristic(cAction);
				else _cPerminantActions.Remove(cAction);
			}

			// do the last item in the action list 
			cCurAction = _cActionQueue[0];
			if(cCurAction.bConditionNotMet)
				cCurAction.dHuristic(cCurAction);
			else _cActionQueue.RemoveAt(0);

			// handle the animation 
			//// ddhj: not sure I want this to be here ... but it is specific to the object type so it might be a good place for it ... 
			if((_cKeyFrame = _cAnimProc.Process(cTime)) != null) 
				ProcessKeyFrame();
		}
	}
}
