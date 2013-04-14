using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class Stats
	{
		int			_iHp = 50, 
					_iAc = 10, 
					_iPower = 5;

		public int iHp		{ get { return _iHp; } set { _iHp = value; }}
		public int iAc		{ get { return _iAc; } set { _iAc = value; }}
		public int iPower	{ get { return _iPower; } set { _iPower = value; }}

		// maybe more?
	}
}
