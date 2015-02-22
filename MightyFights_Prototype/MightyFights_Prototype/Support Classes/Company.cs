using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	[Serializable]
	public class SelectedTemplate
	{
		public int	iTemplateId			{ get; set; }
		public int	iCurrentCount		{ get; set; }
		public int	iMaxCount			{ get; set; }
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

	[Serializable]
	public class Company
	{
		Dictionary<string, SelectedTemplate>	_hTemplateByName = new Dictionary<string,SelectedTemplate>();
		Dictionary<int, SelectedTemplate>		_hTemplateById = new Dictionary<int,SelectedTemplate>();
		CompanyStats							_cStats = new CompanyStats();

		public int iMaxSize		{ get; set; }
		public string sName		{ get; set; }
		public string sIconName	{ get; set; }
		public int iTemplageMax { get; set; }
		public List<SelectedTemplate> caTemplates	{ get { return _hTemplateByName.Values.ToList(); }}
		public CompanyStats cStats { get { return _cStats; } set { _cStats = value; }}
	}
}
