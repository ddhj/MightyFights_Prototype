using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public class BattleObj : IObject	{
		public Vector2 tPos				{ get; set; }
		public Vector2 tCenter			{ get; set; }
		public int iId					{ get; set; }
		public EObjectStates eObjState	{ get; set; }
	}

	public abstract class Combatant : BattleObj, IBuffableObject
	{
		public AiBattleData cAiData		{ get; set; }
		public int iWeaponRange			{ get; set; }
		public bool bAvailablePos		{ get; }
		public Zone cZone				{ get; set; }
		public Stats cStats				{ get; set; }
		public BattleObj nTarget		{ get; set; }
		public Team cTeam				{ get; }
		public bool bPoisonBlade		{ get; set; }
		public ExperienceData cExpData	{ get; }

		void DealDamage(Combatant nOpponent, int iDamage, bool bCrit);
		float Heal( float fHp );
		void RemoveHeal( );
		bool IsDead();
		bool InWeaponRange( bool bCollisionTest );
		void SetAttacker(Combatant nCombatant, out int iPos);
		void RemoveAttacker(int iPos);
		Vector2 RequestAttackPoint(Combatant nCombatant, out int iPos);
		Vector2 RequestPersuitPoint(int iPos);
	}

	public class Healer : BattleObj	{
		float fHealth	{ get; }
		float fHp		{ get; set; }
		bool bActive		{ get; }
		bool bAvailableSpots	{ get; }

	// Functions
		Vector2 GetOpenLocation( );
		void TakeSpot( Combatant nSoldier );
		void FreeSpot( Combatant nSoldier );
		void DoDamage( float fDamage );
	}

	public interface IStatusEffects { 
		//Dictionary<EStatusEffects, StatusEffectData>
	}
}
