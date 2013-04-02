using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class TrooperActMgr : ActionManager, IBattleAi
	{
		EBattleAiStates		_eState;
		Trooper				_cTrooper;
		IOpponent			_nOpponent;
		AnimationProcessor	_cAnimProc;
		Stats				_cStats;
		Vector2				_tDir;
		KeyFrame			_cKeyFrame;

		public TrooperActMgr(object oData) : base(oData)
		{
			_cTrooper = (Trooper)oData;
			_cAnimProc = _cTrooper.cAnimationProcessor;
			_eState = EBattleAiStates.Moving;

			// process the initial state
			_cAnimProc.SetAnimationCriteria("Idle", "Normal", "transition", -1);
		}

		void ProcessKeyFrame()
		{

		}

		public override void Process(GameTime cTime)
		{
			switch(_eState) {
 				case EBattleAiStates.Start:
					// this is for the start of battle (formation and all that)
				break;
				case EBattleAiStates.Idle:
					IdleHuristic();
				break;
				case EBattleAiStates.Dead:
					// do something to signal the system to remove
				break;
				case EBattleAiStates.Dying:
					if(!_cAnimProc.bActive)
						_eState = EBattleAiStates.Dead;
				break;
				case EBattleAiStates.Moving:
					MoveHuristic();
				break;
				case EBattleAiStates.Pursuit:
					PersueHuristic();
				break;
				case EBattleAiStates.Attacking:
					AttackHuristic();
				break;
				default:
				break;
			}

			if((_cKeyFrame = _cAnimProc.Process(cTime)) != null) 
				ProcessKeyFrame();
		}

		#region IBattleAi Members

		public EBattleAiStates eState	{ get { return _eState; } set { _eState = value; }}
		public Stats cStats		{ get { return _cStats; } set { _cStats = value; }}

		public void IdleHuristic()
		{
			// check to see if we have an opponent
			if(_nOpponent == null) { 
				
			}
		}

		public void PersueHuristic()
		{
		}

		public void AttackHuristic()
		{
		}

		public void MoveHuristic()
		{
		}

		public void Init(TemplateConfig cConfig)
		{
		}

		#endregion
	}
}
