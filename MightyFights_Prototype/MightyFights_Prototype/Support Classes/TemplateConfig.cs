using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	[Serializable]
	public class TemplateConfig
	{
		string		_sTrooperType,
					_sColor;

		// this i am not 100% certian what I want to do yet 
		Dictionary<string, float>		_cActionModifier = new Dictionary<string,float>();

		public string sTrooperType		{ get { return _sTrooperType; } set { _sTrooperType = value; }}
		public string sColor			{ get { return _sColor; } set { _sColor = value; }}
		public Stats cStats				{ get; set; }
		public int iTopLevel			{ get; set; }
		public int iBottomLevel			{ get; set; }
		public int iTLeftPos			{ get; set; }
		public int iBLeftPos			{ get; set; }
		public int iCount				{ get; set; }
		public int iId					{ get; set; }
		public string sTemplateName		{ get; set; }
		public Dictionary<string, float> cActionModifier	{ get { return _cActionModifier; } set { _cActionModifier = value; }}


		public TemplateConfig() {}

		public TemplateConfig(string sTrooperType, string sColor)
		{
			_sColor = sColor;
			_sTrooperType = sTrooperType;

			this.iTopLevel = 0;
			this.iBottomLevel = 0;
		}

		public TemplateConfig(TemplateConfig cSrc)
		{
			_sTrooperType = cSrc.sTrooperType;
			_sColor = cSrc.sColor;
			if(cSrc.cStats != null)
				this.cStats = new Stats(cSrc.cStats);
			else this.cStats = new Stats();

			this.iBottomLevel = cSrc.iBottomLevel;
			this.iTopLevel = cSrc.iTopLevel;
			this.iCount = cSrc.iCount;
		}

		public void SetData(TemplateConfig cSrc)
		{
			_sTrooperType = cSrc.sTrooperType;
			_sColor = cSrc.sColor;
			if(cSrc.cStats != null)
				this.cStats = new Stats(cSrc.cStats);
			else this.cStats = new Stats();

			this.iBottomLevel = cSrc.iBottomLevel;
			this.iTopLevel = cSrc.iTopLevel;
			this.iCount = cSrc.iCount;
		}
	}
}
