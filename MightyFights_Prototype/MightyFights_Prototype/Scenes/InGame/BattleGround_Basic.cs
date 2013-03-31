using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	class BattleGround_Basic : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cBackground;
		SpriteBatch		_cSpriteBatch;

		public BattleGround_Basic(string sBackground)
		{
			DataStore.cInstance.cContent.Load<Texture2D>("dirt_grass_large");
		}

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
			return true;
		}

		public void Unload()
		{
			_cBackground.Dispose();
		}

		#endregion
	}
}
