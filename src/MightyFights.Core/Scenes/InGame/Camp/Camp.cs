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
						_cSmithHut,
						_cSupplyHut, 
						_cSurgeonTent,
						_cWagon;

		SpriteBatch		_cSpriteBatch;
		Random			_cRand = DataStore.cInstance.cRand;
		Song			_cBgm;
		ESceneStates	_eState;
		SpriteFont		_cFont;
		Cursor			_cCursor;

		// button guys
		IMouseInteractive	_nKnight,
							_nPikard,
							_nSmith,
							_nKaijuHunt;

		CampObjectManager	_cObjMgr = new CampObjectManager();
		CampMenuManager		_cMenuMgr;

		public CampMenuManager cMenuMgr		{ get { return _cMenuMgr; }}

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			_cObjMgr.Process(cTime, _cMenuMgr.bNoMenus);
			_cMenuMgr.Process(cTime);
		}

		public void Draw(GameTime cTime)
		{
			_cSpriteBatch.Begin(SpriteSortMode.BackToFront, null); { 
				_cSpriteBatch.Draw(_cCampGround, new Vector2(0, 0), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .999f); 
				
				// buildings are going to be in the 8 layer
				_cSpriteBatch.Draw(_cWagon, new Vector2(28, 420), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cBarracks, new Vector2(40, 190), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cSurgeonTent, new Vector2(30, 10), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cSmithHut, new Vector2(270, 17), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cCaptianHouse, new Vector2(580, 15), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				_cSpriteBatch.Draw(_cSupplyHut, new Vector2(580, 290), null, Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, .899f); 
				
				_cObjMgr.Draw(_cSpriteBatch);
				_cMenuMgr.Draw(_cSpriteBatch);

				_cCursor.Draw(_cSpriteBatch);
			} _cSpriteBatch.End();
		}

		public bool Init()
		{
			DataStore		cData = DataStore.cInstance;
			ContentManager	cContent = cData.cContent;
			GraphicsDevice	cGraphics = cData.cGraphics;
			ClickableSprite	cTmpSpr;

			try { 
				_cMenuMgr = new CampMenuManager(this);
				_cSpriteBatch = new SpriteBatch(DataStore.cInstance.cGraphics);
				_cCampGround = cContent.Load<Texture2D>(@"In Game\Camp\ground");
				_cFont = cContent.Load<SpriteFont>(@"Shared\DebugFont");
				_cBgm = DataManager.cInstance.CreateMusic(@"\Camp\wodyn#4");

				// set the object manager parent
				_cObjMgr.cParentData = this;

				// these will all have to be sprites into the object manager when we get the sprite data for them
				_cBarracks = cContent.Load<Texture2D>(@"In Game\Camp\barracks");
				_cCaptianHouse = cContent.Load<Texture2D>(@"In Game\Camp\Captain's_house");
				_cSmithHut = cContent.Load<Texture2D>(@"In Game\Camp\smith");
				_cSupplyHut = cContent.Load<Texture2D>(@"In Game\Camp\supply hut");
				_cSurgeonTent = cContent.Load<Texture2D>(@"In Game\Camp\surgeon_tent");
				_cWagon = cContent.Load<Texture2D>(@"In Game\Camp\wagon");

				// clickable sprites that will bring up and set data into the menu manger
				cTmpSpr = new ClickableKnight(_cMenuMgr);
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\knight_guy");
				cTmpSpr.tPos = new Vector2(148, 310);
				cTmpSpr.sTexName = @"In Game\Camp\KnightMenu\knight_guy";
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(cTmpSpr);
				// set the button object
				_nKnight = (IMouseInteractive)cTmpSpr;

				cTmpSpr = new ClickableSmith(_cMenuMgr);
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\SmithMenu\smith_guy");
				cTmpSpr.tPos = new Vector2(369, 90);
				cTmpSpr.sTexName = @"In Game\Camp\SmithMenu\smith_guy";
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(cTmpSpr);
				_nSmith = (IMouseInteractive)cTmpSpr;

				cTmpSpr = new ClickablePikard(_cMenuMgr);
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\picard_guy");
				cTmpSpr.tPos = new Vector2(642, 196);
				cTmpSpr.sTexName = @"In Game\Camp\picard_guy";
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(cTmpSpr);
				_nPikard = (IMouseInteractive)cTmpSpr;

				//// ddhj: 2026 -- Camp's own path into Kaiju Hunt (see docs/DESIGN_DIRECTION.md).
				//// spy_guy/spy_box were unused camp art, free to repurpose here without a new
				//// art dependency.
				cTmpSpr = new ClickableKaijuHunt(_cMenuMgr);
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\spy_guy");
				cTmpSpr.tPos = new Vector2(90, 460);
				cTmpSpr.sTexName = @"In Game\Camp\spy_guy";
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds,
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2),
					new Vector2(0, 0), new Vector2(0, 0),
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height),
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cObjMgr.AddObject(cTmpSpr);
				_nKaijuHunt = (IMouseInteractive)cTmpSpr;

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