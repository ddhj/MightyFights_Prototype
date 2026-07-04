// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 direction change (see docs/DESIGN_DIRECTION.md) -- the title's Play Game now
	//// lands here instead of going straight to Camp. Each entry is a game mode built on the
	//// combat core: Skirmish is the classic Camp -> Battleground loop, Kaiju Hunt is the new
	//// demo mode. Spritefont-rendered so it needs no new art.
	public class ModeSelect : IGameScene
	{
		ESceneStates	_eState;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		SpriteFont		_cFont;
		Cursor			_cCursor;

		string[]		_saEntries = new string[] { "Skirmish", "Kaiju Hunt", "Back to Title" };
		Rectangle[]		_caEntryRects;
		int				_iHovered = -1;

		const int		iFirstEntryY = 260;
		const int		iEntrySpacing = 56;
		const float		fEntryScale = 2f;

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Color.Black);

			_cBatch.Begin(); {
				Vector2	tSize = _cFont.MeasureString("Select Mode") * fEntryScale;
				_cBatch.DrawString(_cFont, "Select Mode", new Vector2(_cGraphics.Viewport.Width / 2 - tSize.X / 2, 160),
					Color.LightGray, 0, Vector2.Zero, fEntryScale, SpriteEffects.None, 0);

				for(int iCount = 0; iCount < _saEntries.Length; ++iCount)
					_cBatch.DrawString(_cFont, _saEntries[iCount],
						new Vector2(_caEntryRects[iCount].X, _caEntryRects[iCount].Y),
						iCount == _iHovered ? Color.Gold : Color.White,
						0, Vector2.Zero, fEntryScale, SpriteEffects.None, 0);

				_cCursor.Draw(_cBatch);
			} _cBatch.End();
		}

		public bool Init()
		{
			try {
				DataStore	cData = DataStore.cInstance;

				_cGraphics = cData.cGraphics;
				_cFont = cData.cFont;
				_cBatch = new SpriteBatch(_cGraphics);

				// build the clickable text rects, centered
				_caEntryRects = new Rectangle[_saEntries.Length];
				for(int iCount = 0; iCount < _saEntries.Length; ++iCount) {
					Vector2	tSize = _cFont.MeasureString(_saEntries[iCount]) * fEntryScale;
					_caEntryRects[iCount] = new Rectangle(
						(int)(_cGraphics.Viewport.Width / 2 - tSize.X / 2),
						iFirstEntryY + iCount * iEntrySpacing,
						(int)tSize.X, (int)tSize.Y);
				}

				_cCursor = new Cursor();
				_cCursor.cTexRef = cData.cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2),
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			} catch(Exception xEx) {
				System.Diagnostics.Debug.WriteLine(xEx.ToString());
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

		public void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
			InputSystem.MouseDown += new MouseEventHandler(InputSystem_MouseDown);
		}

		public void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;
			InputSystem.MouseDown -= InputSystem_MouseDown;
		}

		#endregion

		void InputSystem_MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			_cCursor.Update(eMouseEvt.Location);

			_iHovered = -1;
			for(int iCount = 0; iCount < _caEntryRects.Length; ++iCount)
				if(_caEntryRects[iCount].Contains(eMouseEvt.Location))
					_iHovered = iCount;
		}

		void InputSystem_MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			for(int iCount = 0; iCount < _caEntryRects.Length; ++iCount) {
				if(!_caEntryRects[iCount].Contains(eMouseEvt.Location))
					continue;

				switch(iCount) {
					case 0: {	// Skirmish -- the classic Camp -> Battleground loop
						IGameScene	nScene = new Camp();
						if(nScene.Init())
							DataStore.cInstance.cSceneMgr.AddScene(nScene);
					} break;

					case 1: {	// Kaiju Hunt -- new demo mode
						IGameScene	nScene = new KaijuHunt();
						if(nScene.Init())
							DataStore.cInstance.cSceneMgr.AddScene(nScene);
					} break;

					case 2:		// back to title
						DataStore.cInstance.cSceneMgr.RemoveScene(this);
					break;
				}
				return;
			}
		}
	}
}
