using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class TemplateConfig
	{
		string		_sTrooperType,
					_sColor;

		// this i am not 100% certian what I want to do yet 
		Dictionary<string, float>		_cActionModifier = new Dictionary<string,float>();

		public string sTrooperType		{ get { return _sTrooperType; }}
		public string sColor			{ get { return _sColor; } set { _sColor = value; }}
		public Stats cStats				{ get; set; }
		public int iTopLevel			{ get; set; }
		public int iBottomLevel			{ get; set; }

		public TemplateConfig(string sTrooperType, string sColor)
		{
			_sColor = sColor;
			_sTrooperType = sTrooperType;

			this.iTopLevel = 0;
			this.iBottomLevel = 0;
		}
	}
}
