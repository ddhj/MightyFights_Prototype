// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 War Battle mode (docs/WAR_BATTLE_DESIGN.md). Shared data model between the
	//// strategic layer (WarBattle) and the engine-backed clash (WarClash). Nothing here touches
	//// the shared combat engine -- these are mode-local types passed across the scene-stack push.

	//// the Suikoden I rock-paper-scissors stances, committed per clash. Charge beats Bow beats
	//// Magic beats Charge.
	public enum EWarStance
	{
		Charge,
		Bow,
		Magic
	}

	public static class WarRules
	{
		//// +1 if eA beats eB, -1 if eB beats eA, 0 on a tie. The cycle is Charge>Bow>Magic>Charge,
		//// i.e. each stance beats the one that follows it (with wraparound).
		public static int Beats(EWarStance eA, EWarStance eB)
		{
			if(eA == eB)
				return 0;

			// eA beats the stance one step ahead of it in the Charge->Bow->Magic->Charge cycle
			return ((int)eA + 1) % 3 == (int)eB ? 1 : -1;
		}

		public static string Name(EWarStance eStance)
		{
			switch(eStance) {
				case EWarStance.Charge:	return "Charge";
				case EWarStance.Bow:	return "Bow";
				default:				return "Magic";
			}
		}

		//// what this stance is strong against, for the tooltip line
		public static string Beats(EWarStance eStance)
		{
			switch(eStance) {
				case EWarStance.Charge:	return "Bow";
				case EWarStance.Bow:	return "Magic";
				default:				return "Charge";
			}
		}
	}

	//// a formation-block on the strategic grid. For v1 a division is homogeneous (one template)
	//// and its troop count is what gets spawned as real troopers when it clashes.
	public class Division
	{
		public int					iSide;		// 0 = player, 1 = enemy
		public int					iCol,
									iRow;		// strategic grid cell
		public int					iTroops;
		public EWarStance			eStance;	// this division's committed stance / affinity
		public string				sLabel;
		public TemplateCfgMaster	cCfg;		// the template its troopers are built from
		public Color				cColor;		// strategic-token tint
	}

	//// strategic -> tactical: the immutable inputs to one clash. The advantage from the stance
	//// matchup is already baked into the spawn counts (WarBattle computes them), so WarClash just
	//// spawns iPlayerSpawn / iEnemySpawn troopers of the respective templates.
	public class WarClashPlan
	{
		public TemplateCfgMaster	cPlayerCfg,
									cEnemyCfg;
		public int					iPlayerSpawn,
									iEnemySpawn;
		public string				sPlayerLabel,
									sEnemyLabel;
		public EWarStance			ePlayerStance,
									eEnemyStance;
		public int					iAdvantageSide;	// 0 player, 1 enemy, -1 none
	}

	//// tactical -> strategic: written by WarClash.OnBattleOver before teardown, polled by
	//// WarBattle once control returns up the stack.
	public class WarClashResult
	{
		public bool	bComplete;
		public int	iWinnerSide;	// 0 = player, 1 = enemy
		public int	iSurvivors;		// how many of the winner's troopers lived
	}
}
