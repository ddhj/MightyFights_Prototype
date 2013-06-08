using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework.Media;

namespace MightyFights_Prototype
{
	public partial class DebugData : Form
	{
		public DebugData()
		{
			InitializeComponent();

			_cLifeBar.Checked = DataStore.cInstance.bLifeBars;
			_cDmgNum.Checked = DataStore.cInstance.bDamageNumbers;
			_cHealSpots.Checked = DataStore.cInstance.bHealSpots;
			_cBattleZone.Checked = DataStore.cInstance.bZoneDisplay;
			_cSlowMo.Checked = DataStore.cInstance.bSlowMo;
			_cMusic.Checked = DataStore.cInstance.bPlayMusic;
			_cShowBg.Checked = DataStore.cInstance.bShowBg;
		}

		public void SetChecks()
		{
			_cLifeBar.Checked = DataStore.cInstance.bLifeBars;
			_cDmgNum.Checked = DataStore.cInstance.bDamageNumbers;
			_cHealSpots.Checked = DataStore.cInstance.bHealSpots;
			_cBattleZone.Checked = DataStore.cInstance.bZoneDisplay;
			_cSlowMo.Checked = DataStore.cInstance.bSlowMo;
			_cMusic.Checked = DataStore.cInstance.bPlayMusic;
			_cShowBg.Checked = DataStore.cInstance.bShowBg;
		}

		private void _cLifeBar_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bLifeBars = _cLifeBar.Checked;
		}

		private void _cDmgNum_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bDamageNumbers = _cDmgNum.Checked;
		}

		private void _cHealSpots_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bHealSpots = _cHealSpots.Checked;
		}

		private void _cBattleZone_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bZoneDisplay = _cBattleZone.Checked;
		}

		private void _cSlowMo_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bSlowMo = _cSlowMo.Checked;
		}

		private void _cMusic_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bPlayMusic = _cMusic.Checked;
			if(_cMusic.Checked) {
				if(DataStore.cInstance.cBgm != null) 
					MediaPlayer.Play(DataStore.cInstance.cBgm);
			} else { 
				MediaPlayer.Stop();
			}
		}

		private void _cShowBg_CheckedChanged(object sender, EventArgs e)
		{
			DataStore.cInstance.bShowBg = _cShowBg.Checked;
		}

		private void _cOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
