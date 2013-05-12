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
	class FleeSpot	{
	// Data
		int		_iMaxSlots;
		List<Vector2>	_taOpenSpots = new List<Vector2>( );
		Dictionary<int,KeyValuePair<Vector2,ICombatant>>	_cUsedSpots = new Dictionary<int,KeyValuePair<Vector2,ICombatant>>( );

	// Properties
		public bool bOpen	{ get { return _cUsedSpots.Count < _iMaxSlots; }}
		public Vector2 tNextOpenSpot	{ get { return _taOpenSpots[0]; }}
		public Dictionary<int,KeyValuePair<Vector2,ICombatant>> cUsedSpots	{ get { return _cUsedSpots; }}

	// Constructor
		public FleeSpot( int iMaxSlots, int iRadius, Vector2 tCenter )
		{
			_iMaxSlots = iMaxSlots;
		}

	// Functions
		public void TakeSpot( ICombatant nSoldier )
		{
			_cUsedSpots.Add( nSoldier.iId, new KeyValuePair<Vector2,ICombatant>( _taOpenSpots[0], nSoldier ));
			_taOpenSpots.RemoveAt( 0 );
		}

		public void FreeSpot( ICombatant nSoldier )
		{
			_taOpenSpots.Add( _cUsedSpots[nSoldier.iId].Key );
			_cUsedSpots.Remove( nSoldier.iId );
		}

		public void ResetSpots( )
		{
			foreach( KeyValuePair<Vector2,ICombatant> tPair in _cUsedSpots.Values )
				_taOpenSpots.Add( tPair.Key );
			_cUsedSpots.Clear( );
		}
	}
}
