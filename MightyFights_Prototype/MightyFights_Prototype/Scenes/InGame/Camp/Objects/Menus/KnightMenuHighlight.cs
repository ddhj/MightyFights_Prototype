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
		bool		_bShowHighlight;
		Frame		_cFrame;
	
		#region IMouseInteractive Members

		public void MouseIn()
		{
			_bShowHighlight = true;
		}

		public void MouseOut()
		{
			_bShowHighlight = false;
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
			if(_bShowHighlight) {
				cBatch.Draw(_cHighlight, tPos, _cFrame.tRect, Color.White, _cFrame.bRot ? -(float)Math.PI/2 : 0, 
					// and 2: the direction vector
					cFrame.tTopLeft, 1, SpriteEffects.None, this.fZRange + .01f);
			}
		}
	}
}