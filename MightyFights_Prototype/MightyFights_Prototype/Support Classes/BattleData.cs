using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public delegate void DBattleHuristic(BattlegroundData cData);

	public class BattlegroundData
	{	
		public List<ICombatant>		naOpponents		{get; set;}
		public List<ICombatant>		naArmy			{get; set;}

		public EBattlegroundState	eState			{get; set;}
	}

	public class AiBattleData
	{
		public Dictionary<EBattleHuristics, DBattleHuristic>	cHurisitics  {get; set;}
		public EBattleAiStates									eState		 {get; set;}
	}
}
