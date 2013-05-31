using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	[Flags]
	public enum EObjectStates
	{
		Draw	= 0x01,
		Active	= 0x02
	}

	public enum EZoneData
	{
		ZoneColumns = 8,
		ZoneRows = 6,
		ZoneColWidth = 100,
		ZoneRowHeight = 72
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

	public enum EBattleHeuristics
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
		HalberdHeight = 64,
		HalberdWidth = 100,
		BuffContainerHeight = 44,
		BuffContainerWidth = 60
	}

	public enum EBuffEffects
	{
		Dragon_Wing,
		Lion_Paw, 
		Eagle_Feather,
		Snake_Fang, 
		Toad_Eye, 
		Wolf_Ear, 
		Crab_Claw,
		Squirrel_Acorn,
		MaxBuffs
	}

	public enum EStatusEffects
	{
		Poision,
		Stun,
		Berzerk
	}

	public enum EHealerStates
	{
		Idle, 
		Healing,
		Recharging
	}
}
