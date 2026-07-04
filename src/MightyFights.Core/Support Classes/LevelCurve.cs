using System;

namespace MightyFights_Prototype
{
	//// ddhj: 2026, step 3 of the kaiju build order -- the exp -> level formula that never got
	//// written in 2012 (the WinForms StatsDialog existed to tune it). Owner picked a Fibonacci
	//// curve, the classic early-RPG progression: the XP needed for each successive level grows
	//// 1,1,2,3,5,8,... * iXpBase, applied cumulatively (L1 at 100, L2 at 200, L3 at 400, L4 at
	//// 700, L5 at 1200, ...). Templates level, not instances -- see DESIGN_DIRECTION.md.
	public static class LevelCurve
	{
		const int	iXpBase = 100;
		const int	iMaxLevel = 20;

		/// <summary> the scalar XP value of a banked experience record; kills dominate, with
		/// support credit for crits, landed/defended attacks, battles survived, and healing </summary>
		public static int GetXp(ExperienceData cExp)
		{
			if(cExp == null)
				return 0;

			return cExp.iKills * 10 + cExp.iCritSuccess * 3 + cExp.iAttackSuccess * 2
				+ cExp.iAttackDefended + cExp.iBattles * 5 + cExp.iHealed;
		}

		public static int GetLevel(ExperienceData cExp)
		{
			int		iXp = GetXp(cExp),
					iFibA = 1, iFibB = 1,
					iThreshold = 0,
					iLevel = 0;

			while(iLevel < iMaxLevel) {
				iThreshold += iFibA * iXpBase;
				if(iXp < iThreshold)
					break;

				++iLevel;
				int iNext = iFibA + iFibB;
				iFibA = iFibB;
				iFibB = iNext;
			}

			return iLevel;
		}

		/// <summary> v1 growth applied to a unit's stats at spawn: +6% power, +8% hp, +4% armor
		/// class per level, compounding -- the fibonacci gate keeps high levels rare enough that
		/// compounding stays sane. Tune here, one place. </summary>
		public static void ApplyLevel(Stats cStats, int iLevel)
		{
			if(iLevel <= 0)
				return;

			cStats.iPower = (int)(cStats.iPower * Math.Pow(1.06, iLevel));
			cStats.iMaxHp = (int)(cStats.iMaxHp * Math.Pow(1.08, iLevel));
			cStats.fHp = cStats.iMaxHp;
			cStats.iArmorClass = (int)(cStats.iArmorClass * Math.Pow(1.04, iLevel));
		}

		//// ddhj: 2026 -- the Template editor's shared point-spend economy (owner call, see
		//// docs/DESIGN_DIRECTION.md). Every banked level is one point; a point buys either one
		//// slider notch bump (top or bottom) or one ability-card unlock, player's choice.
		/// <summary> points already committed via the Template editor: one per slider notch
		/// (iTopLevel/iBottomLevel each count directly since they start at 0) plus one per
		/// unlocked ability card </summary>
		public static int GetSpentPoints(TemplateConfig cCfg)
		{
			int	iAbilities = 0;

			if(cCfg.baAbilitiesOn != null)
				foreach(bool bOn in cCfg.baAbilitiesOn)
					if(bOn)
						++iAbilities;

			return cCfg.iTopLevel + cCfg.iBottomLevel + iAbilities;
		}

		/// <summary> points earned (GetLevel) minus points already spent, floored at 0 -- a
		/// template whose preset stats already imply "spend" ahead of its banked exp (e.g. the
		/// Captain preset) just shows 0 available rather than going negative </summary>
		public static int GetAvailablePoints(TemplateCfgMaster cBank)
		{
			return Math.Max(0, GetLevel(cBank.cExpData) - GetSpentPoints(cBank));
		}
	}
}
