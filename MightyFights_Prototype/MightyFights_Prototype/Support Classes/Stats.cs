using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class Stats
	{
		public float fHp			{ get; set; }
		public int iMaxHp		{ get; set; }
		public int iHealPoint	{ get; set; }
		public int iPower		{ get; set; }
		public int iAtkSpeed	{ get; set; }
		public int iMovement	{ get; set; }

		public Stats()
		{
			Random cRand = new Random();
			this.fHp = this.iMaxHp = cRand.Next(100, 300);
			this.iHealPoint = (int)( .5 * this.iMaxHp );
			this.iPower = cRand.Next(5, 15);
			this.iAtkSpeed = cRand.Next(0, 15);
			this.iMovement = cRand.Next(0, 15);
		}

		public Stats(Stats cSrc)
		{ 
			this.fHp = this.iMaxHp = cSrc.iMaxHp;
			this.iPower = cSrc.iPower;
			this.iHealPoint = cSrc.iHealPoint;
			this.iAtkSpeed = cSrc.iAtkSpeed;
			this.iMovement = cSrc.iMovement;
		}
	}
}
