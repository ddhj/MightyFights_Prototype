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
		int		_iMaxSlots,
				_iRadius;
		Vector2	_tCenter;
		List<Vector2>	_taOpenSpots = new List<Vector2>( );
		Dictionary<int,KeyValuePair<Vector2,ICombatant>>	_cUsedSpots = new Dictionary<int,KeyValuePair<Vector2,ICombatant>>( );

	// Properties
		public bool bOpen	{ get { return _cUsedSpots.Count < _iMaxSlots; }}
		public int iRadius	{ get { return _iRadius; }}
		public Vector2 tNextOpenSpot	{ get { return _taOpenSpots[0]; }}
		public Vector2 tCenter			{ get { return _tCenter; }}
		public Dictionary<int,KeyValuePair<Vector2,ICombatant>> cUsedSpots	{ get { return _cUsedSpots; }}

		public List<Vector2> taOpenSpots	{ get { return _taOpenSpots; }}

	// Constructor
		public FleeSpot( int iMaxSlots, int iRadius, Vector2 tCenter )
		{
			int		iMaxRings = 1,
					iRing = 1;
			float	fRingPoint;
			float[]	faRingWidth;
			Vector2	tPos;

			// record passed in items
			_iMaxSlots = iMaxSlots;
			_tCenter = tCenter;
			_iRadius = iRadius;

			// if more than x slots, make 3 'rings'
			if( iMaxSlots > 8 )
			{
				iMaxRings = 3;
				iMaxSlots = 8;
			}
			else	iMaxRings = 2;

			// record the center as the first slot
			_taOpenSpots.Add( tCenter );
			--iMaxSlots;

			// make the 2nd ring
			fRingPoint = 360f / ( iMaxSlots + 1 );
			faRingWidth = new float[] { 0, (float)iRadius / iMaxRings, 2 * (float)iRadius / iMaxRings };
			for( int iCount = 0; iCount < iMaxSlots; ++iCount )
			{
				tPos = new Vector2( tCenter.X + faRingWidth[iRing] * (float)Math.Cos( iCount * fRingPoint ), tCenter.Y + faRingWidth[iRing] * (float)Math.Sin( iCount * fRingPoint ));
				_taOpenSpots.Add( tPos );
			}

			// if necessary, make the 3rd ring
			if( _iMaxSlots > 8 )
			{
				++iRing;
				iMaxSlots = _iMaxSlots - 8;
				fRingPoint = 360f / ( iMaxSlots + 1 );
				for( int iCount = 0; iCount < iMaxSlots; ++iCount )
				{
					tPos = new Vector2( tCenter.X + faRingWidth[iRing] * (float)Math.Cos( iCount * fRingPoint ), tCenter.Y + faRingWidth[iRing] * (float)Math.Sin( iCount * fRingPoint ));
					_taOpenSpots.Add( tPos );
				}
			}
		}

	// Functions
		public void TakeSpot( ICombatant nSoldier )
		{
			_cUsedSpots.Add( nSoldier.iId, new KeyValuePair<Vector2,ICombatant>( _taOpenSpots[0], nSoldier ));
			_taOpenSpots.RemoveAt( 0 );
		}

		public void FreeSpot( ICombatant nSoldier )
		{
			if( _cUsedSpots.ContainsKey( nSoldier.iId ))
			{
				_taOpenSpots.Add( _cUsedSpots[nSoldier.iId].Key );
				_cUsedSpots.Remove( nSoldier.iId );
			}
		}

		public void ResetSpots( )
		{
			foreach( KeyValuePair<Vector2,ICombatant> tPair in _cUsedSpots.Values )
				_taOpenSpots.Add( tPair.Key );
			_cUsedSpots.Clear( );
		}
	}
}
