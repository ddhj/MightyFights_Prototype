using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MightyFights_Prototype
{
	public class SceneManager : DrawableGameComponent
	{
		List<IGameScene>	_naSceneList = new List<IGameScene>();

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
				_naSceneList[_naSceneList.Count - 2].UnRegisterHandlers();
			}

			nGameScene.RegisterHandlers();
		}

		public void RemoveScene(IGameScene nGameScene)
		{
			//// ddhj: 2026 -- AddScene unregisters the handlers of the scene it covers, but this
			//// never unregistered the handlers of the scene being removed. A removed scene's
			//// input handlers (e.g. a battle's own MouseDown) stayed subscribed forever, and since
			//// BattlegroundData.Clear() never resets eState back off Victory, a left battle's
			//// stale handler permanently matches its own "Victory + right-click" withdraw check on
			//// every future click anywhere, re-triggering RemoveScene/RegisterHandlers on whatever
			//// is currently on top. Harmless the first time any given scene type was only ever
			//// entered once, but re-entering the same reentrant scene (Camp <-> Kaiju Hunt) leaks
			//// one more of these every round trip and snowballs.
			nGameScene.UnRegisterHandlers();
			nGameScene.Unload();
			// this could be remove last or pop scene rather than a search
			_naSceneList.Remove(nGameScene);

			// make sure the last one in the list is active
			_naSceneList[_naSceneList.Count - 1].eState = ESceneStates.Active;
			_naSceneList[_naSceneList.Count - 1].ToggleControls();
			_naSceneList[_naSceneList.Count - 1].RegisterHandlers();
		}

		//// ddhj: 2026 -- "quit to title" from deep in the stack (e.g. Camp, itself several
		//// pushes below Title -> ModeSelect -> Camp) needs to unwind every scene above the root
		//// in one shot, with the same per-scene cleanup RemoveScene already does, rather than the
		//// caller needing a reference to each intermediate scene.
		public void PopToRoot()
		{
			while(_naSceneList.Count > 1) {
				IGameScene	cTop = _naSceneList[_naSceneList.Count - 1];
				cTop.UnRegisterHandlers();
				cTop.Unload();
				_naSceneList.RemoveAt(_naSceneList.Count - 1);
			}

			if(_naSceneList.Count > 0) {
				_naSceneList[0].eState = ESceneStates.Active;
				_naSceneList[0].ToggleControls();
				_naSceneList[0].RegisterHandlers();
			}
		}
	}
}
