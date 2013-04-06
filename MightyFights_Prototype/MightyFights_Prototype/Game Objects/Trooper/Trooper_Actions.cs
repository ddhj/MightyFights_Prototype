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

			return true;
		}

		public bool MoveToPoint(Action cAction)
		{
			Vector2		tDest = (Vector2)cAction.oData,
						tDirVect = (Vector2)cAction.oCanvas;
			
			// check to see if we need to make the direction vector or not
			if(cAction.bInit) { 
				// make the direction vector and set the direction for the sprite
				tDirVect = _tPos - tDest;
				bDir = tDirVect.X > 0;
				tDirVect.Normalize();
				cAction.oCanvas = tDirVect;
				_cAnimProc.SetAnimationCriteria("Move", "Walk", "walk", -1);

				cAction.bInit = false;
			}

			// move the sprite by the speed of walk (this data should come from the template)
			////ddhj Template add for speed of walk
			_tPos += tDirVect * 1.5f;

			if(_tPos == tDest) { 
				_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);

				return false;
			}

			return true;
		}

		public bool ChargeOpponent(Action cAction)
		{

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
			if(tTime > TimeSpan.FromMilliseconds((double)(int)cAction.oData))
				return false;
			return true;
		}
	}
}
