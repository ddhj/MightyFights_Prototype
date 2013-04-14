using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class AnimationProcessor
	{
		AnimationData	_cAnimDataRef; 
		Frame			_cCurFrame;
		ActionData		_cCurAction;
		int				_iCount, 
						_iItteration,
						_iCurIncrement,
						_iCurFrameIdx,
						_iCurFrame;
		TimeSpan		_tTime;

		Dictionary<string, int>		_cActionIncrement = new Dictionary<string,int>();

		public Frame cCurFrame	{ get { return _cCurFrame; }}
		public bool	bActive { get; set; }

		public AnimationProcessor(AnimationData cAnimData, TemplateConfig cTemplateCfg)
		{
			_cAnimDataRef = cAnimData;

		}

		public void SetAnimationCriteria(string sActionType, string sSubCat, string sAction, int iCount)
		{
			_cCurAction = _cAnimDataRef.cReferenceList[sActionType][sSubCat][sAction];
			_iCount = iCount;
			_iCurIncrement = _cCurAction.iIncrement;
			_iCurFrameIdx = _cCurAction.iStartIndex;
			_cCurFrame = _cAnimDataRef.caFrameData[_cCurAction.iStartIndex];
			_iItteration = 0;
			bActive = true;
		}

		public KeyFrame Process(GameTime cTime)
		{
			// check to see if we are self terminating
			if(_iCount > -1) 
				// check to see if we are past the count
				if(_iItteration == _iCount)
					bActive = false;
	
			if(bActive) { 
				// move the time for processing
				_tTime += cTime.ElapsedGameTime;

				// if we have moved past the elapsed time for the animation, move to the next frame in the animation, 
				// or reset it to zero
				if((_tTime += cTime.ElapsedGameTime) > TimeSpan.FromMilliseconds(_iCurIncrement)) { 
					if(_iCurFrame < _cCurAction.iMaxFrames)
						_cCurFrame = _cAnimDataRef.caFrameData[_iCurFrameIdx + _iCurFrame++];
					else { 
						_iCurFrame = 0;
						++_iItteration;
					}
						
					_tTime = TimeSpan.Zero;
				}
			}

			return _cCurFrame.cKeyFrame;
		}
	}
}
