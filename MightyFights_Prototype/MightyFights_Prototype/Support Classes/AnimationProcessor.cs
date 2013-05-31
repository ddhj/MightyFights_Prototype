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
		string			_sActionType,
						_sSubCat,
						_sAction;

		Dictionary<string, int>		_cActionIncrement = new Dictionary<string,int>();

		public Frame cCurFrame	{ get { return _cCurFrame; }}
		public string sType		{ get { return _sActionType; }}
		public string sSubType	{ get { return _sSubCat; }}
		public string sAction	{ get { return _sAction; }}
		public bool	bActive		{ get; set; }
		public AnimationData cAnimData	{ get { return _cAnimDataRef; }}

		public Dictionary<string, int> cActionIncrement	 { get { return _cActionIncrement; }}

		public AnimationProcessor(AnimationData cAnimData)
		{
			_cAnimDataRef = cAnimData;
		}

		public void SetAnimationCriteria(string sActionType, string sSubCat, string sAction, int iCount)
		{
			_sActionType = sActionType;
			_sSubCat = sSubCat;
			_sAction = sAction;

			_cCurAction = _cAnimDataRef.cReferenceList[sActionType][sSubCat][sAction];
			_iCount = iCount;
			_iCurIncrement = _cCurAction.iIncrement;

			// check to see if we have an additional increment to the action
			if(_cActionIncrement.ContainsKey(sAction))
				_iCurIncrement += _cActionIncrement[sAction];

			_iCurFrameIdx = _cCurAction.iStartIndex;
			_cCurFrame = _cAnimDataRef.caFrameData[_cCurAction.iStartIndex];
			_iItteration = 0;
			_tTime = TimeSpan.Zero;
			bActive = true;
		}

		public KeyFrame Process(GameTime cTime)
		{
			KeyFrame		cKeyFrame = null;

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
				if( _tTime > TimeSpan.FromMilliseconds(_iCurIncrement)) { 
					if(_iCurFrame < _cCurAction.iMaxFrames)
						_cCurFrame = _cAnimDataRef.caFrameData[_iCurFrameIdx + _iCurFrame++];
					else { 
						_iCurFrame = 0;
						_cCurFrame = _cAnimDataRef.caFrameData[_iCurFrameIdx];
						++_iItteration;
					}
					
					// set the keyframe for this frame, we only want this to happen the first time we are in the frame and not during 
					// the delta between increments 
					cKeyFrame = _cCurFrame.cKeyFrame;

					_tTime = TimeSpan.Zero;
				}		
			}

			return cKeyFrame;
		}
	}
}
