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
	//// ddhj: Kaiju Hunt demo mode (docs/DESIGN_DIRECTION.md) -- player spawns troopers freely to
	//// bring down one huge monster; survivors bank experience through the existing tally/template
	//// mechanics. This is the scene scaffold: the combat build-out (kaiju Combatant subclass,
	//// zone-exempt targeting, mid-battle spawning, exp banking) lands here next. Right-click
	//// exits, matching the battleground convention.
	public class KaijuHunt : IGameScene
	{
		ESceneStates	_eState;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		SpriteFont		_cFont;
		Cursor			_cCursor;

		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(new Color(12, 8, 24));

			_cBatch.Begin(); {
				Vector2	tSize = _cFont.MeasureString("KAIJU HUNT") * 3f;
				_cBatch.DrawString(_cFont, "KAIJU HUNT", new Vector2(_cGraphics.Viewport.Width / 2 - tSize.X / 2, 200),
					Color.OrangeRed, 0, Vector2.Zero, 3f, SpriteEffects.None, 0);

				string	sBody = "Under construction -- right-click to return";
				tSize = _cFont.MeasureString(sBody);
				_cBatch.DrawString(_cFont, sBody, new Vector2(_cGraphics.Viewport.Width / 2 - tSize.X / 2, 300), Color.White);

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

				_cCursor = new Cursor();
				_cCursor.cTexRef = cData.cContent.Load<Texture2D>(@"Shared\gauntlet_cursor");
				_cCursor.sTexName = @"Shared\gauntlet_cursor00";
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
		}

		void InputSystem_MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			if(eMouseEvt.Button == MouseButton.Right)
				DataStore.cInstance.cSceneMgr.RemoveScene(this);
		}
	}
}
