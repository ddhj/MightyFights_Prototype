using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class PriestActMgr : ActionManager<Priest>
	{
		AnimationProcessor	_cAnimProc;
		KeyFrame			_cKeyFrame;

		public PriestActMgr(AnimationProcessor cAnimProc)
		{
			_cAnimProc = cAnimProc;
		}

		void ProcessKeyFrame()
		{
			switch(_cKeyFrame.Type) { 
				case "Heal": { 
					this.cData.Heal();
				} break;
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
			if((_cKeyFrame = _cAnimProc.Process(cTime)) != null) 
				// there was a keyframe on the animation, process it
				ProcessKeyFrame();
		}
	}
}
