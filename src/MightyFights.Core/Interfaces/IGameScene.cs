using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Linq;

namespace MightyFights_Prototype
{
	public interface IGameScene
	{
		ESceneStates eState { get; set;}
		void Update(GameTime cTime);
		void Draw(GameTime cTime);
		bool Init();
		void Unload();

		//// ddhj: some windows controls are not making me happy
		void ToggleControls();

		//// ddhj: manager for the event driven input
		void RegisterHandlers();
		void UnRegisterHandlers();
	}
}
