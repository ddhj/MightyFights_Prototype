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

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public partial class Camp : IGameScene
	{
		public void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);

			// add all the clickable guys
			InputSystem.MouseDown += new MouseEventHandler(_nKnight.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nSmith.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nPikard.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nKaijuHunt.MouseDown);
		}

		public void RegisterHandlersMenu()
		{
			// add all the clickable guys
			InputSystem.MouseDown += new MouseEventHandler(_nKnight.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nSmith.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nPikard.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nKaijuHunt.MouseDown);
		}

		void InputSystem_MouseMove(object sender, MouseEventArgs e)
		{
			_cCursor.Update(e.Location);
		}

		public void UnRegisterHandlersMenu()
		{
			InputSystem.MouseDown -= _nKnight.MouseDown;
			InputSystem.MouseDown -= _nSmith.MouseDown;
			InputSystem.MouseDown -= _nPikard.MouseDown;
			InputSystem.MouseDown -= _nKaijuHunt.MouseDown;
		}

		public void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;

			// add all the clickable guys
			InputSystem.MouseDown -= _nKnight.MouseDown;
			InputSystem.MouseDown -= _nSmith.MouseDown;
			InputSystem.MouseDown -= _nPikard.MouseDown;
			InputSystem.MouseDown -= _nKaijuHunt.MouseDown;
		}
	}
}