using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	class BattleGround_Basic : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cBackground;
		SpriteBatch		_cSpriteBatch;
		Dictionary<string, List<IDrawable>>		_cDrawList = new Dictionary<string,List<IDrawable>>();
		List<Trooper>							_cActiveList = new List<Trooper>();
		BasicSprite		_cCursor;
	
		#region IGameScene Members

		public void Update(GameTime cTime)
		{
			List<Trooper>	cRemoveList = new List<Trooper>();

			// set the datastore elapsed time for the action and huristic processing 
			DataStore.cInstance.cTime = cTime;

			foreach(Trooper cTrooper in _cActiveList)
				if(cTrooper.bActive) { 
					cTrooper.Process(cTime);
				} else cRemoveList.Add(cTrooper);

			foreach(Trooper cTrooper in cRemoveList) { 
				// remove from the draw list
				_cDrawList[cTrooper.cTexRef.Name].Remove(cTrooper);

				// remove from the processing list
				_cActiveList.Remove(cTrooper);
			}
		}

		public void Draw(GameTime cTime)
		{
			_cSpriteBatch.Begin(); { 
				_cSpriteBatch.Draw(_cBackground, _cBackground.Bounds, Color.White);

				// draw all the troopers based on their texture 
				foreach(KeyValuePair<string, List<IDrawable>> tTrooperList in _cDrawList)
					foreach(IDrawable nSprite in tTrooperList.Value)
						nSprite.Draw(_cSpriteBatch);

			} _cSpriteBatch.End();
		}

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public bool Init()
		{
			try { 
				ContentManager	cContent = DataStore.cInstance.cContent;
				GraphicsDevice	cGraphics = DataStore.cInstance.cGraphics;
				List<IDrawable>	naDrawList;
				Trooper			cP1;

				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass_large");

				TemplateConfig cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fazure");
				_cActiveList.Add(cP1 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate)));
				cP1.tPos = new Vector2(cGraphics.Viewport.Width / 2 - 100, cGraphics.Viewport.Height / 2 - (int)EConstants.HalberdHight / 2);

				cP1.cActionManager.AddAction(new Action(cP1.Wait, 400, TimeSpan.Zero));
				cP1.cActionManager.AddAction(new Action(cP1.MoveToPoint, new Vector2(100, 100), null));

				if(!_cDrawList.TryGetValue(cP1.cTexRef.Name, out naDrawList))
					_cDrawList.Add(cP1.cTexRef.Name, naDrawList = new List<IDrawable>());

				naDrawList.Add(cP1);

				_cCursor = new BasicSprite();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), null, false, false);

				if(!_cDrawList.TryGetValue(_cCursor.cTexRef.Name, out naDrawList))
					_cDrawList.Add(_cCursor.cTexRef.Name, naDrawList = new List<IDrawable>());

				naDrawList.Add(_cCursor);
			} catch(Exception xEx) { 
				return false;
			}

			return true;
		}

		public void Unload()
		{
			_cBackground.Dispose();
		}

		#endregion
	}
}
