using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Linq;

namespace MightyFights_Prototype
{
	public interface IGameScene
	{
		ESceneStates eState { get; set;}
		void Update(GameTime tTime);
		void Draw(GameTime tTime);
		bool Init();
		void Unload();

		//// dhdj: some windows controls are not making me happy
		void ToggleControls();
	}
}
