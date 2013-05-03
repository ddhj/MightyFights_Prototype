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
	public class SceneManager : DrawableGameComponent
	{
		List<IGameScene>	_naSceneList = new List<IGameScene>();
		Game				_cGameObj;

		public SceneManager(Game cGame) : base(cGame) {}

		public override void Initialize()
		{
			// set up the driver class data for the data store
			DataStore.cInstance.cContent = Game.Content;
			DataStore.cInstance.cGraphics = Game.GraphicsDevice;
			DataStore.cInstance.cSceneMgr = this;

			// 

			// likely do some other data store stuff if we need to pass... not sure if that is needed here or not 
		}

		protected override void LoadContent()
		{
			// not sure that there is going to be any loading or unloading of data in the system at the manager stage
		}

		protected override void UnloadContent()
		{

		}

		public override void Update(GameTime gameTime)
		{
			if(_naSceneList.Count > 0) 
				_naSceneList[_naSceneList.Count - 1].Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if(_naSceneList.Count > 0)	
				_naSceneList[_naSceneList.Count - 1].Draw(gameTime);
		}

		public void AddScene(IGameScene nGameScene)
		{
			nGameScene.eState = ESceneStates.Active;
			_naSceneList.Add(nGameScene);
			if(_naSceneList.Count > 1) { 
				_naSceneList[_naSceneList.Count - 2].eState = ESceneStates.Inactive;
				_naSceneList[_naSceneList.Count - 2].ToggleControls();
			}
		}

		public void RemoveScene(IGameScene nGameScene)
		{
			nGameScene.Unload();
			// this could be remove last or pop scene rather than a search
			_naSceneList.Remove(nGameScene);

			// make sure the last one in the list is active
			_naSceneList[_naSceneList.Count - 1].eState = ESceneStates.Active;
			_naSceneList[_naSceneList.Count - 1].ToggleControls();
		}
	}
}
