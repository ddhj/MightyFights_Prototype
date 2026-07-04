// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMercury;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 consolidation -- the shared battle machinery (state machine, drop economy,
	//// slow-mo, cursor, music, common draw frame, teardown) moved to BattleSceneBase; this
	//// scene keeps the classic skirmish mode: symmetric armies from both stewards, the
	//// left-click rematch balance loop, and the dev HUD.
	public partial class BattleGround_Basic : BattleSceneBase
	{
		Dictionary<string, List<IDrawable>>		_cDrawList = new Dictionary<string,List<IDrawable>>();
		Dictionary<string, List<Combatant>>	_cTrooperRef = new Dictionary<string,List<Combatant>>();
		List<TrooperTemplate>	_cTmpList = new List<TrooperTemplate>();

		////ddhj: debug data
		TimeSpan		_tTime = TimeSpan.Zero,
						_tVictoryElapsed = TimeSpan.Zero,
						_tEllapsedTime,
						_tOneSecond = TimeSpan.FromSeconds( 1 );

		int				_iFrameRate = 0,
						_iFrameCtr = 0;

		protected override void OnBattleOver(int iLosingTeamId)
		{
			_cBgm = DataManager.cInstance.CreateMusic( "Minibossies_FinalFantasy6_VictoryFanfare" );
			if(DataStore.cInstance.bPlayMusic) MediaPlayer.Play( _cBgm );

			// debug for exp (the tally itself already ran in the base)
			WriteExpData();
		}

		protected override void ProcessVictoryState()
		{
			// left click is restart battle
			if(Mouse.GetState().LeftButton == ButtonState.Pressed)
				ResetBattle();
			else if(Mouse.GetState().RightButton == ButtonState.Pressed)
				BackToMenu();
			else if(Mouse.GetState().MiddleButton == ButtonState.Pressed) { }
				// PORT (T1.5): StatsDialog.ShowDialog() removed with the dialog -- see PORT_NOTES.md

			//_tVictoryElapsed += tTime.ElapsedGameTime;
			//if(_tVictoryElapsed > TimeSpan.FromMilliseconds(2000)) {
			//    ResetBattle();
			//}
		}

		protected override void UpdateDebug(GameTime cTime)
		{
			// this is for the debug
			_tTime += cTime.ElapsedGameTime;
			if(_tTime > _tOneSecond) {
				_tTime -= _tOneSecond;
				_iFrameRate = _iFrameCtr;
				_iFrameCtr = 0;
			}

			if( _cBattleData.eState == EBattlegroundState.Battle )
				_tEllapsedTime += cTime.ElapsedGameTime;
		}

		protected override void DrawHud(SpriteBatch cBatch)
		{
			++_iFrameCtr;

			//// development interface
			{
					int		iLHp = 0,
							iRHp = 0,
							iLPow = 0,
							iRPow = 0,
							iLAc = 0,
							iRAc = 0;

					foreach( Team cTeam in _cBattleData.caTeams )
						foreach( Trooper cTrooper in cTeam.cActiveList.Values )
							if( cTrooper.cTeam.iId == 0 )
							{
								iLHp += (int)cTrooper.cStats.fHp;
								iLPow += cTrooper.cStats.iPower;
								iLAc += cTrooper.cStats.iArmorClass;
							}
							else	{
								iRHp += (int)cTrooper.cStats.fHp;
								iRPow += cTrooper.cStats.iPower;
								iRAc += cTrooper.cStats.iArmorClass;
							}

					cBatch.DrawString(_cFont, string.Format("LeftArmy: {0}    Left HP: {1}    Left Pow: {2}    Left AC: {3}",
							_cBattleData.caTeams[0].cActiveList.Count, iLHp, iLPow, iLAc ), new Vector2(10, 10), Color.White);
					cBatch.DrawString(_cFont, string.Format("RightArmy: {0}  Right HP: {1}  Right Pow:{2}   Right AC: {3}",
							_cBattleData.caTeams[1].cActiveList.Count, iRHp, iRPow, iRAc ), new Vector2(10, 30), Color.White);
					cBatch.DrawString(_cFont, string.Format("fps: {0}", _iFrameRate ), new Vector2(820, 10), Color.White);
					cBatch.DrawString(_cFont, string.Format("Time: {0}", _tEllapsedTime.ToString( "c" )), new Vector2(860, 10), Color.White);
				}

		}


		void SetBattleStart()
		{
			int		iCount = 0,
					iX = 9, iY = 0;
			Trooper	cTrooper;

			foreach( Combatant nCombatant in _cBattleData.caTeams[0].cActiveList.Values )
			{
				cTrooper = (Trooper)nCombatant;

				// set an initial script for the trooper
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(147 + iX * 17, iY * 18 + 100), null));

				// set the trooper for battle
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				
				// increment our grid counters
				++iY;
				if(iY > 19) { 
					iY = 0;
					--iX;

					if(( iX & 1 ) == 1 )
						iX -= 2;
				}
			}

			// reset out grid counters
			iX = iY = 0;
			iCount = 0;

			foreach( Combatant nCombatant in _cBattleData.caTeams[1].cActiveList.Values )
			{
				cTrooper = (Trooper)nCombatant;

				// set an initial script for the trooper
				cTrooper.cActionManager.AddAction(new Action(cTrooper.Wait, iCount += 50, TimeSpan.Zero));
				cTrooper.cActionManager.AddAction(new Action(cTrooper.MoveToPoint, new Vector2(717 + iX * 17, iY * 18 + 100), null));

				// set the trooper for battle
				cTrooper.cActionManager.AddPermAction(new Action(cTrooper.BasicBattleManager, _cBattleData, null));
				
				// increment our grid counters
				++iY;
				if(iY > 19) { 
					iY = 0;
					if(( iX & 1 ) == 1 )
						iX += 2;
					++iX;
				}
			}
		}

		
		void InitTemplateList(List<TemplateCfgMaster> cList)
		{
			_cTmpList.Clear();
			foreach(TemplateCfgMaster cTmplateCfg in cList)
				for(int i = 0; i < cTmplateCfg.iCount; ++i)
					_cTmpList.Add(DataManager.cInstance.CreateTemplate(cTmplateCfg));
		}

		bool GetTrooperTemplate(out TrooperTemplate cTemplate)
		{
			cTemplate = null;
			if(_cTmpList.Count == 0)
				return false;

			int iIndex = _cRand.Next(_cTmpList.Count);
			cTemplate = _cTmpList[iIndex];
			_cTmpList.RemoveAt(iIndex);
			return true;
		}

		protected override void SetupBattle()
		{
			DataStore		cData = DataStore.cInstance;
			GraphicsDevice	cGraphics = cData.cGraphics;
			DataManager	cObjMgr = DataManager.cInstance;
			Trooper			cTmpTrooper = null;
			Team			cTeam;
			Priest			cHealer;
			TrooperTemplate cTemplate;

			_tEllapsedTime = TimeSpan.Zero;

			cTeam = _cBattleData.caTeams[0];
			// make a block of troopers
			InitTemplateList(cData.cLSteward.cTemplates);
			while(GetTrooperTemplate(out cTemplate)) {
				// add the newly created trooper to the active list and set some initial battle data
				cTmpTrooper = new Trooper(cObjMgr.iCurObjId, cTeam, cTemplate);
				cTeam.cActiveList.Add(cTmpTrooper.iId, cTmpTrooper);
				// add to the member list just for debug maybe
				cTeam.cMembers.Add(cTmpTrooper);
				cTmpTrooper.tPos = new Vector2(25, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);

				// add the new object to the object manager
				_cObjMgr.AddObject(cTmpTrooper);
			}

			for( int iCount = 0; iCount < 2; ++iCount )
			{
				cHealer = cObjMgr.CreatePriest(new Vector2( 65, 180 + 120 * iCount ), cTeam,   (int)( 40f * 7 * 6 ), 7, 40f, 1.3333f, _cBattleData);
				cTeam.cHealerList.Add( cHealer.iId, cHealer );
				_cObjMgr.AddObject(cHealer);
			}

			cTeam = _cBattleData.caTeams[1];
			// make a block of opponents
			InitTemplateList(cData.cRSteward.cTemplates);
			while(GetTrooperTemplate(out cTemplate)) {
				// set the opponents to the acitve list
				cTmpTrooper = new Trooper(cObjMgr.iCurObjId, cTeam, cTemplate);
				cTeam.cActiveList.Add(cTmpTrooper.iId, cTmpTrooper);
				// add to the member list just for debug maybe
				cTeam.cMembers.Add(cTmpTrooper);
				cTmpTrooper.tPos = new Vector2(950, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHeight / 2);

				// add to the object manger
				_cObjMgr.AddObject(cTmpTrooper);
			}
			for( int iCount = 0; iCount < 2; ++iCount )
			{
				cHealer = cObjMgr.CreatePriest(new Vector2( 830, 180 + 120 * iCount ), cTeam,  (int)( 40f * 7 * 6 ), 7, 40f, 1.3333f, _cBattleData);
				cTeam.cHealerList.Add( cHealer.iId, cHealer );
				_cObjMgr.AddObject(cHealer);
			}

			// set the start of battle
			SetBattleStart();
		}


		void PartialClean()
		{
			CleanCommon();
			_cTrooperRef.Clear();
			// PORT (T1.5): dialog teardown removed along with the dialogs -- see PORT_NOTES.md
		}

		void ResetBattle()
		{
			MediaPlayer.Stop( );
			PartialClean();
			Init();
			_cBattleData.eState = EBattlegroundState.Init;
			_tVictoryElapsed = TimeSpan.Zero;
		}

		void CleanData()
		{
			MediaPlayer.Stop( );
			CleanCommon();
			_cDrawList.Clear();
			_cTrooperRef.Clear();
		}

		public override void Unload()
		{
			CleanData();
		}

	}
}
