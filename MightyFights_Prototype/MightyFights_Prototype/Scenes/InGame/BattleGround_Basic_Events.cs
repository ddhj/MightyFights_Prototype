using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public partial class BattleGround_Basic : IGameScene
	{
		void ToggleLifeBarChange(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bLifeBars = _cToggleLifeBar.Checked;
		}

		void ToggleDamageNumbersChange(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bDamageNumbers = _cToggleDamageNumbers.Checked;
		}

		void ToggleSlowMoChange(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bSlowMo = _cToggleSlowMo.Checked;
		}

		void ToggleHealSpots(object oSender, EventArgs eEvtArgs) 
		{
			DataStore.cInstance.bHealSpots = _cToggleHealSpots.Checked;
		}

		void ProcessBuffClick(object oSender, object oArgs)
		{
			_cBuffContainerList[((BasicBuff)oArgs).eType].AddBuff((BasicBuff)oArgs);
		}

		void ProcessHammerClick(object oSender, object oArgs)
		{
			++_iHammerCtr;
		}

		void ProcessCheck(object oSender, object oArgs)
		{
			BuffContainer cContainer = (BuffContainer)((System.Windows.Forms.CheckBox)oSender).Tag;
			cContainer.bRad = ((System.Windows.Forms.CheckBox)oSender).Checked;
		}

		void ProcessValChange(object oSender, object oArgs) 
		{
			BuffContainer cContainer = (BuffContainer)((System.Windows.Forms.NumericUpDown)oSender).Tag;
			cContainer.iRad = (int)((System.Windows.Forms.NumericUpDown)oSender).Value;
		}

		void ProcessAllCheck(object oSender, object oArgs)
		{
			_cAcornRadCb.Checked = 
			_cClawRadCb.Checked = 
			_cWolfRadCb.Checked = 
			_cToadRadCb.Checked = 
			_cSnakeRadCb.Checked = 
			_cEagleRadCb.Checked = 
			_cLionRadCb.Checked = 
			_cDragRadCb.Checked = _cAllRadCb.Checked;
		}
	}
}
