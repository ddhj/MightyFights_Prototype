using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization.Json;

namespace MightyFights_Prototype
{
	public partial class BattleGround_Basic : IGameScene
	{
		void ProcessBuffClick(object oSender, object oArgs)
		{
			_cBuffContainerList[((BasicBuff)oArgs).eType].AddBuff((BasicBuff)oArgs);
			if( _cBuffSfx.State == Microsoft.Xna.Framework.Audio.SoundState.Playing )
				_cBuffSfx.Stop( );
			_cBuffSfx.Play( );
		}

		void ProcessHammerClick(object oSender, object oArgs)
		{
			++_iHammerCtr;
			if( _cHammerSfx.State == Microsoft.Xna.Framework.Audio.SoundState.Playing )
				_cHammerSfx.Stop( );
			_cHammerSfx.Play( );
		}

		void WriteExpData()
		{
			DataContractJsonSerializer cExpDataSer = new DataContractJsonSerializer(typeof(ExperienceData));
			System.IO.MemoryStream	cData = new System.IO.MemoryStream();

			foreach( Team cTeam in _cBattleData.caTeams )
				if( cTeam.cActiveList.Count != 0 ) 
					foreach(ICombatant nCom in cTeam.cActiveList.Values)
						cExpDataSer.WriteObject(cData, nCom.cExpData);
			
			using(var vFile = System.IO.File.Create(string.Format("ExpData_{0}.txt", DateTime.Now.ToString("u").Replace(":", "")))) { 
				cData.Seek(0, System.IO.SeekOrigin.Begin);
				cData.CopyTo(vFile);
			}
		}

		void _cDebugButton_Click(object sender, EventArgs e)
		{
			_cDebugDialog.SetChecks();
			_cDebugDialog.ShowDialog();
		}

		void _cDlg_Shown(object sender, EventArgs e)
		{
			_eOldState = _cBattleData.eState;
			_cBattleData.eState = EBattlegroundState.Dialog;
		}

		void _cDlg_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			_cBattleData.eState = _eOldState;
		}

	}
}
