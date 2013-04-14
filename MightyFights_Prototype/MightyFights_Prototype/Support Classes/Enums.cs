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
		Start,
		Attacking, 
		Dying, 
		Idle, 
		Moving,
		Dead,
		Pursuit,
		Panting,
		Defending,
		Ready
	}

	public enum EBattleHuristics
	{
		Idle,
		Attack, 
		Flee,
		Persue,
		Pant,
		ChooseOpponent
	}

	public enum EBattlegroundState
	{
		Init,
		Battle, 
		Victory
	}

	public enum EConstants
	{
		HalberdHight = 64,
		HalberdWidth = 100
	}
}
