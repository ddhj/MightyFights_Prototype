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
	}
}
