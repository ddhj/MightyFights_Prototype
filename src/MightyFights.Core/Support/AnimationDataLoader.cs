// system includes
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace MightyFights_Support
{
	//// PORT (Phase 2, T2.2): runtime replacement for the XNA content pipeline's
	//// AnimDataReader/AnimationDataEx route (deleted -- see docs/PORT_NOTES.md). This is a
	//// byte-for-byte port of AnimationDataEx/SupportClasses.cs's AnimationDataConstructor,
	//// which used to run at XNA *build* time (JavaScriptSerializer, custom ContentTypeWriter,
	//// AnimDataReader deserializing the resulting .xnb). It now runs at game *load* time
	//// (System.Text.Json reading the TexturePacker JSON directly), producing the same
	//// AnimationData/Frame/ActionData/KeyFrame graph as before. Ground truth for every formula
	//// below is AnimationDataEx/SupportClasses.cs's AnimationDataConstructor -- do not
	//// "simplify" or unify the two frame-geometry branches below, they differ in the original
	//// (ProcessActionData vs ProcessSimpleSheet use different math for tCenter/tCenterLeft/
	//// tCenterRight) and preserving that exactly is the point of a behavior-preserving port
	//// (risk R3 in the port plan).
	public static class AnimationDataLoader
	{
	// TexturePacker JSON DTOs -- input shape only, property names match the JSON keys exactly
	// (case-sensitive) so no [JsonPropertyName] attributes are needed. Mirrors
	// AnimationDataEx/SupportClasses.cs's TPFrame/MainAction/Action/Animation/Meta types.
		private class TPRect
		{
			public int x { get; set; }
			public int y { get; set; }
			public int w { get; set; }
			public int h { get; set; }
		}

		private class TPDim
		{
			public int w { get; set; }
			public int h { get; set; }
		}

		private class TPKeyFrame
		{
			public string Type { get; set; }
			// always a JSON string in every source file (e.g. "10", ""), even where consuming
			// code treats it as numeric via Convert.ToSingle(object) -- see TrooperActMgr.cs /
			// HealerActMgr.cs ProcessKeyFrame(). Keep as string here; the runtime KeyFrame.oData
			// property is still object, matching the original, so this assigns in cleanly.
			public string oData { get; set; }
		}

		private class TPFrame
		{
			public string filename { get; set; }
			public TPRect frame { get; set; }
			public bool rotated { get; set; }
			public bool trimmed { get; set; }
			public TPRect spriteSourceSize { get; set; }
			public TPDim sourceSize { get; set; }
			public TPKeyFrame KeyFrame { get; set; }
		}

		private class TPAnimation
		{
			public string sAction { get; set; }
			public int iIncrement { get; set; }
		}

		private class TPAction
		{
			public string Type { get; set; }
			public TPAnimation[] Actions { get; set; }
		}

		private class TPMainAction
		{
			public string MainType { get; set; }
			public TPAction[] SubTypes { get; set; }
		}

		private class TPData
		{
			public TPMainAction[] ActionTypes { get; set; }
			public TPFrame[] frames { get; set; }
			// "meta" (TexturePacker app/version/image/format/size/scale/smartupdate) is present
			// in every source file but was never read by AnimationDataConstructor either --
			// intentionally not modeled here, matching the original's behavior exactly.
		}

	// Public API

		public static AnimationData Load(string sJsonPath)
		{
			string sJson = File.ReadAllText(sJsonPath);
			TPData cData = JsonSerializer.Deserialize<TPData>(sJson);

			return cData.ActionTypes != null ? ProcessActionData(cData) : ProcessSimpleSheet(cData);
		}

	// Ported verbatim from AnimationDataConstructor.ProcessActionData

		private static AnimationData ProcessActionData(TPData cData)
		{
			List<Frame> caFrames = new List<Frame>();
			Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>> cRefList =
				new Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>>();
			Dictionary<string, Dictionary<string, ActionData>> cSubRef;
			Dictionary<string, ActionData> cRef;
			Dictionary<string, KeyValuePair<string, string>> cActionToMainAction =
				new Dictionary<string, KeyValuePair<string, string>>();

			foreach (TPMainAction cMainAction in cData.ActionTypes)
				foreach (TPAction cAction in cMainAction.SubTypes)
					foreach (TPAnimation cAnim in cAction.Actions)
					{
						cActionToMainAction.Add(cAnim.sAction, new KeyValuePair<string, string>(cMainAction.MainType, cAction.Type));
						if (!cRefList.TryGetValue(cMainAction.MainType, out cSubRef))
							cRefList.Add(cMainAction.MainType, cSubRef = new Dictionary<string, Dictionary<string, ActionData>>());
						if (!cSubRef.TryGetValue(cAction.Type, out cRef))
							cSubRef.Add(cAction.Type, cRef = new Dictionary<string, ActionData>());
						cRef.Add(cAnim.sAction, new ActionData(cAnim.iIncrement));
					}

			int iIndex = 0;
			string sSubString, sCurAction = "";
			ActionData cActData = null;
			Vector2 tCenter, tTopLeft, tBottomRight, tFlipTopLeft, tCenterLeft, tCenterRight;
			KeyValuePair<string, string> tRefLookup;

			foreach (TPFrame cFrame in cData.frames)
			{
				sSubString = cFrame.filename.Substring(0, cFrame.filename.IndexOf('.') - 2);
				if (sSubString != sCurAction)
				{
					if (sCurAction != "")
						cActData.iMaxFrames = iIndex - cActData.iStartIndex;

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

				Rectangle cRect;
				if (cFrame.rotated)
				{
					cRect = new Rectangle(cFrame.frame.x, cFrame.frame.y, cFrame.frame.h, cFrame.frame.w);
					tTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -cFrame.spriteSourceSize.x);
					tFlipTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)));

					tCenterLeft = new Vector2(Math.Abs(tTopLeft.Y), tTopLeft.X - cRect.Width);
					tCenterRight = new Vector2(Math.Abs(tTopLeft.Y + cRect.Height), Math.Abs(tTopLeft.X + cRect.Width / 2));
					tCenter = new Vector2(Math.Abs(tTopLeft.Y + cRect.Height / 2), Math.Abs(tTopLeft.X + cRect.Width / 2));
				}
				else
				{
					cRect = new Rectangle(cFrame.frame.x, cFrame.frame.y, cFrame.frame.w, cFrame.frame.h);

					tCenter = new Vector2(Math.Abs(tTopLeft.X + cRect.Width / 2), Math.Abs(tTopLeft.Y));
					tCenterLeft = new Vector2(Math.Abs(tTopLeft.X), Math.Abs(tTopLeft.Y));
					tCenterRight = new Vector2(Math.Abs(tTopLeft.X + cRect.Width), Math.Abs(tTopLeft.Y + cRect.Height / 2));
				}

				KeyFrame cKeyFrame = cFrame.KeyFrame == null ? null : new KeyFrame { Type = cFrame.KeyFrame.Type, oData = cFrame.KeyFrame.oData };

				caFrames.Add(new Frame(cRect, tCenter, tTopLeft, tFlipTopLeft, tBottomRight, tCenterLeft, tCenterRight,
					cKeyFrame, cFrame.rotated, cFrame.trimmed));
				++iIndex;
			}
			if (cActData != null)
				cActData.iMaxFrames = iIndex - cActData.iStartIndex;

			return new AnimationData(caFrames, cRefList);
		}

	// Ported verbatim from AnimationDataConstructor.ProcessSimpleSheet -- note the frame
	// geometry math genuinely differs from ProcessActionData above in the original (tCenter is
	// never reassigned inside either branch here, tCenterLeft's formula differs in both
	// branches). That is the original's behavior; preserved as-is, not a copy-paste bug to fix.

		private static AnimationData ProcessSimpleSheet(TPData cData)
		{
			List<Frame> caFrames = new List<Frame>();
			Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>> cRefList =
				new Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>>();
			Dictionary<string, ActionData> cRef = new Dictionary<string, ActionData>();
			cRefList.Add("Main", new Dictionary<string, Dictionary<string, ActionData>> { { "Sub", cRef } });

			int iIndex = 0;
			string sSubString, sCurAction = "";
			ActionData cActData = null;
			Vector2 tCenter, tTopLeft, tBottomRight, tFlipTopLeft, tCenterLeft, tCenterRight;

			foreach (TPFrame cFrame in cData.frames)
			{
				sSubString = cFrame.filename.Substring(0, cFrame.filename.IndexOf('.') - 2);
				if (sSubString != sCurAction)
				{
					if (sCurAction != "")
						cActData.iMaxFrames = iIndex - cActData.iStartIndex;

					cRef.Add(sSubString, cActData = new ActionData());
					cActData.iStartIndex = iIndex;

					// default running speed
					cActData.iIncrement = 100;
					sCurAction = sSubString;
				}

				tTopLeft = new Vector2(-cFrame.spriteSourceSize.x, -cFrame.spriteSourceSize.y);
				tFlipTopLeft = new Vector2(-(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)), -cFrame.spriteSourceSize.y);

				tCenter = new Vector2(((cFrame.sourceSize.w / 2.0f) - (cFrame.spriteSourceSize.x)),
					((cFrame.sourceSize.h / 2.0f) - (cFrame.spriteSourceSize.y)));
				tBottomRight = new Vector2((cFrame.sourceSize.w - (cFrame.spriteSourceSize.x)),
					(cFrame.sourceSize.h - (cFrame.spriteSourceSize.y)));

				Rectangle cRect;
				if (cFrame.rotated)
				{
					cRect = new Rectangle(cFrame.frame.x, cFrame.frame.y, cFrame.frame.h, cFrame.frame.w);
					tTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -cFrame.spriteSourceSize.x);
					tFlipTopLeft = new Vector2(cFrame.spriteSourceSize.y + cFrame.frame.h, -(100 - (cFrame.spriteSourceSize.x + cFrame.spriteSourceSize.w)));
					tCenterLeft = new Vector2(Math.Abs(tTopLeft.Y), Math.Abs(tTopLeft.X + cRect.Width / 2));
					tCenterRight = new Vector2(Math.Abs(tTopLeft.Y + cRect.Height), Math.Abs(tTopLeft.X + cRect.Width / 2));
				}
				else
				{
					cRect = new Rectangle(cFrame.frame.x, cFrame.frame.y, cFrame.frame.w, cFrame.frame.h);

					tCenterLeft = new Vector2(Math.Abs(tTopLeft.X), Math.Abs(tTopLeft.Y + cRect.Height / 2));
					tCenterRight = new Vector2(Math.Abs(tTopLeft.X + cRect.Width), Math.Abs(tTopLeft.Y + cRect.Height / 2));
				}

				KeyFrame cKeyFrame = cFrame.KeyFrame == null ? null : new KeyFrame { Type = cFrame.KeyFrame.Type, oData = cFrame.KeyFrame.oData };

				caFrames.Add(new Frame(cRect, tCenter, tTopLeft, tFlipTopLeft, tBottomRight, tCenterLeft, tCenterRight,
					cKeyFrame, cFrame.rotated, cFrame.trimmed));
				++iIndex;
			}
			if (cActData != null)
				cActData.iMaxFrames = iIndex - cActData.iStartIndex;

			return new AnimationData(caFrames, cRefList);
		}
	}
}
