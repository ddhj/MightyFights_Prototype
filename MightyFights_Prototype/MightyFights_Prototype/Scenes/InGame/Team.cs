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
		int		_iId;
		bool	_bLeftSide;
		Dictionary<int,ICombatant>		_cActiveList = new Dictionary<int,ICombatant>( );
		Dictionary<int,Priest>			_cHealerList = new Dictionary<int,Priest>( );
		List<ICombatant>				_cMembers = new List<ICombatant>();

	// Properties
		public int iId			{ get { return _iId; }}
		public bool bDirection	{ get { return _bLeftSide; }}
		public Dictionary<int,ICombatant>	cActiveList	{ get { return _cActiveList; }}
		public Dictionary<int,Priest>		cHealerList	{ get { return _cHealerList; }}
		public List<ICombatant> cMembers { get { return _cMembers; }}

	// Constructor
		public Team( int iId, bool bLeftTeam )
		{
			_iId = iId;
			_bLeftSide = bLeftTeam;
		}

	// Functions
		public void Clear( )
		{
			_cActiveList.Clear( );
			_cHealerList.Clear( );
		}
	}
}
