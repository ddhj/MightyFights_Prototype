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

		public string sName { get { return _sName; } set { _sName = value; }}

		public NameTextBox(CampMenuManager cMgr) : base(cMgr){}

		public void KeyDown(object sender, KeyEventArgs vKeyEvt)
		{
			bool	bShift;
			_eKey = vKeyEvt.KeyCode;
			
			// what the fuck is this ?
			if(Enum.GetName(typeof(Keys), _eKey) == null)
				return;

			if(!_bHeld) { 
				KeyboardState cState = Keyboard.GetState();
				cState.GetPressedKeys();

				bShift = cState.IsKeyDown(Keys.LeftShift) || cState.IsKeyDown(Keys.RightShift);

				switch(_eKey) { 
					case Keys.Back:	if(_sName.Length > 0) _sName = _sName.Remove(_sName.Length - 1); break; 
					case Keys.Space: _sName += " "; break;
					case Keys.OemMinus: { 
						if(bShift)	_sName += "_"; 
						else _sName += "-";
					} break;
					default: { 
						// check to see if we are in letterland
						if((int)_eKey > 64 && (int)_eKey < 90) { 
							// are we in a caps or lower case mode
							if(bShift)
								_sName += Enum.GetName(typeof(Keys), _eKey); 
							else _sName += Convert.ToChar((int)_eKey + 32);
						// check to see if we are in number land
						} else if((int)_eKey > 47 && (int)_eKey < 58)
							_sName += Convert.ToChar((int)_eKey);
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

	public class CompanyName : NameTextBox
	{
		BasicSprite		_cIcon;
		
		public CompanyName(CampMenuManager cMgr) : base(cMgr) {}

		public void SetIcon(ClickableSprite cIcon)
		{
			_cIcon = new BasicSprite();
			_cIcon.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(cIcon.sTexName);
			_cIcon.tPos = new Vector2(tPos.X + 26, tPos.Y + 12);
			_cIcon.sTexName = cIcon.sTexName;
			_cIcon.fZRange = .5f;
			_cIcon.cFrame = new Frame(_cIcon.cTexRef.Bounds, 
				new Vector2(_cIcon.cTexRef.Bounds.Width / 2, _cIcon.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), 
				new Vector2(_cIcon.cTexRef.Bounds.Width, _cIcon.cTexRef.Bounds.Height), 
				new Vector2(0, 0), new Vector2(0, 0), null, false, false);
		}

		public void SetIcon(BasicSprite cIcon) 
		{
			_cIcon = new BasicSprite();
			_cIcon.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(cIcon.sTexName);
			_cIcon.tPos = new Vector2(tPos.X + 26, tPos.Y + 12);
			_cIcon.sTexName = cIcon.sTexName;
			_cIcon.fZRange = .5f;
			_cIcon.cFrame = new Frame(_cIcon.cTexRef.Bounds, 
				new Vector2(_cIcon.cTexRef.Bounds.Width / 2, _cIcon.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), 
				new Vector2(_cIcon.cTexRef.Bounds.Width, _cIcon.cTexRef.Bounds.Height), 
				new Vector2(0, 0), new Vector2(0, 0), null, false, false);
		}

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
			if(_cIcon != null)	_cIcon.Draw(cBatch);
		}
	}
}