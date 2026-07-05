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
		public void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
			InputSystem.KeyDown += new KeyEventHandler(InputSystem_KeyDown_Quit);
			InputSystem.MouseDown += new MouseEventHandler(InputSystem_MouseDown_Quit);

			// add all the clickable guys
			InputSystem.MouseDown += new MouseEventHandler(_nKnight.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nSmith.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nPikard.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nKaijuHunt.MouseDown);
		}

		public void RegisterHandlersMenu()
		{
			// add all the clickable guys
			InputSystem.MouseDown += new MouseEventHandler(_nKnight.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nSmith.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nPikard.MouseDown);
			InputSystem.MouseDown += new MouseEventHandler(_nKaijuHunt.MouseDown);
		}

		void InputSystem_MouseMove(object sender, MouseEventArgs e)
		{
			_cCursor.Update(e.Location);
		}

		//// ddhj: 2026 -- Escape quit-confirm (owner call). Only opens from bare Camp (bNoMenus)
		//// so it can't fight with CampMenuManager's own suppress/restore of the clickable guys;
		//// reuses that exact suppress/restore pair (UnRegisterHandlersMenu/RegisterHandlersMenu)
		//// while the confirm dialog is up.
		void InputSystem_KeyDown_Quit(object oSender, KeyEventArgs eKeyEvt)
		{
			if(eKeyEvt.KeyCode != Keys.Escape)
				return;

			if(_bQuitConfirm) {
				_bQuitConfirm = false;
				RegisterHandlersMenu();
			} else if(_cMenuMgr.bNoMenus) {
				_bQuitConfirm = true;
				UnRegisterHandlersMenu();
			}
		}

		void InputSystem_MouseDown_Quit(object oSender, MouseEventArgs eMouseEvt)
		{
			if(!_bQuitConfirm || eMouseEvt.Button != MouseButton.Left)
				return;

			if(_tQuitYesRect.Contains(eMouseEvt.Location)) {
				_bQuitConfirm = false;
				DataStore.cInstance.cSceneMgr.PopToRoot();
			} else if(_tQuitNoRect.Contains(eMouseEvt.Location)) {
				_bQuitConfirm = false;
				RegisterHandlersMenu();
			}
		}

		public void UnRegisterHandlersMenu()
		{
			InputSystem.MouseDown -= _nKnight.MouseDown;
			InputSystem.MouseDown -= _nSmith.MouseDown;
			InputSystem.MouseDown -= _nPikard.MouseDown;
			InputSystem.MouseDown -= _nKaijuHunt.MouseDown;
		}

		public void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;
			InputSystem.KeyDown -= InputSystem_KeyDown_Quit;
			InputSystem.MouseDown -= InputSystem_MouseDown_Quit;

			// add all the clickable guys
			InputSystem.MouseDown -= _nKnight.MouseDown;
			InputSystem.MouseDown -= _nSmith.MouseDown;
			InputSystem.MouseDown -= _nPikard.MouseDown;
			InputSystem.MouseDown -= _nKaijuHunt.MouseDown;
		}
	}
}