using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class BasicSprite : IDrawable, IClickable
	{
		Rectangle	_cDrawnRect;
		Vector2		_tPos;
		Texture2D	_cTexRef;

		public Texture2D cTexRef { get { return _cTexRef; } set { _cTexRef = value; }}
		public Frame cFrame { get; set; }
		public Vector2 tPos { get { return _tPos; } 
			set {
				_tPos = value; 
				_cDrawnRect = new Rectangle((int)_tPos.X, (int)_tPos.Y, _cTexRef.Bounds.Width, _cTexRef.Bounds.Height);
			}
		}

		public bool ContainsPoint(Point tPoint) 
		{
			return _cDrawnRect.Contains(tPoint);
		}
		
		public void Draw(SpriteBatch cBatch) 
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White);
		}

		public void Dispose()
		{
			_cTexRef.Dispose();
		}
	}
}
