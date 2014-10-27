using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class HighlightButton : ClickableSprite, IMouseInteractive
	{
		bool		_bMouseIn = false;
	
		public override void Draw(SpriteBatch cBatch)
		{
			if(_bMouseIn)	
				base.Draw(cBatch);
		}

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			_bMouseIn = ContainsPoint(eMouseEvt.Location);
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			if(_bMouseIn)
				if(dlProcessClick != null)	dlProcessClick(oSender, eMouseEvt);
		}

		public void MouseHover(object oSender, MouseEventArgs eMouseEvt)
		{
		}

		public void MouseWheel(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class MenuButton : ClickableSprite, IMouseInteractive
	{
		bool		_bMouseIn = false;
	
		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			_bMouseIn = ContainsPoint(eMouseEvt.Location);
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			if(_bMouseIn)
				if(dlProcessClick != null)	dlProcessClick(oSender, eMouseEvt);
		}

		public void MouseHover(object oSender, MouseEventArgs eMouseEvt)
		{
		}

		public void MouseWheel(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class MenuControlBase : ClickableSprite, IMouseInteractive
	{
		CampMenuManager		_cMgr; 

		public MenuControlBase(CampMenuManager _cMgr) 
		{

		}

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseHover(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseWheel(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class MenuControlKeyboard : MenuControlBase
	{
		public MenuControlKeyboard(CampMenuManager cMgr) : base(cMgr)
		{
			
		}
	}

	public class NameTextBox : MenuControlBase 
	{
		bool	_bHeld;
		string	_sName = "";
		Keys	_eKey;

		public string sName { get { return _sName; }}

		public NameTextBox(CampMenuManager cMgr) : base(cMgr){}

		public void KeyDown(object sender, KeyEventArgs vKeyEvt)
		{
			_eKey = vKeyEvt.KeyCode;
			
			// what the fuck is this ?
			if(Enum.GetName(typeof(Keys), _eKey) == null)
				return;

			if(!_bHeld) { 
				KeyboardState cState = Keyboard.GetState();
				cState.GetPressedKeys();

				switch(_eKey) { 
					case Keys.Back:	if(_sName.Length > 0) _sName = _sName.Remove(_sName.Length - 1); break; 
					default: { 
						// are we in a caps or lower case mode
						if(cState.IsKeyDown(Keys.LeftShift) || cState.IsKeyDown(Keys.RightShift))
							_sName += Enum.GetName(typeof(Keys), _eKey); 
						else _sName += Convert.ToChar((int)_eKey + 32);
					} break;
				}
				
				_bHeld = true;
			}
		}

		public void KeyUp(object sender, KeyEventArgs vKeyEvt)
		{
			_bHeld = false;
		}
	}
}