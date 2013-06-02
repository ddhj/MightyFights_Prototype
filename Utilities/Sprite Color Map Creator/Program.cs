// system includes
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprite_Color_Map_Creator	{
	class Program	{
		static void Main( string[] saArgs )
		{
			if( saArgs.Length == 0 )
			{
				Console.WriteLine( "The proper way to use this is to drag the base colored sprite onto the executable. The other pngs in the folder will be used to generate the map files." );
				return;
			}

			new SpriteComparer( saArgs[0] ).Go( );
		}
	}
}
