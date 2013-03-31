using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class MainMenu : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cCardP1, 
						_cCardP2,
						_cToBattle,
						_cTrooperTex;
		AnimationData	_cTroopers;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		
		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Color.Black);

			_cBatch.Begin();
				_cBatch.Draw(_cCardP1, new Vector2(300, 300), _cCardP1.Bounds, Color.White);
				_cBatch.Draw(_cCardP2, new Vector2(500, 300), _cCardP2.Bounds, Color.White);
				_cBatch.Draw(_cToBattle, new Vector2(300, 500), _cToBattle.Bounds, Color.White);
			_cBatch.End();
		}

		public bool Init()
		{
			try { 
				ContentManager cContent = DataStore.cInstance.cContent;
				_cGraphics = DataStore.cInstance.cGraphics;

				_cCardP1 = cContent.Load<Texture2D>(@"Out Game\Main Menu\green_card");
				_cCardP2 = cContent.Load<Texture2D>(@"Out Game\Main Menu\red_card");
				_cToBattle = cContent.Load<Texture2D>(@"Out Game\Main Menu\to_battle!-1");
				_cTrooperTex = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\Guys");
			
				_cTroopers = cContent.Load<AnimationData>(@"Sprite Data\Troopers\Halberd\Front Facing\GuysArray");

				_cBatch = new SpriteBatch(DataStore.cInstance.cGraphics);

				return true;
			} catch(Exception xEx) { 
				return false;
			}
		}

		public void Unload()
		{
			_cCardP1.Dispose();
			_cCardP2.Dispose();
			_cToBattle.Dispose();
			_cTrooperTex.Dispose();
			
			_cTroopers.Dispose();
		}

		#endregion
	}
}
