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
						_iCurIncrement,
						_iCurFrameIdx;
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
			_iCurFrameIdx = 0;
			_cCurFrame = _cAnimDataRef.caFrameData[_cCurAction.iStartIndex];
		}

		public KeyFrame Process(GameTime cTime)
		{
			if((_tTime += cTime.ElapsedGameTime) > TimeSpan.FromMilliseconds(_iCurIncrement))
				_cCurFrame = _cAnimDataRef.caFrameData[++_iCurFrameIdx];

			return _cCurFrame.cKeyFrame;
		}
	}
}
