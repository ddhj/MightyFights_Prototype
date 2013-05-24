using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public class Stats: IBattleStats
	{
		public float fHp		{ get; set; }
		public int iMaxHp		{ get; set; }
		public int iHealPoint	{ get; set; }
		public int iFleePoint	{ get; set; }
		public int iPower		{ get; set; }
		public int iAtkSpeed	{ get; set; }
		public int iMovement	{ get; set; }
		public int iArmorClass	{ get; set; }
		public float fCrit		{ get; set; }

		public Stats()
		{
			// this, if we use rand, should be the one we use
				// but right now, it causes the sides to be always lopsided
//			Random	cRand = DataStore.cInstance.cRand;
			Random	cRand = new Random();
			this.fHp = this.iMaxHp = cRand.Next(100, 300);
			this.iHealPoint = (int)( .6 * this.iMaxHp );
			this.iFleePoint = (int)( .2 * this.iMaxHp );
			this.iPower = cRand.Next(5, 15);
			this.iAtkSpeed = cRand.Next(0, 15);
			this.iMovement = cRand.Next(0, 15);
			this.iArmorClass = cRand.Next(0, 10);
		}

		public Stats(Stats cSrc)
		{ 
			this.fHp = cSrc.fHp;
			this.iMaxHp = cSrc.iMaxHp;
			this.iPower = cSrc.iPower;
			this.iHealPoint = cSrc.iHealPoint;
			this.iFleePoint = cSrc.iFleePoint;
			this.iAtkSpeed = cSrc.iAtkSpeed;
			this.iMovement = cSrc.iMovement;
			this.iArmorClass = cSrc.iArmorClass;
			this.fCrit = cSrc.fCrit;
		}

		public void SetStats(IBattleStats nStats)
		{
			this.iPower = nStats.iPower;
			this.iAtkSpeed = nStats.iAtkSpeed;
			this.iMovement = nStats.iMovement;
			this.iArmorClass = nStats.iArmorClass;
			this.fCrit = nStats.fCrit;
		}
	}

	public interface IBattleStats
	{
		int iPower		{ get; set; }
		int iAtkSpeed	{ get; set; }
		int iMovement	{ get; set; }
		int iArmorClass	{ get; set; }
		float fCrit		{ get; set; }
	}
}
