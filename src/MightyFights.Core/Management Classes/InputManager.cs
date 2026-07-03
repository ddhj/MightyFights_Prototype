using System;

namespace Microsoft.Xna.Framework.Input
{
	public class CharacterEventArgs : EventArgs
	{
		private readonly char character;
		private readonly int lParam;

		public CharacterEventArgs(char character, int lParam)
		{
			this.character = character;
			this.lParam = lParam;
		}

		public char Character	{ get { return character; }}
		public int Param		{ get { return lParam; }}
		public int RepeatCount	{ get { return lParam & 0xffff; }}
		public bool ExtendedKey { get { return (lParam & (1 << 24)) > 0; }}
		public bool AltPressed	{ get { return (lParam & (1 << 29)) > 0; }}
		public bool PreviousState	{ get { return (lParam & (1 << 30)) > 0; }}
		public bool TransitionState	{ get { return (lParam & (1 << 31)) > 0; }}
	}

	public class KeyEventArgs : EventArgs
	{
		private Keys keyCode;

		public KeyEventArgs(Keys keyCode)
		{
			this.keyCode = keyCode;
		}

		public Keys KeyCode		{ get { return keyCode; }}
	}

	public class MouseEventArgs : EventArgs
	{
		private MouseButton button;
		private int clicks;
		private int x;
		private int y;
		private int delta;

		public MouseButton Button { get { return button;         } }
		public int Clicks         { get { return clicks;         } }
		public int X              { get { return x;              } }
		public int Y              { get { return y;              } }
		public Point Location     { get { return new Point(x,y); } }
		public int Delta          { get { return delta;          } }

		public MouseEventArgs(MouseButton button, int clicks, int x, int y, int delta)
		{
			this.button = button;
			this.clicks = clicks;
			this.x = x;
			this.y = y;
			this.delta = delta;
		}
	}

	public delegate void CharEnteredHandler(object sender, CharacterEventArgs e);
	public delegate void KeyEventHandler(object sender, KeyEventArgs e);

	public delegate void MouseEventHandler(object sender, MouseEventArgs e);

	/// <summary>
	/// Mouse Key Flags from WinUser.h for mouse related WM messages.
	/// </summary>
	[Flags]
	public enum MouseKeys
	{
		LButton =  0x01,
		RButton =  0x02,
		Shift =    0x04,
		Control =  0x08,
		MButton =  0x10,
		XButton1 = 0x20,
		XButton2 = 0x40
	}

	public enum MouseButton
	{
		None, Left, Right, Middle, X1, X2
	}

	/// <summary>
	/// Higher-level input event source for the game.
	///
	/// PORT (Phase 6): the original XNA build raised these events from a custom Win32 WndProc hook
	/// (SetWindowLong/CallWindowProc/Imm32) -- a pre-engine, Windows-only approach that broke under
	/// the net8.0/x64 SDL2 backend (the 64-bit WndProc pointer was truncated to int, so the subclass
	/// silently failed and ALL input was dead) and would not exist at all on Linux/web. Reimplemented
	/// on top of MonoGame's cross-platform Mouse/Keyboard polling, pumped once per frame from
	/// GameShell.Update via InputSystem.Update(). The public event API is unchanged, so scene input
	/// handlers did not need to change. See PORT_NOTES.md Phase 6.
	/// </summary>
	public static class InputSystem
	{
		#region Events
		/// <summary>Event raised when a character has been entered (cross-platform, via GameWindow.TextInput).</summary>
		public static event CharEnteredHandler CharEntered;

		/// <summary>Event raised when a key has been pressed down.</summary>
		public static event KeyEventHandler KeyDown;

		/// <summary>Event raised when a key has been released.</summary>
		public static event KeyEventHandler KeyUp;

		/// <summary>Event raised when a mouse button is pressed.</summary>
		public static event MouseEventHandler MouseDown;

		/// <summary>Event raised when a mouse button is released.</summary>
		public static event MouseEventHandler MouseUp;

		/// <summary>Event raised when the mouse changes location.</summary>
		public static event MouseEventHandler MouseMove;

		/// <summary>Event raised when the mouse has hovered in the same location for a short period of time.</summary>
		public static event MouseEventHandler MouseHover;

		/// <summary>Event raised when the mouse wheel has been moved.</summary>
		public static event MouseEventHandler MouseWheel;

		/// <summary>Event raised when a mouse button has been double clicked.</summary>
		public static event MouseEventHandler MouseDoubleClick;
		#endregion

		#region Poll State
		static bool				initialized;
		static bool				bHasPrev;
		static MouseState		cPrevMouse;
		static KeyboardState	cPrevKeys;

		// hover tracking: fire MouseHover once after the pointer sits still for HoverMs
		const double			HoverMs = 500;
		static double			fHoverTimer;
		static bool				bHoverFired;

		// double-click tracking (Left button)
		const double			DblClickMs = 400;
		const int				DblClickSlop = 3;
		static double			fSinceLastLeftDown = double.MaxValue;
		static Point			tLastLeftDown;
		#endregion

		public static Point MouseLocation
		{
			get
			{
				MouseState state = Mouse.GetState();
				return new Point(state.X, state.Y);
			}
		}

		public static bool ShiftDown
		{
			get
			{
				KeyboardState state = Keyboard.GetState();
				return state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
			}
		}

		public static bool CtrlDown
		{
			get
			{
				KeyboardState state = Keyboard.GetState();
				return state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl);
			}
		}

		public static bool AltDown
		{
			get
			{
				KeyboardState state = Keyboard.GetState();
				return state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt);
			}
		}

		/// <summary>
		/// Wire up character input. Kept the original signature so GameShell's call site is unchanged;
		/// GameWindow.TextInput is MonoGame's cross-platform replacement for the old WM_CHAR path.
		/// </summary>
		public static void Initialize(GameWindow window)
		{
			if (initialized)
				throw new InvalidOperationException("InputSystem.Initialize can only be called once!");

			if (window != null)
				window.TextInput += (sender, e) => { if (CharEntered != null) CharEntered(null, new CharacterEventArgs(e.Character, 0)); };

			initialized = true;
		}

		/// <summary>
		/// Poll mouse/keyboard once per frame and raise the higher-level events. Call from GameShell.Update.
		/// </summary>
		public static void Update(GameTime cTime)
		{
			double fElapsed = cTime != null ? cTime.ElapsedGameTime.TotalMilliseconds : 0;

			MouseState cMouse = Mouse.GetState();
			int iX = cMouse.X, iY = cMouse.Y;

			if (!bHasPrev)
			{
				// first frame: establish a baseline, don't synthesize transitions against garbage
				cPrevMouse = cMouse;
				cPrevKeys = Keyboard.GetState();
				bHasPrev = true;
				return;
			}

			// ---- movement + hover ----
			if (iX != cPrevMouse.X || iY != cPrevMouse.Y)
			{
				if (MouseMove != null) MouseMove(null, new MouseEventArgs(MouseButton.None, 0, iX, iY, 0));
				fHoverTimer = 0; bHoverFired = false;
			}
			else
			{
				fHoverTimer += fElapsed;
				if (!bHoverFired && fHoverTimer >= HoverMs)
				{
					bHoverFired = true;
					if (MouseHover != null) MouseHover(null, new MouseEventArgs(MouseButton.None, 0, iX, iY, 0));
				}
			}

			// ---- buttons ----
			PollButton(MouseButton.Left, cPrevMouse.LeftButton, cMouse.LeftButton, iX, iY, fElapsed);
			PollButton(MouseButton.Right, cPrevMouse.RightButton, cMouse.RightButton, iX, iY, fElapsed);
			PollButton(MouseButton.Middle, cPrevMouse.MiddleButton, cMouse.MiddleButton, iX, iY, fElapsed);
			fSinceLastLeftDown += fElapsed;

			// ---- wheel ----
			int iWheelDelta = cMouse.ScrollWheelValue - cPrevMouse.ScrollWheelValue;
			if (iWheelDelta != 0 && MouseWheel != null)
				MouseWheel(null, new MouseEventArgs(MouseButton.None, 0, iX, iY, iWheelDelta / 120));

			// ---- keyboard ----
			KeyboardState cKeys = Keyboard.GetState();
			if (KeyDown != null)
				foreach (Keys eKey in cKeys.GetPressedKeys())
					if (cPrevKeys.IsKeyUp(eKey)) KeyDown(null, new KeyEventArgs(eKey));
			if (KeyUp != null)
				foreach (Keys eKey in cPrevKeys.GetPressedKeys())
					if (cKeys.IsKeyUp(eKey)) KeyUp(null, new KeyEventArgs(eKey));

			cPrevMouse = cMouse;
			cPrevKeys = cKeys;
		}

		static void PollButton(MouseButton eButton, ButtonState ePrev, ButtonState eNow, int iX, int iY, double fElapsed)
		{
			if (ePrev == ButtonState.Released && eNow == ButtonState.Pressed)
			{
				if (MouseDown != null) MouseDown(null, new MouseEventArgs(eButton, 1, iX, iY, 0));

				if (eButton == MouseButton.Left)
				{
					if (fSinceLastLeftDown <= DblClickMs
						&& Math.Abs(iX - tLastLeftDown.X) <= DblClickSlop
						&& Math.Abs(iY - tLastLeftDown.Y) <= DblClickSlop)
					{
						if (MouseDoubleClick != null) MouseDoubleClick(null, new MouseEventArgs(eButton, 2, iX, iY, 0));
					}
					fSinceLastLeftDown = 0;
					tLastLeftDown = new Point(iX, iY);
				}
			}
			else if (ePrev == ButtonState.Pressed && eNow == ButtonState.Released)
			{
				if (MouseUp != null) MouseUp(null, new MouseEventArgs(eButton, 1, iX, iY, 0));
			}
		}
	}
}
