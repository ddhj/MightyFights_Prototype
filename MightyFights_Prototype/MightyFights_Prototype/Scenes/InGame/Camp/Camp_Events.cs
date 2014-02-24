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
			_cObjMgr.RegisterEvents();
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
		}

		void InputSystem_MouseMove(object sender, MouseEventArgs e)
		{
			_cCursor.Update(e.Location);
		}

		public void UnRegisterHandlers()
		{

		}
	}
}