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

		#region IGameScene Members

		public void Update(GameTime cTime)
		{
			
		}

		public void Draw(GameTime cTime)
		{
			_cSpriteBatch.Begin();
				_cSpriteBatch.Draw(_cBackground, _cBackground.Bounds, Color.White);
			_cSpriteBatch.End();
		}

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public bool Init()
		{
			try { 
				ContentManager	cContent = DataStore.cInstance.cContent;
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cBackground = cContent.Load<Texture2D>(@"Backgrounds\dirt_grass_large");
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
