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
	class Team	{
		Dictionary<int,ICombatant>		_cActiveList = new Dictionary<int,ICombatant>( );
		List<FleeSpot>					_caSpots = new List<FleeSpot>( );
	}
}
