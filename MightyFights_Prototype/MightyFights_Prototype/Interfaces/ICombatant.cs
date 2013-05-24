using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public interface IBattleObj : IObject	{
		Vector2 tPos			{ get; set; }
		Vector2 tCenter			{ get; set; }
	}

	public interface ICombatant : IBattleObj, IBuffableObject
	{
		AiBattleData cAiData	{ get; set; }
		int iWeaponRange		{ get; set; }
		bool bAvailablePos		{ get; }
		Zone cZone				{ get; set; }
		Stats cStats			{ get; set; }
		IBattleObj nTarget		{ get; set; }
		Team cTeam				{ get; }
		bool bPoisonBlade		{ get; set; }

		void DealDamage(ICombatant nOpponent, int iDamage, bool bCrit);
		float Heal( float fHp );
		void RemoveHeal( );
		bool IsDead();
		bool InWeaponRange( bool bCollisionTest );
		void SetAttacker(ICombatant nCombatant, out int iPos);
		void RemoveAttacker(int iPos);
		Vector2 RequestAttackPoint(ICombatant nCombatant, out int iPos);
		Vector2 RequestPersuitPoint(int iPos);
	}

	public interface IHealer : IBattleObj	{
		float fHealth	{ get; }
		float fHp		{ get; set; }
		bool bActive		{ get; }
		bool bAvailableSpots	{ get; }

	// Functions
		Vector2 GetOpenLocation( );
		void TakeSpot( ICombatant nSoldier );
		void FreeSpot( ICombatant nSoldier );
		void DoDamage( float fDamage );
	}

	public interface IStatusEffects { 
		//Dictionary<EStatusEffects, StatusEffectData>
	}
}
