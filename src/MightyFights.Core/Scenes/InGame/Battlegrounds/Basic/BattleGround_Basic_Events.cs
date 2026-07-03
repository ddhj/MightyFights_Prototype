using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;

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
			DataContractJsonSerializer cExpListSer = new DataContractJsonSerializer(typeof(List<ExperienceData>));
			System.IO.MemoryStream	cData = new System.IO.MemoryStream();

			foreach( Team cTeam in _cBattleData.caTeams )
				if( cTeam.cActiveList.Count != 0 )
					foreach(Combatant nCom in cTeam.cActiveList.Values)
						cExpDataSer.WriteObject(cData, nCom.cExpData);

			// PORT (T1.5): the balance tally that used to be visible in the WinForms StatsDialog
			// grid (see ExperienceTally / PORT_NOTES.md "WinForms debug tooling") had no output
			// path left after the dialog was removed -- it was being computed but never surfaced
			// anywhere. Appended here onto the existing raw-combatant dump so balance data is
			// still inspectable without a WinForms replacement.
			WriteTallyLabel(cData, "LCap");	cExpListSer.WriteObject(cData, _cExpTally.cLCapData);
			WriteTallyLabel(cData, "LT1");		cExpListSer.WriteObject(cData, _cExpTally.cLT1Data);
			WriteTallyLabel(cData, "LT2");		cExpListSer.WriteObject(cData, _cExpTally.cLT2Data);
			WriteTallyLabel(cData, "RCap");	cExpListSer.WriteObject(cData, _cExpTally.cRCapData);
			WriteTallyLabel(cData, "RT1");		cExpListSer.WriteObject(cData, _cExpTally.cRT1Data);
			WriteTallyLabel(cData, "RT2");		cExpListSer.WriteObject(cData, _cExpTally.cRT2Data);
			WriteTallyLabel(cData, "LSum");	cExpDataSer.WriteObject(cData, _cExpTally.cLSumData);
			WriteTallyLabel(cData, "RSum");	cExpDataSer.WriteObject(cData, _cExpTally.cRSumData);

			using(var vFile = System.IO.File.Create(string.Format("ExpData_{0}.txt", DateTime.Now.ToString("u").Replace(":", "")))) {
				cData.Seek(0, System.IO.SeekOrigin.Begin);
				cData.CopyTo(vFile);
			}
		}

		// PORT (T1.5): small helper for the tally labels appended in WriteExpData above --
		// the file is already a flat sequence of dumped objects (no enclosing JSON document),
		// so plain text labels between entries match the existing convention.
		void WriteTallyLabel(System.IO.Stream cStream, string sLabel)
		{
			byte[] baLabel = System.Text.Encoding.UTF8.GetBytes("\r\n" + sLabel + ":\r\n");
			cStream.Write(baLabel, 0, baLabel.Length);
		}

		// PORT (T1.5): _cDebugButton_Click / _cDlg_Shown / _cDlg_FormClosed removed along
		// with the WinForms StatsDialog/DebugData dialogs -- see PORT_NOTES.md.

		public void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
		}

		void InputSystem_MouseMove(object sender, MouseEventArgs e)
		{
			_cCursor.Update(e.Location);
		}

		public void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;
		}
	}
}
