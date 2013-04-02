using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	interface IBattleAi
	{
		EBattleAiStates	eState { get; set; }
		Stats			cStats { get; set; }

		void IdleHuristic();
		void PersueHuristic();
		void AttackHuristic();
		void MoveHuristic();

		void Init(TemplateConfig cConfig);
	}
}
