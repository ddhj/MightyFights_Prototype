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
		Texture2D	_cHighlight; 
		bool		_bMouseIn = false;
		Frame		_cFrame;
		
		public DProcessMouseEvent dlMouseOut	{ get; set; }
		public DProcessMouseEvent dlMouseIn		{ get; set; }

		#region IMouseInteractive Members

		public void MouseIn()
		{
			if(!_bMouseIn) { 
				_bMouseIn = true;
				if(dlMouseIn != null)	dlMouseIn(this);
			}
		}

		public void MouseOut()
		{
			if(_bMouseIn) { 
				_bMouseIn = false;
				if(dlMouseOut != null)	dlMouseOut(this);
			}
		}

		public bool MouseMove(MouseState tMouseState)
		{			
			Point	tPoint = new Point(tMouseState.X, tMouseState.Y);
			if(ContainsPoint(tPoint)) { 
				MouseIn();
				return true;
			} else MouseOut();

			return false;
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			if(_bMouseIn)	
				base.Draw(cBatch);
		}
	}
}