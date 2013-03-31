using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public enum ESceneStates
	{
		TransitionIn, 
		Active, 
		TransitionOut, 
		Inactive
	}

	public enum EBattleAiStates
	{
		Attacking, 
		Dying, 
		Idle, 
		Moving,
		Dead,
		Pursuit
	}
}
