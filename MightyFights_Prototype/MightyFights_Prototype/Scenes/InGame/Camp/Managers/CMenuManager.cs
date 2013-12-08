// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Renderers;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class CampMenuManager
	{
		List<ClickableSprite>		_cMenuObjects = new List<ClickableSprite>();

		public List<ClickableSprite>	cMenuObjects	{ get { return _cMenuObjects; }}

		void Process()
		{

		}

		void Draw(SpriteBatch cBatch)
		{

		}
	}
}