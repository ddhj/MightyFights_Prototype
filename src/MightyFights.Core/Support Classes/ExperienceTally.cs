using System;
using System.Collections.Generic;
using System.Linq;

namespace MightyFights_Prototype
{
	//// PORT (Phase 1): the per-kill / experience tally used to live inside the WinForms
	//// StatsDialog (StatsDialog.InitGridData). The dialog was display; this aggregation is
	//// working balance bookkeeping and is not WinForms-dependent -- lifted here verbatim so
	//// it survives the cross-platform port. The six per-template buckets were BindingSource
	//// grid rows in the dialog; here they are plain List<ExperienceData>. The two team totals
	//// are summed exactly as before via ExperienceData.operator+. See docs/PORT_NOTES.md.
	public class ExperienceTally
	{
	// Data -- per-template buckets (one ExperienceData per trooper, as the dialog grids held)
		List<ExperienceData>	_cLCapData = new List<ExperienceData>(),
								_cLT1Data = new List<ExperienceData>(),
								_cLT2Data = new List<ExperienceData>(),
								_cRCapData = new List<ExperienceData>(),
								_cRT1Data = new List<ExperienceData>(),
								_cRT2Data = new List<ExperienceData>();
	// team totals
		ExperienceData			_cLSumData = new ExperienceData(),
								_cRSumData = new ExperienceData();

	// Properties
		public List<ExperienceData> cLCapData	{ get { return _cLCapData; }}
		public List<ExperienceData> cLT1Data	{ get { return _cLT1Data; }}
		public List<ExperienceData> cLT2Data	{ get { return _cLT2Data; }}
		public List<ExperienceData> cRCapData	{ get { return _cRCapData; }}
		public List<ExperienceData> cRT1Data	{ get { return _cRT1Data; }}
		public List<ExperienceData> cRT2Data	{ get { return _cRT2Data; }}
		public ExperienceData cLSumData			{ get { return _cLSumData; }}
		public ExperienceData cRSumData			{ get { return _cRSumData; }}

	// functions
		public void Tally( BattlegroundData cBattleData )
		{
			ExperienceData	cLSumData = new ExperienceData(),
							cRSumData = new ExperienceData();

			Team			cTeam = cBattleData.caTeams[0];
			string			sTemplateName;
			Trooper			cTrooper;

			foreach(Combatant nCombatant in cTeam.cMembers) {
				cTrooper = (Trooper)nCombatant;
				sTemplateName = cTrooper.sTemplateName;
				switch(sTemplateName) {
					case "LCap":	_cLCapData.Add(cTrooper.cExpData);	break;
					case "LT1":		_cLT1Data.Add(cTrooper.cExpData);	break;
					case "LT2":		_cLT2Data.Add(cTrooper.cExpData);	break;
				}

				cLSumData += cTrooper.cExpData;
			}
			_cLSumData = cLSumData;

			cTeam = cBattleData.caTeams[1];
			foreach(Combatant nCombatant in cTeam.cMembers) {
				cTrooper = (Trooper)nCombatant;
				sTemplateName = cTrooper.sTemplateName;
				switch(sTemplateName) {
					case "RCap":	_cRCapData.Add(cTrooper.cExpData);	break;
					case "RT1":		_cRT1Data.Add(cTrooper.cExpData);	break;
					case "RT2":		_cRT2Data.Add(cTrooper.cExpData);	break;
				}

				cRSumData += cTrooper.cExpData;
			}
			_cRSumData = cRSumData;
		}
	}
}
