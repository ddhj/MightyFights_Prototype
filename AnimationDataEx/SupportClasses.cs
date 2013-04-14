using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Web.Script.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content;

namespace AnimationDataEx
{
	internal class Rectangle
	{
		public int x { get; set; }
		public int y { get; set; }
		public int w { get; set; }
		public int h { get; set; }
	}

	internal class Dimensions
	{
		public int w { get; set; }
		public int h { get; set; }
	}

	internal class TPFrame
	{
		public string filename { get; set; }
		public Rectangle frame { get; set; }
		public bool rotated { get; set; }
		public bool trimmed { get; set; }
		public Rectangle spriteSourceSize { get; set; }
		public Dimensions sourceSize { get; set; }
		public KeyFrame KeyFrame { get; set; }
	}

	internal class Animation
	{
		public string sAction { get; set;}
		public int iIncrement { get; set;}
	}

	internal class Action
	{
		public string Type { get; set; }
		public Animation[] Actions { get; set; }
	}

	internal class MainAction
	{
		public string MainType { get; set; }
		public Action[] SubTypes { get; set; }

	}

	[ContentSerializerRuntimeType("MightyFights_Support.KeyFrame, MightyFights_Support")]
	public class KeyFrame 
	{
		string _sType;
		object _oData;
		public string Type { get { return _sType; } set { _sType = value; } }
		public object oData { get { return _oData; } set { _oData = value; } }
	}

	internal class Meta
	{
		public string app { get; set; }
		public string version { get; set; }
		public string image { get; set; }
		public string format { get; set; }
		public Dimensions size { get; set; }
		public string scale { get; set; }
		public string smartupdate { get; set; }
	}

	internal class TextureDictionaryData
	{
		public MainAction[] ActionTypes { get; set; }
		public TPFrame[] frames { get; set; }
		public Meta meta { get; set; }
	}

	[ContentSerializerRuntimeType("MightyFights_Support.Frame, MightyFights_Support")]
	public class Frame
	{
		Microsoft.Xna.Framework.Rectangle	_tRect;
		Vector2		_tCenter,
					_tTopLeft, 
					_tBottomRight,
					_tFlipTopLeft;
		KeyFrame	_cKeyFrame;
		bool		_bRot,
					_bTrim;

		public Microsoft.Xna.Framework.Rectangle tRect		{ get { return _tRect; } set { _tRect = value; } }
		public Vector2 tCenter		{ get { return _tCenter; } set { _tCenter = value; } } 
		public Vector2 tTopLeft		{ get { return _tTopLeft; } set { _tTopLeft = value; } } 
		public Vector2 tFlipTopLeft { get { return _tFlipTopLeft; } set { _tFlipTopLeft = value; }}
		public Vector2 tBottomRight	{ get { return _tBottomRight; } set { _tBottomRight = value; } } 
		public KeyFrame cKeyFrame	{ get { return _cKeyFrame; } set { _cKeyFrame = value; } }
		public bool	bRot			{ get { return _bRot; } set { _bRot = value; } }
		public bool bTrim			{ get { return _bTrim; } set { _bTrim = value; } }
		
		public Frame(){}
		public Frame(Microsoft.Xna.Framework.Rectangle tRect, Vector2 tCenter, Vector2 tTopLeft, Vector2 tFlipTopLeft, Vector2 tBottomRight, KeyFrame cKeyFrame, bool bRot, bool bTrim)
		{
			_tCenter = tCenter;
			_tRect = tRect;
			_cKeyFrame = cKeyFrame;
			_bRot = bRot;
			_tTopLeft = tTopLeft;
			_tBottomRight = tBottomRight;
			_bTrim = bTrim;
			_tFlipTopLeft = tFlipTopLeft;
		}
	}

	[ContentSerializerRuntimeType("MightyFights_Support.ActionData, MightyFights_Support")]
	public class ActionData 
	{
		int		_iMaxFrames,
				_iStartIndex, 
				_iIncrement;

		public int iMaxFrames	{ get { return _iMaxFrames; } set { _iMaxFrames = value; }} 
		public int iStartIndex	{ get { return _iStartIndex; } set { _iStartIndex = value; }}
		public int iIncrement	{ get { return _iIncrement; } set { _iIncrement = value; }}

		public ActionData(){}
		public ActionData(int iIncrement)
		{
			_iIncrement = iIncrement;
		}

		public ActionData(int iMaxFrames, int iStartIndex) 
		{
			_iMaxFrames = iMaxFrames;
			_iStartIndex = iStartIndex;
		}
	}

	//[ContentSerializerRuntimeType("WindowsGameLibrary1.AnimationData, WindowsGameLibrary1")]
	public class AnimationDataContent
	{
		List<Frame>			_caFrameData;
		Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>>	_cReferenceList;

		public List<Frame> caFrameData	{ get { return _caFrameData; } set { _caFrameData = value; } }
		public Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>> cReferenceList	{ get { return _cReferenceList; } set { _cReferenceList = value; } }

		public AnimationDataContent(){}
		public AnimationDataContent(List<Frame> caFrameData, Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>> cReferenceList)
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
	}

	public class AnimationDataConstructor
	{
		AnimationDataContent	_cFinalData;

		public AnimationDataContent cFinalData		{ get { return _cFinalData; } }

		void ProcessActionData(TextureDictionaryData cData)
		{
			List<Frame>		caFrames = new List<Frame>();
			Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>> cRefList = new Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>>();
			Dictionary<string, Dictionary<string, ActionData>> cSubRef;
			Dictionary<string, ActionData> cRef;
			Dictionary<string,KeyValuePair<string, string>> cActionToMainAction = new Dictionary<string,KeyValuePair<string, string>>();
		
			foreach(MainAction cMainAction in cData.ActionTypes)
				foreach(Action cAction in cMainAction.SubTypes) 
					foreach(Animation cAnim in cAction.Actions) { 
						cActionToMainAction.Add(cAnim.sAction, new KeyValuePair<string, string>(cMainAction.MainType, cAction.Type));
						if(!cRefList.TryGetValue(cMainAction.MainType, out cSubRef)) 
							cRefList.Add(cMainAction.MainType, cSubRef = new Dictionary<string, Dictionary<string, ActionData>>());
						if(!cSubRef.TryGetValue(cAction.Type, out cRef))
							cSubRef.Add(cAction.Type, cRef = new Dictionary<string,ActionData>());
						cRef.Add(cAnim.sAction, new ActionData(cAnim.iIncrement));
					}

			int iIndex = 0;
			string	sSubString,
					sCurAction = "";
			ActionData cActData = null;
			Vector2	tCenter,
					tTopLeft, 
					tBottomRight,
					tFlipTopLeft;
			KeyValuePair<string, string> tRefLookup;
			foreach(TPFrame cFrame in cData.frames) { 
				sSubString = cFrame.filename.Substring(0, cFrame.filename.IndexOf('.') - 2);
				if(sSubString != sCurAction) { 
					if(sCurAction != "") { 
						cActData.iMaxFrames = iIndex - cActData.iStartIndex;
					}

					// get the action data 
					tRefLookup = cActionToMainAction[sSubString];
					cActData = cRefList[tRefLookup.Key][tRefLookup.Value][sSubString];
					cActData.iStartIndex = iIndex;
					sCurAction = sSubString;
				}

				tTopLeft = new Vector2(-cFrame.spriteSourceSize.x, -cFrame.spriteSourceSize.y);
				tFlipTopLeft = new Vector2(-(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)), -cFrame.spriteSourceSize.y);

				tCenter = new Vector2(((cFrame.sourceSize.w / 2.0f) - (cFrame.spriteSourceSize.x)), 
					((cFrame.sourceSize.h / 2.0f) - (cFrame.spriteSourceSize.y)));
				tBottomRight = new Vector2((cFrame.sourceSize.w - (cFrame.spriteSourceSize.x)), 
					(cFrame.sourceSize.h - (cFrame.spriteSourceSize.y)));

				Microsoft.Xna.Framework.Rectangle cRect;
				if(cFrame.rotated) { 
					cRect = new Microsoft.Xna.Framework.Rectangle(
						cFrame.frame.x, cFrame.frame.y, cFrame.frame.h, cFrame.frame.w);
					tTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -cFrame.spriteSourceSize.x);
					tFlipTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)));
				} else 
					cRect = new Microsoft.Xna.Framework.Rectangle(
						cFrame.frame.x, cFrame.frame.y, cFrame.frame.w, cFrame.frame.h);

				caFrames.Add(new Frame(cRect, tCenter, tTopLeft, tFlipTopLeft, tBottomRight, 
					cFrame.KeyFrame, cFrame.rotated, cFrame.trimmed));
				++iIndex;
			}
			cActData.iMaxFrames = iIndex - cActData.iStartIndex;

			_cFinalData = new AnimationDataContent(caFrames, cRefList);
		}

		void ProcessSimpleSheet(TextureDictionaryData cData)
		{
			List<Frame>		caFrames = new List<Frame>();
			Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>> cRefList = new Dictionary<string,Dictionary<string, Dictionary<string, ActionData>>>();
			Dictionary<string, Dictionary<string, ActionData>> cSubRef;
			Dictionary<string, ActionData> cRef;
			Dictionary<string,KeyValuePair<string, string>> cActionToMainAction = new Dictionary<string,KeyValuePair<string, string>>();

			cRefList.Add("Main", cSubRef = new Dictionary<string,Dictionary<string,ActionData>>());
			cSubRef.Add("Sub", cRef = new Dictionary<string,ActionData>());

			int iIndex = 0;
			string	sSubString,
					sCurAction = "";
			ActionData cActData = null;
			Vector2	tCenter,
					tTopLeft, 
					tBottomRight,
					tFlipTopLeft;
			foreach(TPFrame cFrame in cData.frames) { 
				sSubString = cFrame.filename.Substring(0, cFrame.filename.IndexOf('.'));
				if(sSubString != sCurAction) { 
					if(sCurAction != "") { 
						cActData.iMaxFrames = iIndex - cActData.iStartIndex;
					}

					// get the action data 
					cRef.Add(sSubString, cActData = new ActionData());
					cActData.iStartIndex = iIndex;
					sCurAction = sSubString;
				}

				tTopLeft = new Vector2(-cFrame.spriteSourceSize.x, -cFrame.spriteSourceSize.y);
				tFlipTopLeft = new Vector2(-(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)), -cFrame.spriteSourceSize.y);
				tCenter = new Vector2(((cFrame.sourceSize.w / 2.0f) - (cFrame.spriteSourceSize.x)), 
					((cFrame.sourceSize.h / 2.0f) - (cFrame.spriteSourceSize.y)));
				tBottomRight = new Vector2((cFrame.sourceSize.w - (cFrame.spriteSourceSize.x)), 
					(cFrame.sourceSize.h - (cFrame.spriteSourceSize.y)));

				Microsoft.Xna.Framework.Rectangle cRect;
				if(cFrame.rotated) { 
					cRect = new Microsoft.Xna.Framework.Rectangle(
						cFrame.frame.x, cFrame.frame.y, cFrame.frame.h, cFrame.frame.w);
					tTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -cFrame.spriteSourceSize.x);
					tFlipTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)));
				} else 
					cRect = new Microsoft.Xna.Framework.Rectangle(
						cFrame.frame.x, cFrame.frame.y, cFrame.frame.w, cFrame.frame.h);

				caFrames.Add(new Frame(cRect, tCenter, tTopLeft, tFlipTopLeft, tBottomRight, 
					cFrame.KeyFrame, cFrame.rotated, cFrame.trimmed));
				++iIndex;
			}
			cActData.iMaxFrames = iIndex - cActData.iStartIndex;

			_cFinalData = new AnimationDataContent(caFrames, cRefList);

		}

		public AnimationDataConstructor(string sFilename)
		{
			StreamReader sr = new System.IO.StreamReader(sFilename);
			string jsonText = sr.ReadToEnd();
			List<Frame>		caFrames = new List<Frame>();

			JavaScriptSerializer jss = new JavaScriptSerializer();
			TextureDictionaryData cData = jss.Deserialize<TextureDictionaryData>(jsonText);

			if(cData.ActionTypes != null) 
				ProcessActionData(cData);
			else ProcessSimpleSheet(cData);
		}
	}
}
