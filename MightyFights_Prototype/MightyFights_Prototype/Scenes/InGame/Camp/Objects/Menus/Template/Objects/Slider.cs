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
	public class Slider : ClickableSprite, IUpdate
	{
		AnimationData	_cAnimData;
		int				_iCenter,
						_iLeftPos,
						_iRightPos,
						_iStopLeft,
						_iStopRight,
						_iStartY,
						_iFrameWidth,
						_iGrabPoint = int.MinValue;
		
		bool			_bSelected = false;

		ActionData		_cActionData;

		static int[]	_iaYOffsets = new int[] { 0, 0, -5, -5, -5, -5, -8, -8, -12, -12, -14, -14, -16, -16 },
						_iaBYOffsets = new int[] { 0, 0, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 };

		public int iCenter		{ get { return _iCenter; } set { _iCenter = value; }}
		public int iLeftPos		{ get { return _iLeftPos; } set { _iLeftPos = value; }}
		public int iRightPos	{ get { return _iRightPos; } set { _iRightPos = value; }}
 		public int iStopLeft	{ get { return _iStopLeft; } set { _iStopLeft = value; }}
		public int iStopRight	{ get { return _iStopRight; } set { _iStopRight = value; }}
		public int iStartY		{ get { return _iStartY; } set { _iStartY = value; }}
		public bool bSelected	{ get { return _bSelected; } set { _bSelected = value; _iGrabPoint = int.MinValue; }}
		public int iMaxLeft		{ get; set; }
		public int iMaxRight	{ get; set; }
		public int iLevel		{ get; set; }

		public Slider(AnimationData cAnimData, string sTexRef)
		{
			_cAnimData = cAnimData;
			sTexName = sTexRef;

			_cActionData = _cAnimData.GetActionData("Main", "Sub", sTexName);
			this.cFrame = _cAnimData.GetFrame(_cActionData, 0);
			dlProcessClick = ProcessClick;
		}

		public void SetLevel(int iLevel)
		{
			int iDeltaX,
				iDeltaY;

			this.iLevel = iLevel;
			this.cFrame = _cAnimData.GetFrame(_cActionData, iLevel);

			iDeltaX = this.cFrame.tRect.Width / 2;
			_iFrameWidth = this.cFrame.tRect.Width; 
			if(this.cFrame.bRot) { 
				iDeltaX = this.cFrame.tRect.Height / 2;
				_iFrameWidth = this.cFrame.tRect.Height;
			}

			if(this.sTexName == "top")
				iDeltaY = _iaYOffsets[iLevel];
			else iDeltaY = _iaBYOffsets[iLevel];

			// reset the position to center
			this.tPos = new Vector2(iCenter - iDeltaX, _iStartY + iDeltaY);

			// set the iLeft and iRight stop points
			iDeltaX = cFrame.tRect.Width / 2;

			_iStopRight = _iCenter;//(int)this.tPos.X + _iFrameWidth - (int)(iDeltaX * .2);
			_iStopLeft = _iCenter;////(int)this.tPos.X + (int)(iDeltaX * .2); 
			_iLeftPos = (int)this.tPos.X;
			_iRightPos = _iLeftPos + _iFrameWidth;
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White, cFrame.bRot ? -(float)Math.PI/2 : 0, cFrame.tTopLeft, 1, 
				// and 2: the direction vector
				SpriteEffects.None, fZRange);
		}
	
		#region IUpdate Members

		public void Update(GameTime cTime)
		{
			MouseState cState = Mouse.GetState();

			// are we selected?
			if(_bSelected) {   
				if(_iGrabPoint == int.MinValue)
					_iGrabPoint = cState.X;

				// check the distance from last update to this one for x
				int iDx = cState.X -_iGrabPoint;
				int iXPos = (int)this.tPos.X + iDx,
					iDx1, 
					iDx2;

				if(iDx < 0) { 
					// first case are we near the stop right
					if(iDx + this.tPos.X + _iFrameWidth < _iStopRight) { 
						// is the delta enough to go past max
						if(iDx + this.tPos.X < this.iMaxLeft)  { 
							// get the distance between max and right
							iDx1 = this.iMaxLeft - (iDx + (int)this.tPos.X);
							iDx2 = _iStopRight - (iDx + (int)this.tPos.X + _iFrameWidth);
							// if the distance to right is less than the distnace to max then use it 
							if(iDx1 < iDx2)
								iXPos = _iStopRight - _iFrameWidth;
							else iXPos = this.iMaxLeft;
						// we are not past max but are past right
						} else iXPos = _iStopRight - _iFrameWidth;
					// we are not past right but we are past max
					} else if(iDx + this.tPos.X < this.iMaxLeft)
						iXPos = this.iMaxLeft;
				} else if(iDx > 0) {
					if(iDx + this.tPos.X > _iStopLeft) { 
						if(iDx + this.tPos.X + _iFrameWidth > this.iMaxRight) {
							iDx1 = (iDx + (int)this.tPos.X + _iFrameWidth) - this.iMaxRight;
							iDx2 = (iDx + (int)this.tPos.X) - _iStopLeft;
							if(iDx1 < iDx2) 
								iXPos = _iStopLeft;
							else iXPos = this.iMaxRight - _iFrameWidth;
						} else iXPos = _iStopLeft;
					} else if(iDx + this.tPos.X + _iFrameWidth > this.iMaxRight)
						iXPos = this.iMaxRight - _iFrameWidth;
				} else iXPos = (int)this.tPos.X;

				this.tPos = new Vector2(iXPos, this.tPos.Y);

				_iLeftPos = (int)this.tPos.X;
				_iRightPos = _iLeftPos + _iFrameWidth;

				_iGrabPoint = cState.X;
			} 
		}

		#endregion

		public override bool ContainsPoint(Point tPoint)
		{
			Rectangle tRect;
			if(this.cFrame.bRot) 
				tRect = new Rectangle((int)this.tPos.X, (int)this.tPos.Y, this.cFrame.tRect.Height, this.cFrame.tRect.Width);
			else tRect = new Rectangle((int)this.tPos.X, (int)this.tPos.Y, this.cFrame.tRect.Width, this.cFrame.tRect.Height);

			return tRect.Contains(tPoint);
		}

		void ProcessClick(object oSender, object oArgs)
		{
			++iLevel;
			if(iLevel > 13) iLevel = 0;
			SetLevel(iLevel);
		}
	}
}
