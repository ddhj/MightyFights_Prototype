using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace MightyFights_Prototype
{
	class BattleGround_Basic : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cBackground;
		SpriteBatch		_cSpriteBatch;

		//// ddhj just some hack starting things to get it working
		Trooper			_cP1, 
						_cP2;

		#region IGameScene Members

		public void Update(GameTime cTime)
		{
			_cP1.Process(cTime);
			_cP2.Process(cTime);
		}

		public void Draw(GameTime cTime)
		{
			_cSpriteBatch.Begin(); { 
				_cSpriteBatch.Draw(_cBackground, _cBackground.Bounds, Color.White);

				_cP1.Draw(_cSpriteBatch);
				_cP2.Draw(_cSpriteBatch);
			} _cSpriteBatch.End();
		}

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public bool Init()
		{
			try { 
				ContentManager	cContent = DataStore.cInstance.cContent;
				GraphicsDevice	cGraphics = DataStore.cInstance.cGraphics;

				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass_large");

				TemplateConfig cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fazure");
				_cP1 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate));
				_cP1.tPos = new Vector2(cGraphics.Viewport.Width / 2 - 100, cGraphics.Viewport.Height / 2 - _cP1.cFrame.tRect.Height / 2);

				cTemplate = new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsteel");
				_cP2 = new Trooper(ObjectManager.cInstance.CreateTemplate(cTemplate));
				_cP2.tPos = new Vector2(cGraphics.Viewport.Width / 2 + 100, cGraphics.Viewport.Height / 2 - _cP1.cFrame.tRect.Height / 2);
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
