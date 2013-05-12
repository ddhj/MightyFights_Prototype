// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype	{
	public class Healer	{
	// Data
		int			_iId,
					_iMaxHp;
		float		_fHp,
					_fRegenRate,
					_fHealRate;
		Vector2		_tPos;
		FleeSpot	_cSupportZone;

	// Properties
		public bool bActive	{ get { return _fHp > 0; }}
		public bool bAvailableSpots	{ get { return _cSupportZone.bOpen; }}

	// Constructor
		public Healer( int iId, int iMaxHp, int iMaxSlots, float fHealRate, float fRegenRate, Vector2 tPos, Team cTeam )
		{
			_iMaxHp = iMaxHp;
			_fRegenRate = fRegenRate;
			_fHealRate = fHealRate;
			_tPos = tPos;

			_cSupportZone = new FleeSpot( iMaxSlots, 60, new Vector2( tPos.X + ( cTeam.bDirection ? 1 : -1 ) * 100, tPos.Y )); 
		}

	// Functions
		public Vector2 GetOpenLocation( )
		{
			return _cSupportZone.tNextOpenSpot;
		}

		public void TakeSpot( ICombatant nSoldier )
		{
			_cSupportZone.TakeSpot( nSoldier );
		}

		public void FreeSpot( ICombatant nSoldier )
		{
			_cSupportZone.FreeSpot( nSoldier );
		}

		public void DoDamage( float fDamage )
		{
			_fHp -= fDamage;
		}

		public void Process( GameTime cTime )
		{
			if( _cSupportZone.cUsedSpots.Count == 0 )
				_fHp += _fRegenRate;
			else if( _fHp > 0 )
			{
				foreach( KeyValuePair<Vector2,ICombatant> tPair in _cSupportZone.cUsedSpots.Values )
					if( _fHp > 0 )
						if( _fHp > _fHealRate )
							_fHp -= tPair.Value.Heal( _fHealRate );
						else	_fHp -= tPair.Value.Heal( _fHp );
			}
			else	{
				foreach( KeyValuePair<Vector2,ICombatant> tPair in _cSupportZone.cUsedSpots.Values )
					tPair.Value.RemoveHeal( );
				_cSupportZone.ResetSpots( );
			}
		}
	}
}
