using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	public class SelectedTemplate
	{
		int		_iTemplateId,
				_iCurrentCount,
				_iMaxCount;
	}

	public class CompanyStats
	{
		public int iBattlesWon		{ get; set; }
		public int iBattlesLost		{ get; set; }
		public int iMissionsSent	{ get; set; }
		public int iMissionsWon		{ get; set; }
		public int iMissionsLost	{ get; set; }
		public int iTotalKills		{ get; set; }
		public int iTotalDeaths		{ get; set; }
	}

	public class Company
	{
		Dictionary<string, SelectedTemplate>	_hTemplateByName = new Dictionary<string,SelectedTemplate>();
		Dictionary<int, SelectedTemplate>		_hTemplateById = new Dictionary<int,SelectedTemplate>();
		CompanyStats							_cStats;

		public int iMaxSize		{ get; set; }
		public string sName		{ get; set; }
		public BasicSprite cIcon	{ get; set; }
		
		// comander
	}
}
