using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MightyFights_Support
{
	public class KeyFrame 
	{
		string _sType;
		object _oData;
		public string Type { get { return _sType; } set { _sType = value; } }
		public object oData { get { return _oData; } set { _oData = value; } }
	}

	public class Frame
	{
		Microsoft.Xna.Framework.Rectangle	_tRect;
		Vector2		_tCenter,
					_tTopLeft, 
					_tBottomRight;
		KeyFrame	_cKeyFrame;
		bool		_bRot,
					_bTrim;

		public Microsoft.Xna.Framework.Rectangle tRect		{ get { return _tRect; } set { _tRect = value; } }
		public Vector2 tCenter		{ get { return _tCenter; } set { _tCenter = value; } } 
		public Vector2 tTopLeft		{ get { return _tTopLeft; } set { _tTopLeft = value; } } 
		public Vector2 tBottomRight		{ get { return _tBottomRight; } set { _tBottomRight = value; } } 
		public KeyFrame cKeyFrame	{ get { return _cKeyFrame; } set { _cKeyFrame = value; } }
		public bool	bRot			{ get { return _bRot; } set { _bRot = value; } }
		public bool bTrim			{ get { return _bTrim; } set { _bTrim = value; } }
		
		public Frame(){}
		public Frame(Microsoft.Xna.Framework.Rectangle tRect, Vector2 tCenter, Vector2 tTopLeft, Vector2 tBottomRight, KeyFrame cKeyFrame, bool bRot, bool bTrim)
		{
			_tCenter = tCenter;
			_tRect = tRect;
			_cKeyFrame = cKeyFrame;
			_bRot = bRot;
			_tTopLeft = tTopLeft;
			_tBottomRight = tBottomRight;
			_bTrim = bTrim;
		}
	}

	public class ActionData 
	{
		int		_iMaxFrames,
				_iStartIndex;

		public int iMaxFrames  { get { return _iMaxFrames; } set { _iMaxFrames = value; } } 
		public int iStartIndex { get { return _iStartIndex; } set { _iStartIndex = value; } }

		public ActionData(){}
		public ActionData(int iMaxFrames, int iStartIndex) 
		{
			_iMaxFrames = iMaxFrames;
			_iStartIndex = iStartIndex;
		}
	}

	public class AnimationData
	{
		List<Frame>			_caFrameData;
		Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>>	_cReferenceList;

		public List<Frame> caFrameData	{ get { return _caFrameData; } set { _caFrameData = value; } }
		public Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>> cReferenceList	{ get { return _cReferenceList; } set { _cReferenceList = value; } }

		public AnimationData(){}
		public AnimationData(List<Frame> caFrameData, Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>> cReferenceList)
		{
			_caFrameData = caFrameData;
			_cReferenceList = cReferenceList;
		}

		public ActionData GetActionData(string sMainType, string sSubType, string sAction)
		{
			return _cReferenceList[sMainType][sSubType][sAction];
		}

		public Frame GetFrame(ActionData cActData, int iFrameIndex)
		{
			return _caFrameData[cActData.iStartIndex + iFrameIndex];
		}

		public void Dispose()
		{
			caFrameData.Clear();
			cReferenceList.Clear();
		}
	}

}