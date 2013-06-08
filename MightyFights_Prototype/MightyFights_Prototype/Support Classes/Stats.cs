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
			this.iHealPoint = (int)( .4 * this.iMaxHp );
			this.iFleePoint = (int)( .05 * this.iMaxHp );
			this.iPower = cRand.Next(5, 15);
			this.iAtkSpeed = cRand.Next(0, 15);
			this.iMovement = cRand.Next(0, 15);
			this.iArmorClass = cRand.Next(1, 5);
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

	public class ExperienceData 
	{
		public int iAttacks			{ get; set; }
		public int iAttackSuccess	{ get; set; }
		public int iAttackDefended	{ get; set; }
		public int iBasicAttacks	{ get; set; }
		public int iCrits			{ get; set; }
		public int iCritSuccess		{ get; set; }
		public int iCritsDefended	{ get; set; }
		public int iDefenceAttempts	{ get; set; }
		public int iKills			{ get; set; }
		public int iFleeKills		{ get; set; }
		public int iHealingKills	{ get; set; }
		public int iBuffedKills		{ get; set; }
		public int iAfflictedKills	{ get; set; }
		public int iNearDeath		{ get; set; }

		public int iAttacked		{ get; set; }
		public int iCritted			{ get; set; } 
		public int iDefendedAttacks	{ get; set; }
		public int iDefendedCrits	{ get; set; }

		public int iFlee			{ get; set; }
		public int iHealed			{ get; set; }
		public int iDarkEncounters	{ get; set; }
		public int iDarkKills		{ get; set; }
		public int iLightKills		{ get; set; }

		public int iBattles			{ get; set; }
		public int iRetreats		{ get; set; }

		public int iBuffsApplied	{ get; set; }
		public int iDragonWing		{ get; set; }
		public int iLionPaw 		{ get; set; }
		public int iEagleFeather	{ get; set; }
		public int iSnakeFang		{ get; set; }
		public int iToadEye 		{ get; set; }
		public int iWolfEar 		{ get; set; }
		public int iCrabClaw		{ get; set; }
		public int iSquirrelAcorn	{ get; set; }

		static public ExperienceData operator +(ExperienceData cLVal, ExperienceData cRVal)
		{
			ExperienceData cExp = new ExperienceData();
			cExp.iAttacks = cLVal.iAttacks + cRVal.iAttacks;
			cExp.iAttackSuccess = cLVal.iAttackSuccess + cRVal.iAttackSuccess;
			cExp.iAttackDefended = cLVal.iAttackDefended + cRVal.iAttackDefended;
			cExp.iBasicAttacks = cLVal.iBasicAttacks + cRVal.iBasicAttacks;
			cExp.iCrits = cLVal.iCrits + cRVal.iCrits;
			cExp.iCritSuccess = cLVal.iCritSuccess + cRVal.iCritSuccess;
			cExp.iCritsDefended = cLVal.iCritsDefended + cRVal.iCritsDefended;
			cExp.iDefenceAttempts = cLVal.iDefenceAttempts + cRVal.iDefenceAttempts;
			cExp.iKills	= cLVal.iKills + cRVal.iKills;
			cExp.iFleeKills = cLVal.iFleeKills + cRVal.iFleeKills;
			cExp.iHealingKills = cLVal.iHealingKills + cRVal.iHealingKills;
			cExp.iBuffedKills = cLVal.iBuffedKills + cRVal.iBuffedKills;
			cExp.iAfflictedKills = cLVal.iAfflictedKills + cRVal.iAfflictedKills;
			cExp.iNearDeath = cLVal.iNearDeath + cRVal.iNearDeath;

			cExp.iAttacked = cLVal.iAttacked + cRVal.iAttacked;
			cExp.iCritted = cLVal.iCritted + cRVal.iCritted;
			cExp.iDefendedAttacks = cLVal.iDefendedAttacks + cRVal.iDefendedAttacks;
			cExp.iDefendedCrits = cLVal.iDefendedCrits + cRVal.iDefendedCrits;

			cExp.iFlee = cLVal.iFlee + cRVal.iFlee;
			cExp.iHealed = cLVal.iHealed + cRVal.iHealed;
			cExp.iDarkEncounters = cLVal.iDarkEncounters + cRVal.iDarkEncounters;
			cExp.iDarkKills = cLVal.iDarkKills + cRVal.iDarkKills;
			cExp.iLightKills = cLVal.iLightKills + cRVal.iLightKills;

			cExp.iBattles = cLVal.iBattles + cRVal.iBattles;
			cExp.iRetreats = cLVal.iRetreats + cRVal.iRetreats;

			cExp.iBuffsApplied = cLVal.iBuffsApplied + cRVal.iBuffsApplied;
			cExp.iDragonWing = cLVal.iDragonWing + cRVal.iDragonWing;
			cExp.iLionPaw = cLVal.iLionPaw + cRVal.iLionPaw;
			cExp.iEagleFeather = cLVal.iEagleFeather + cRVal.iEagleFeather;
			cExp.iSnakeFang = cLVal.iSnakeFang + cRVal.iSnakeFang;
			cExp.iToadEye = cLVal.iToadEye + cRVal.iToadEye;
			cExp.iWolfEar = cLVal.iWolfEar + cRVal.iWolfEar;
			cExp.iCrabClaw = cLVal.iCrabClaw + cRVal.iCrabClaw;
			cExp.iSquirrelAcorn = cLVal.iSquirrelAcorn + cRVal.iSquirrelAcorn;

			return cExp;
		}
	}
}
