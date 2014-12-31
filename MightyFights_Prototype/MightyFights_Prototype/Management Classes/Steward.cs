using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class TemplateCfgMaster : TemplateConfig
	{
		ExperienceData		_cExpData;

		public ExperienceData	cExpData	{ get { return _cExpData; } set { _cExpData = value; }}

		public TemplateCfgMaster(string sTrooperType, string sColor) : base(sTrooperType, sColor) {} 
		public TemplateCfgMaster(TemplateConfig cTemplateSrc) : base(cTemplateSrc) {} 
	}

	public class Steward
	{
		Dictionary<EBuffEffects, int>		_cBuffs = new Dictionary<EBuffEffects,int>();
		List<TemplateCfgMaster>				_cTemplates = new List<TemplateCfgMaster>();
		Dictionary<string, Company>			_hCompaniesByIconName = new Dictionary<string,Company>();
		Dictionary<string, Company>			_hCompaniesByName = new Dictionary<string,Company>();

		public List<TemplateCfgMaster> cTemplates		{ get { return _cTemplates; }}
		public Dictionary<EBuffEffects, int> cBuffs		{ get { return _cBuffs; }}
		public Dictionary<string, Company> hCompaniesByIconName		{ get { return _hCompaniesByIconName; }}
		public Dictionary<string, Company> hCompaniesByName			{ get { return _hCompaniesByName; }}

	}
}
