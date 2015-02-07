using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	[Serializable]
	public class TemplateCfgMaster : TemplateConfig
	{
		ExperienceData		_cExpData;

		public ExperienceData	cExpData	{ get { return _cExpData; } set { _cExpData = value; }}

		public TemplateCfgMaster() {}
		public TemplateCfgMaster(string sTrooperType, string sColor) : base(sTrooperType, sColor) {} 
		public TemplateCfgMaster(TemplateConfig cTemplateSrc) : base(cTemplateSrc) {} 
	}

	[Serializable]
	public class Steward
	{
		Dictionary<EBuffEffects, int>		_cBuffs = new Dictionary<EBuffEffects,int>();
		List<TemplateCfgMaster>				_cTemplates = new List<TemplateCfgMaster>();
		List<TemplateCfgMaster>				_cCaptains = new List<TemplateCfgMaster>();
		Dictionary<string, Company>			_hCompaniesByIconName = new Dictionary<string,Company>();
		Dictionary<string, Company>			_hCompaniesByName = new Dictionary<string,Company>();
		List<Company>						_caCompanies = new List<Company>();

		public List<TemplateCfgMaster> cTemplates		{ get { return _cTemplates; } set { _cTemplates = value; }}
		public List<TemplateCfgMaster> cCaptains		{ get { return _cCaptains; } set { _cCaptains = value; }}
		public Dictionary<EBuffEffects, int> cBuffs		{ get { return _cBuffs; } set { _cBuffs = value; }}
		public Dictionary<string, Company> hCompaniesByIconName		{ get { return _hCompaniesByIconName; }}
		public Dictionary<string, Company> hCompaniesByName			{ get { return _hCompaniesByName; }}
		public List<Company> caCompanies				{ get { return _caCompanies; } set { _caCompanies = value; }}

		public void HydrateCompanyRefLists()
		{
			foreach(Company cCompany in caCompanies) { 
				_hCompaniesByIconName.Add(cCompany.sIconName, cCompany);
				_hCompaniesByName.Add(cCompany.sName, cCompany);

				// get the sprite from the content engine 
				cCompany.cIcon = new BasicSprite();
				cCompany.cIcon.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(cCompany.sIconName);
				cCompany.cIcon.tPos = new Vector2(0, 0);
				cCompany.cIcon.sTexName = cCompany.sIconName;
				cCompany.cIcon.fZRange = .5f;
				cCompany.cIcon.cFrame = new Frame(cCompany.cIcon.cTexRef.Bounds, 
					new Vector2(cCompany.cIcon.cTexRef.Bounds.Width / 2, cCompany.cIcon.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cCompany.cIcon.cTexRef.Bounds.Width, cCompany.cIcon.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);

			}
		}
	}
}
