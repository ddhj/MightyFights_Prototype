// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 Village mode (docs/food.txt). One livestock-defence battle, run on the shared
	//// BattleSceneBase machinery exactly like WarClash / KaijuHunt. The village layer (Village)
	//// pushes this with a plan when the player chooses to DEFEND a predation raid; it spawns the
	//// muster (team 0) against the predators (team 1), the base state machine runs it to a wipe, and
	//// OnBattleOver records the outcome into the shared result before teardown. After a short beat it
	//// pops itself and the village layer reads the result. Per food.txt, putting up a fight protects
	//// food production regardless of who wins -- that verdict lives in Village; this scene only
	//// reports who won and how many defenders fell.
	public class VillageRaid : BattleSceneBase
	{
		VillageRaidPlan		_cPlan;
		VillageRaidResult	_cResult;
		TimeSpan			_tVictory = TimeSpan.Zero;
		static readonly TimeSpan	_tVictoryHold = TimeSpan.FromMilliseconds(1600);

		// formation layout, inside the field bounds (BattleData.SetZone refuses to zone units left of
		// x=112 or above y=70 -- they become untargetable). Same clamps WarClash uses.
		const int	iRowsPerCol = 10,
					iRowSpacing = 30,
					iColSpacing = 26,
					iDefenderCap = 40,
					iRaiderCap = 24;

		// balance. Base troopers barely scratch each other, so scale power up / HP down for a brisk
		// fight (as WarClash does). The raiders are few but nasty -- giants/trolls -- so they hit and
		// soak far harder per body. These mutate the per-trooper Stats copy CreateTemplate hands out
		// (DataManager.cs) -- never the persisted steward template.
		const float	fPowerScale		= 3.0f,
					fHpScale		= 0.5f,
					fRaiderPower	= 3.5f,		// a raider hits this much harder again
					fRaiderHp		= 6.0f;		// ...and soaks this much more

		public VillageRaid(VillageRaidPlan cPlan, VillageRaidResult cResult)
		{
			_cPlan = cPlan;
			_cResult = cResult;
		}

	// BattleSceneBase hooks

		protected override void SetupBattle()
		{
			_tVictory = TimeSpan.Zero;

			// team 0 = the village muster (masses left, faces right); team 1 = the predators (right).
			SpawnFormation(_cBattleData.caTeams[0], _cPlan.cDefenderCfg, Math.Min(_cPlan.iDefenders, iDefenderCap), true,  false);
			SpawnFormation(_cBattleData.caTeams[1], _cPlan.cRaiderCfg,   Math.Min(_cPlan.iRaiders,   iRaiderCap),   false, true);

			// a couple of healers per side keeps the flee/heal AI on the proven BattleGround_Basic
			// path (same reasoning as WarClash). Healers live in cHealerList, never affect the wipe.
			DataManager	cMgr = DataManager.cInstance;
			for(int iCount = 0; iCount < 2; ++iCount) {
				Priest	cHealer = cMgr.CreatePriest(new Vector2(65, 180 + 120 * iCount), _cBattleData.caTeams[0], (int)(40f * 7 * 6), 7, 40f, 1.3333f, _cBattleData);
				_cBattleData.caTeams[0].cHealerList.Add(cHealer.iId, cHealer);
				_cObjMgr.AddObject(cHealer);

				cHealer = cMgr.CreatePriest(new Vector2(830, 180 + 120 * iCount), _cBattleData.caTeams[1], (int)(40f * 7 * 6), 7, 40f, 1.3333f, _cBattleData);
				_cBattleData.caTeams[1].cHealerList.Add(cHealer.iId, cHealer);
				_cObjMgr.AddObject(cHealer);
			}
		}

		protected override void OnBattleOver(int iLosingTeamId)
		{
			// the wiped team lost. Casualties = defenders who did NOT survive (their whole muster if
			// the village was wiped). Read here -- Unload/CleanCommon clears the teams right after.
			int	iWinnerSide = iLosingTeamId == 0 ? 1 : 0;
			int	iDefendersLeft = _cBattleData.caTeams[0].cActiveList.Count;

			_cResult.iWinnerSide = iWinnerSide;
			_cResult.iDefendersLost = Math.Max(0, Math.Min(_cPlan.iDefenders, iDefenderCap) - iDefendersLeft);
			_cResult.bComplete = true;

			if(iWinnerSide == 0) {
				_cBgm = DataManager.cInstance.CreateMusic("Minibossies_FinalFantasy6_VictoryFanfare");
				if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play(_cBgm);
			}
		}

		protected override void ProcessVictoryState()
		{
			// auto-withdraw back to the village after a brief hold (Village polls the shared result
			// once it regains the top of the stack -- no input needed).
			_tVictory += DataStore.cInstance.tTime.ElapsedGameTime;
			if(_tVictory >= _tVictoryHold)
				BackToMenu();
		}

		protected override void DrawHud(SpriteBatch cBatch)
		{
			cBatch.DrawString(_cFont, string.Format("Muster   {0}", _cBattleData.caTeams[0].cActiveList.Count),
				new Vector2(10, 10), Color.CornflowerBlue);
			cBatch.DrawString(_cFont, string.Format("{0}   {1}", _cPlan.sRaider, _cBattleData.caTeams[1].cActiveList.Count),
				new Vector2(10, 30), Color.IndianRed);
			cBatch.DrawString(_cFont, "Defending: " + _cPlan.sVocation, new Vector2(10, 50), Color.Gold);

			if(_cBattleData.eState == EBattlegroundState.Victory)
				cBatch.DrawString(_cFont, _cResult.iWinnerSide == 0 ? "RAID REPELLED -- returning to the village..."
					: "MUSTER OVERRUN -- returning to the village...",
					new Vector2(300, 40), _cResult.iWinnerSide == 0 ? Color.Gold : Color.OrangeRed);
		}

	// internals

		void SpawnFormation(Team cTeam, TemplateCfgMaster cCfg, int iCount, bool bLeftSide, bool bRaider)
		{
			DataManager	cMgr = DataManager.cInstance;

			for(int iUnit = 0; iUnit < iCount; ++iUnit) {
				int	iRow = iUnit % iRowsPerCol,
					iCol = iUnit / iRowsPerCol;

				int	iX = bLeftSide ? 150 + iCol * iColSpacing : 800 - iCol * iColSpacing,
					iY = 110 + iRow * iRowSpacing;

				// clamp inside the field so every unit is zoned/targetable (see WarClash's note on
				// tCenter drifting the right formation toward the x=912 field edge).
				iX = Math.Min(Math.Max(iX, 120), 860);
				iY = Math.Min(Math.Max(iY, 80), 500);

				Trooper	cTrooper = new Trooper(cMgr.iCurObjId, cTeam, cMgr.CreateTemplate(cCfg));

				// cStats is this trooper's own copy (CreateTemplate deep-copies), so scaling it touches
				// nothing persisted and nothing shared with the other troopers.
				Stats	cStats = cTrooper.cStats;
				cStats.iPower = Math.Max(1, (int)(cStats.iPower * fPowerScale * (bRaider ? fRaiderPower : 1f)));
				cStats.iMaxHp = Math.Max(1, (int)(cStats.iMaxHp * fHpScale * (bRaider ? fRaiderHp : 1f)));
				cStats.fHp = cStats.iMaxHp;

				cTeam.cActiveList.Add(cTrooper.iId, cTrooper);
				cTeam.cMembers.Add(cTrooper);

				// bDir before tPos: tPos's setter runs UpdateRefPoints (see KaijuHunt.SpawnTrooper)
				cTrooper.bDir = bLeftSide;
				cTrooper.tPos = new Vector2(iX, iY);
				_cObjMgr.AddObject(cTrooper);

				// spawn already charging, no scripted march (same call KaijuHunt / WarClash make)
				cTrooper.cAiData.eState = EBattleAiStates.Ready;
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
			}
		}
	}
}
