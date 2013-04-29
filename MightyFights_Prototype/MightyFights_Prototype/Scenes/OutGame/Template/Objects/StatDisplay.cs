using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class StatDisplay : BasicSprite
	{
		AnimationData	_cAnimData;
		ActionData		_cActionData;
		int				_iIncrement,
						_iCurFrame;
		
		public bool	bLeft	{ get; set; }
		public int iCurFrame { get { return _iCurFrame; } set { _iCurFrame = value; }}

		public StatDisplay(AnimationData cData, string sTexRefName) : base()
		{
			_cAnimData = cData;
			this.sTexName = sTexRefName;

			_cActionData = _cAnimData.GetActionData("Main", "Sub", this.sTexName);
			this.cFrame = _cAnimData.GetFrame(_cActionData, 0);
			if(this.cFrame.bRot)
				_iIncrement = this.cFrame.tRect.Height / 14;
			else _iIncrement = this.cFrame.tRect.Width / 14;
		}

		public void SetDisplay(int iPosition)
		{
			int iPos = iPosition - (int)this.tPos.X;
			int iWidth = this.cFrame.tRect.Width;

			if(this.cFrame.bRot)
				iWidth = this.cFrame.tRect.Height;

			if(bLeft)
				iPos = iWidth - iPos;

			iPos = iPos / _iIncrement;
			if(iPos >= _cActionData.iMaxFrames)
				iPos = _cActionData.iMaxFrames - 1;
			
			_iCurFrame = iPos;
			
			cFrame = _cAnimData.GetFrame(_cActionData, iPos);
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White, cFrame.bRot ? -(float)Math.PI/2 : 0, cFrame.tTopLeft, 1, 
				// and 2: the direction vector
				SpriteEffects.None, 1);
		}
	}
}
