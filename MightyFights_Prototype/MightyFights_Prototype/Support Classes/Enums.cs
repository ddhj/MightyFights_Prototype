using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public enum EZoneData
	{
		ZoneColumns = 8,
		ZoneRows = 6,
		ZoneColWidth = 114,
		ZoneRowHeight = 114
	}

	[Flags]
	public enum ETrooperAttackPos
	{
		RightMid = 0x01,
		RightTop = 0x02,
		RightBottom = 0x04,
		LeftMid = 0x08,
		LeftTop = 0x10,
		LeftBottom = 0x20
	}

	public enum EMainWindowSize
	{
		WindowWidth = 1024,
		WindowHeight = 768,
		BorderOffsetX = 112, 
		BorderOffsetY = 84
	}

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
		Ready,
		Flee
	}

	public enum EBattleheuristics
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
