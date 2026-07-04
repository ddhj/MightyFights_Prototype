// system include
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace MightyFights_Prototype	{
	/// <summary> class for the basic fields needed for battleground objects </summary>
	public class BattleObj : IObject	{
	// Data
		protected int		_iId;
		protected Vector2	_tPos,
							_tCenter;
		protected EObjectStates	_eObjState;

	// Properties
		public int iId	{ get { return _iId; } set { _iId = value; }}
		public virtual Vector2 tPos		{ get { return _tPos; } set { _tPos = value; }}
		public virtual Vector2 tCenter	{ get { return _tCenter; } set { _tCenter = value; }}
		public virtual EObjectStates eObjState	{ get { return _eObjState; } set { _eObjState = value; }}
	}

	/// <summary> class for the publicly shared values between active combatant types </summary>
	public abstract class Combatant : BattleObj, IBuffableObject	{
	// Data
		protected int	_iWeaponRange;
		protected bool	_bPoisonBlade;
		protected Zone	_cZone;
		protected Team	_cTeam;
		protected Stats	_cStats;
		protected AiBattleData	_cAiData;
		protected BattleObj		_cTarget;
		protected ExperienceData	_cExpData;

	// Properties
		public int iWeaponRange			{ get { return _iWeaponRange; } set { _iWeaponRange = value; }}
		public bool bPoisonBlade		{ get { return _bPoisonBlade; } set { _bPoisonBlade = value; }}
		public AiBattleData cAiData		{ get { return _cAiData; } set { _cAiData = value; }}
		public Zone cZone				{ get { return _cZone; } set { _cZone = value; }}
		public Stats cStats				{ get { return _cStats; } set { _cStats = value; }}
		public BattleObj cTarget		{ get { return _cTarget; } set { _cTarget = value; }}
		public Team cTeam				{ get { return _cTeam; } set { _cTeam = value; }}
		//// ddhj: kaiju -- render/geometry scale of this combatant's body. Defaults to 1 (a
		//// normal-sized fighter); Trooper overrides it to expose _fScale so attackers can widen
		//// their own arrival tolerance against oversized targets (see InWeaponRange).
		public virtual float fCombatantScale	{ get { return 1f; }}
		public ExperienceData cExpData	{ get { return _cExpData; } set { _cExpData = value; }}
		public virtual bool bAvailablePos		{ get; set; }

	// Functions
		public abstract void DealDamage(Combatant nOpponent, int iDamage, bool bCrit);
		public abstract float Heal( float fHp );
		public abstract void RemoveHeal( );
		public abstract bool IsDead();
		public abstract bool InWeaponRange( bool bCollisionTest );
		public abstract void SetAttacker(Combatant nCombatant, out int iPos);
		public abstract void RemoveAttacker(int iPos);
		public abstract Vector2 RequestAttackPoint(Combatant nCombatant, out int iPos);
		public abstract Vector2 RequestPersuitPoint(int iPos);
		public abstract void AddBuff(BasicBuff cBuff);
		public abstract void RemoveBuff(EBuffEffects eType);
	}

/*	/// <summary> class for the publicly shared values between </summary>
	public abstract class Healer : BattleObj	{
		public float fHealth	{ get; set; }
		public float fHp		{ get; set; }
		public bool bActive		{ get; set; }
		public bool bAvailableSpots	{ get; set; }

	// Functions
		public abstract Vector2 GetOpenLocation( );
		public abstract void TakeSpot( Combatant nSoldier );
		public abstract void FreeSpot( Combatant nSoldier );
		public abstract void DoDamage( float fDamage );
	}
*/
	public interface IStatusEffects { 
		//Dictionary<EStatusEffects, StatusEffectData>
	}
}
