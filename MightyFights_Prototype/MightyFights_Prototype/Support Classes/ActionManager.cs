using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public abstract class ActionManager
	{
		object _oData;
		public abstract void Process();
		public ActionManager(object oData)
		{
			_oData = oData;
		}
	}
}
