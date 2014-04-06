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
	public class TemplateButton : ClickableSprite, IMouseInteractive
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
			if(ContainsPoint(eMouseEvt.Location))
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
}