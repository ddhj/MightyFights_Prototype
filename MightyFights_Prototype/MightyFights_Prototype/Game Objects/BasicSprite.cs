using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class ClickableSprite : IDrawable, IDrawableTexture, IClickable, IObject
	{
		Rectangle	_cDrawnRect;
		Vector2		_tPos;
		Texture2D	_cTexRef;

		public int iId				{ get; set; }
		public EObjectStates eObjState	{ get; set; }
		public Texture2D cTexRef	{ get { return _cTexRef; } set { _cTexRef = value; }}
		public Frame cFrame			{ get; set; }
		public string sTexName		{ get; set; }
		public Vector2 tPos			{ get { return _tPos; } 
			set {
				_tPos = value; 
				_cDrawnRect = new Rectangle((int)_tPos.X, (int)_tPos.Y, _cTexRef.Bounds.Width, _cTexRef.Bounds.Height);
			}
		}

		public ClickableSprite()
		{
			this.eObjState = EObjectStates.Draw;
			this.iId = ObjectManager.cInstance.iCurObjId;
		}

		public virtual bool ContainsPoint(Point tPoint) 
		{
			return _cDrawnRect.Contains(tPoint);
		}

		public virtual bool ContainsPoint(Vector2 tLoc)
		{
			return ContainsPoint(new Point((int)tLoc.X, (int)tLoc.Y));
		}

		public virtual void Draw(SpriteBatch cBatch) 
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White);
		}

		public void Dispose()
		{
			_cTexRef.Dispose();
		}
	}

	public class BasicSprite : IDrawable, IDrawableTexture, IObject
	{
		public Texture2D cTexRef			{ get; set; }
		public int iId						{ get; set; }
		public EObjectStates eObjState		{ get; set; }
		public Frame cFrame					{ get; set; }
		public string sTexName				{ get; set; }
		public Vector2 tPos					{ get; set; }

		public BasicSprite()
		{
			this.eObjState = EObjectStates.Draw;
			this.iId = ObjectManager.cInstance.iCurObjId;
		}

		public virtual void Draw(SpriteBatch cBatch) 
		{
			cBatch.Draw(this.cTexRef, this.tPos, this.cFrame.tRect, Color.White, this.cFrame.bRot ? -(float)Math.PI/2 : 0, 
				// and 2: the direction vector
				this.cFrame.tTopLeft, 1, SpriteEffects.None, .99f);
		}
	}
}
