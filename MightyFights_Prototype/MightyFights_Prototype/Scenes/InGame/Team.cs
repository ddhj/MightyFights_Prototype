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

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype	{
	public class Team	{
	// Data
		bool		_bLeftSide;
		Dictionary<int,ICombatant>		_cActiveList = new Dictionary<int,ICombatant>( );
		Dictionary<int,Healer>			_cHealerList = new Dictionary<int,Healer>( );

	// Properties
		public bool bDirection	{ get { return _bLeftSide; }}
		public Dictionary<int,ICombatant>	cActiveList	{ get { return _cActiveList; }}
		public Dictionary<int,Healer>		cHealerList	{ get { return _cHealerList; }}

	// Constructor
		public Team( bool bLeftTeam )
		{
			_bLeftSide = bLeftTeam;
		}

	// Functions

	}
}
