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
		Texture2D		_cCampGround,
						// all the buildings will have to be sprites eventually
						_cCaptianHouse,
						_cBarracks,
						_cSmith,
						_cSupplyHut, 
						_cSurgeonTent,
						_cWagon;

		ClickableSprite	_cSmithSprite,
						_cCaptain,
						_cTheif,
						_cKnight;

		SpriteBatch		_cSpriteBatch;
		Random			_cRand = DataStore.cInstance.cRand;
		Song			_cBgm;
		ESceneStates	_eState;
		SpriteFont		_cFont;
		Cursor			_cCursor;

		SoundEffectInstance		_cBuffSfx,
								_cHammerSfx;

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			_cCursor.Update(cTime);
		}

		public void Draw(GameTime cTime)
		{
			_cSpriteBatch.Begin(SpriteSortMode.BackToFront, null); { 
				_cSpriteBatch.Draw(_cCampGround, new Vector2(0, 0), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .999f); 
				
				// buildings are going to be in the 8 layer
				_cSpriteBatch.Draw(_cWagon, new Vector2(28, 420), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cBarracks, new Vector2(40, 190), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cSurgeonTent, new Vector2(30, 10), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cSmith, new Vector2(270, 17), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cCaptianHouse, new Vector2(580, 15), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cSupplyHut, new Vector2(580, 290), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				
				
				
				_cCursor.Draw(_cSpriteBatch);
			} _cSpriteBatch.End();
		}

		public bool Init()
		{
			DataStore		cData = DataStore.cInstance;
			ContentManager	cContent = cData.cContent;
			GraphicsDevice	cGraphics = cData.cGraphics;
			string[]		saMusic = new string[] {"05 We Are the Fugitives",
				"08 Any City", "12 Nin-Nin Hall", "Chrono_Trigger_Another_Fair_OC_ReMix",
				"Chrono_Trigger_IslandOfZeal_OC_ReMix", "Chrono_Trigger_Millenial_Fair_2001_OC_ReMix",
				"Chrono_Trigger_New_Zeal_OC_ReMix", "Chrono_Trigger_Tears_for_a_Girl_OC_ReMix",
				"Chrono_Trigger_Time_Management_OC_ReMix", "Chrono_Trigger_Zeal_Love_OC_ReMix",
				"Phantasy_Star_3_Legacy_OC_ReMix", "Phantasy_Star_3_Nial_and_Nowhere_OC_ReMix",
				"[Actraiser] Yuzo Koshiro - Birth of the People (arranged)",
				"[Actraiser] Yuzo Koshiro - Filmoa (arranged)"};

			try { 
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cCampGround = cContent.Load<Texture2D>(@"In Game\Camp\ground");
				_cFont = cContent.Load<SpriteFont>(@"Shared\DebugFont");
				_cBgm = DataManager.cInstance.CreateMusic(@"\Camp\" + saMusic[_cRand.Next(saMusic.Length)]);

				// these will all have to be sprites into the object manager when we get the sprite data for them
				_cBarracks = cContent.Load<Texture2D>(@"In Game\Camp\barracks");
				_cCaptianHouse = cContent.Load<Texture2D>(@"In Game\Camp\Captain's_house");
				_cSmith = cContent.Load<Texture2D>(@"In Game\Camp\smith");
				_cSupplyHut = cContent.Load<Texture2D>(@"In Game\Camp\supply hut");
				_cSurgeonTent = cContent.Load<Texture2D>(@"In Game\Camp\surgeon_tent");
				_cWagon = cContent.Load<Texture2D>(@"In Game\Camp\wagon");

				_cCursor = new Cursor();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\gauntlet_cursor");
				_cCursor.sTexName = @"Shared\gauntlet_cursor00";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				MediaPlayer.IsRepeating = true;
				MediaPlayer.Volume = .6f;
				if(DataStore.cInstance.bPlayMusic)	MediaPlayer.Play( _cBgm );
			} catch { 
				return false;
			}

			return true;
		}

		public void Unload()
		{
			
		}

		public void ToggleControls()
		{
			
		}

		#endregion
	}
}