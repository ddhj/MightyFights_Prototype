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
		Pursuit
	}

	public enum EBattleInitState
	{
		Init,
		Wait, 
		Moving
	}

	public enum EConstants
	{
		HalberdHight = 64,
		HalberdWidth = 100
	}
}
