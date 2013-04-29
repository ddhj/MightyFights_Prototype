using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class Stats
	{
		public int iHp		{ get; set; }
		public int iPower	{ get; set; }
		public int iAtkSpeed	{ get; set; }
		public int iMovement	{ get; set; }

		public Stats()
		{
			Random cRand = new Random();
			this.iHp = cRand.Next(100, 300);
			this.iPower = cRand.Next(5, 15);
			this.iAtkSpeed = cRand.Next(0, 15);
			this.iMovement = cRand.Next(0, 15);
		}

		public Stats(Stats cSrc)
		{ 
			this.iHp = cSrc.iHp;
			this.iPower = cSrc.iPower;
			this.iAtkSpeed = cSrc.iAtkSpeed;
			this.iMovement = cSrc.iMovement;
		}
	}
}
