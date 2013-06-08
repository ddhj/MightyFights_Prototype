using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MightyFights_Prototype
{
	public partial class StatsDialog : Form
	{
		public StatsDialog()
		{
			InitializeComponent();
		}

		public void InitGridData()
		{
			ExperienceData	cLSumData = new ExperienceData(),
							cRSumData = new ExperienceData();

			Team			cTeam = DataStore.cInstance.cBattleData.caTeams[0];
			string			sTemplateName;
			Trooper			cTrooper;

			foreach(ICombatant nCombatant in cTeam.cMembers) { 
				cTrooper = (Trooper)nCombatant;
				sTemplateName = cTrooper.sTemplateName;
				switch(sTemplateName) { 
					case "LCap":	_cLCapData.Add(cTrooper.cExpData);	break;
					case "LT1":		_cLT1Data.Add(cTrooper.cExpData);	break;
					case "LT2":		_cLT2Data.Add(cTrooper.cExpData);	break;
				}

				cLSumData += cTrooper.cExpData;
			}
			_cLSumData.Add(cLSumData);

			cTeam = DataStore.cInstance.cBattleData.caTeams[1];
			foreach(ICombatant nCombatant in cTeam.cMembers) { 
				cTrooper = (Trooper)nCombatant;
				sTemplateName = cTrooper.sTemplateName;
				switch(sTemplateName) { 
					case "RCap":	_cRCapData.Add(cTrooper.cExpData);	break;
					case "RT1":		_cRT1Data.Add(cTrooper.cExpData);	break;
					case "RT2":		_cRT2Data.Add(cTrooper.cExpData);	break;
				}

				cRSumData += cTrooper.cExpData;
			}
			_cRSumData.Add(cRSumData);
		}

		private void StatsDialog_Load(object sender, EventArgs e)
		{
		}

		private void _cOk_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
