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
	//// ddhj: 2026 War Battle mode (docs/WAR_BATTLE_DESIGN.md). One division-vs-division field
	//// battle, run on the shared BattleSceneBase machinery exactly like KaijuHunt / BattleGround_
	//// Basic. The strategic layer (WarBattle) pushes this scene with a plan, it spawns both
	//// formations as real troopers, the base state machine runs it to a wipe, and OnBattleOver
	//// records the survivors into the shared result before teardown. After a short victory beat it
	//// pops itself and the strategic layer reads the result.
	public class WarClash : BattleSceneBase
	{
		WarClashPlan	_cPlan;
		WarClashResult	_cResult;
		TimeSpan		_tVictory = TimeSpan.Zero;
		static readonly TimeSpan	_tVictoryHold = TimeSpan.FromMilliseconds(1600);

		// formation layout (inside the field bounds -- see BattleData.SetZone: units left of x=112
		// or above y=70 never get zoned and become untargetable)
		const int	iRowsPerCol = 12,
					iRowSpacing = 28,
					iColSpacing = 26,
					iSpawnCap = 80;		// keep a lopsided clash from spawning a runaway swarm

		// clash balance. Base troopers deal only a couple damage per swing against 60-100 HP, so an
		// even clash grinds on far too long and the stance edge -- just +bodies -- barely reads. Scale
		// power up / HP down at spawn so clashes resolve briskly, and give the stance winner a real
		// damage edge on top of its extra bodies so the RPS choice visibly decides the fight. These
		// mutate the per-trooper Stats copy CreateTemplate hands out (DataManager.cs:107) -- never the
		// persisted steward template, so no save-state bloat.
		const float	fPowerScale		= 3.0f,		// every trooper hits this much harder
					fHpScale		= 0.5f,		// ...and is this much squishier
					fAdvantagePower	= 1.5f;		// the stance winner hits this much harder again

		public WarClash(WarClashPlan cPlan, WarClashResult cResult)
		{
			_cPlan = cPlan;
			_cResult = cResult;
		}

	// BattleSceneBase hooks

		protected override void SetupBattle()
		{
			_tVictory = TimeSpan.Zero;

			// team 0 = player division (masses on the left, faces right); team 1 = enemy (right).
			// The stance winner (iAdvantageSide) fields more troops AND hits harder (see SpawnFormation).
			SpawnFormation(_cBattleData.caTeams[0], _cPlan.cPlayerCfg, Math.Min(_cPlan.iPlayerSpawn, iSpawnCap), true,  _cPlan.iAdvantageSide == 0);
			SpawnFormation(_cBattleData.caTeams[1], _cPlan.cEnemyCfg,  Math.Min(_cPlan.iEnemySpawn,  iSpawnCap), false, _cPlan.iAdvantageSide == 1);

			// a couple of healers per side keeps the flee/heal AI on the proven BattleGround_Basic
			// path (a fleeing trooper with nowhere to heal can otherwise linger and stall the wipe).
			// Healers live in cHealerList, not cActiveList, so they never affect the victory check.
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
			// the wiped team lost; the other side's remaining troopers are the survivors. Read them
			// out here -- Unload/CleanCommon clears the teams right after this.
			int	iWinnerSide = iLosingTeamId == 0 ? 1 : 0;

			_cResult.iWinnerSide = iWinnerSide;
			_cResult.iSurvivors = _cBattleData.caTeams[iWinnerSide].cActiveList.Count;
			_cResult.bComplete = true;

			if(iWinnerSide == 0) {
				_cBgm = DataManager.cInstance.CreateMusic("Minibossies_FinalFantasy6_VictoryFanfare");
				if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play(_cBgm);
			}
		}

		protected override void ProcessVictoryState()
		{
			// auto-withdraw back to the strategic layer after a brief hold (no input needed --
			// WarBattle polls the shared result once it regains the top of the stack)
			_tVictory += DataStore.cInstance.tTime.ElapsedGameTime;
			if(_tVictory >= _tVictoryHold)
				BackToMenu();
		}

		protected override void DrawHud(SpriteBatch cBatch)
		{
			cBatch.DrawString(_cFont, string.Format("{0}  [{1}]   {2}", _cPlan.sPlayerLabel,
				WarRules.Name(_cPlan.ePlayerStance), _cBattleData.caTeams[0].cActiveList.Count),
				new Vector2(10, 10), Color.CornflowerBlue);
			cBatch.DrawString(_cFont, string.Format("{0}  [{1}]   {2}", _cPlan.sEnemyLabel,
				WarRules.Name(_cPlan.eEnemyStance), _cBattleData.caTeams[1].cActiveList.Count),
				new Vector2(10, 30), Color.IndianRed);

			if(_cPlan.iAdvantageSide == 0)
				cBatch.DrawString(_cFont, "Stance advantage: YOU", new Vector2(10, 50), Color.Gold);
			else if(_cPlan.iAdvantageSide == 1)
				cBatch.DrawString(_cFont, "Stance advantage: ENEMY", new Vector2(10, 50), Color.OrangeRed);

			if(_cBattleData.eState == EBattlegroundState.Victory)
				cBatch.DrawString(_cFont, _cResult.iWinnerSide == 0 ? "DIVISION VICTORIOUS -- returning to the field..."
					: "DIVISION ROUTED -- returning to the field...",
					new Vector2(330, 40), _cResult.iWinnerSide == 0 ? Color.Gold : Color.OrangeRed);
		}

	// internals

		void SpawnFormation(Team cTeam, TemplateCfgMaster cCfg, int iCount, bool bLeftSide, bool bAdvantaged)
		{
			DataManager	cMgr = DataManager.cInstance;

			for(int iUnit = 0; iUnit < iCount; ++iUnit) {
				int	iRow = iUnit % iRowsPerCol,
					iCol = iUnit / iRowsPerCol;

				int	iX = bLeftSide ? 150 + iCol * iColSpacing : 800 - iCol * iColSpacing,
					iY = 110 + iRow * iRowSpacing;

				// clamp inside the field so every unit is zoned/targetable. NB: SetZone keys off
				// tCenter, and UpdateRefPoints always shoves tCenter.X ~half-a-sprite to the RIGHT of
				// tPos regardless of facing (Trooper_Combatant.cs) -- so the right formation's centers
				// drift toward the x=912 field edge. A fresh spawn whose tCenter lands in column >= 8
				// (x >= 912) is silently left unzoned by SetZone and can never engage. Keep the right
				// front rank (base 800) and this max well inside 912 so every unit's *center* zones.
				iX = Math.Min(Math.Max(iX, 120), 860);
				iY = Math.Min(Math.Max(iY, 80), 500);

				Trooper	cTrooper = new Trooper(cMgr.iCurObjId, cTeam, cMgr.CreateTemplate(cCfg));

				// faster kills + the stance winner's damage edge. cStats is this trooper's own copy
				// (CreateTemplate deep-copies at DataManager.cs:107), so scaling it touches nothing
				// persisted and nothing shared with the other troopers.
				Stats	cStats = cTrooper.cStats;
				cStats.iPower = Math.Max(1, (int)(cStats.iPower * fPowerScale * (bAdvantaged ? fAdvantagePower : 1f)));
				cStats.iMaxHp = Math.Max(1, (int)(cStats.iMaxHp * fHpScale));
				cStats.fHp = cStats.iMaxHp;

				cTeam.cActiveList.Add(cTrooper.iId, cTrooper);
				cTeam.cMembers.Add(cTrooper);

				// bDir before tPos: tPos's setter runs UpdateRefPoints (see KaijuHunt.SpawnTrooper)
				cTrooper.bDir = bLeftSide;
				cTrooper.tPos = new Vector2(iX, iY);
				_cObjMgr.AddObject(cTrooper);

				// spawn already charging, no scripted march (same call KaijuHunt makes)
				cTrooper.cAiData.eState = EBattleAiStates.Ready;
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
			}
		}
	}
}
