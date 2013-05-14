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
			_cBuffContainerList[((BasicBuff)oArgs).sType].AddBuff((BasicBuff)oArgs);
		}
	}
}
