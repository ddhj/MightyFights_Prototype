using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class TrooperActMgr : ActionManager, IBattleAi
	{
		EBattleAiStates		_eState;
		Trooper				_cTrooper;

		public TrooperActMgr(object oData) : base(oData)
		{

		}

		public override void Process()
		{

		}

		#region IBattleAi Members

		public EBattleAiStates eState	{ get { return _eState; } set { _eState = value; }}

		public void IdleHuristic()
		{
			throw new NotImplementedException();
		}

		public void PersueHuristic()
		{
			throw new NotImplementedException();
		}

		public void AttackHuristic()
		{
			throw new NotImplementedException();
		}

		public void MoveHuristic()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
