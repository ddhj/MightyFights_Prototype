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
	class FleeSpot : IActive<FleeSpot>	{
	// Data
		int		_iMaxSlots,
				_iMaxHp;
		float	_fHp,
				_fRegenRate;
		Vector2	_tPos;

		List<ICombatant>	_naUsedSlots = new List<ICombatant>( );

	// Properties
		bool bOpen		{ get { return _naUsedSlots.Count < _iMaxSlots; }}
		bool bActive	{ get; set; }
		ActionManager<FleeSpot> cActionManager		{ get; set; }

	// Constructor
		public FleeSpot( int iMaxHp, int iMaxSlots, Vector2 tPos, float fRegSpeed )
		{
			_fHp = _iMaxHp = iMaxHp;
			_iMaxSlots = iMaxSlots;
			_tPos = tPos;
			_fRegenRate = fRegSpeed;
		}

	// Functions
		void Process(GameTime cTime)
		{
		}

		void Get
	}
}
