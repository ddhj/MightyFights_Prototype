using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class FrontGuys : ClickableSprite
	{
		AnimationData		_cAnimData;
		int					_iFrameIdx = 0;

		////ddhj: this is because we don't have all the colors that the animation data does for the 
		// actual sprite sheets
		List<Frame> _caFrames = new List<Frame>();
		static Dictionary<string, int> _caNameLookup = new Dictionary<string,int>() { {"fazure", 0}, {"fbrown", 1}, {"fcrimson", 2}, 
			{"fgrey", 3}, {"fgules", 4}, {"fmidnight", 5}, {"fpurple", 6}, {"frust", 7}, {"fsable", 8}, {"fsteel", 9}, {"fstorm", 10}, 
			{"ftenne", 11}, {"fzombie", 12}};
		static Dictionary<int, string> _caNameByIdx = new Dictionary<int,string>() { {0, "fazure"}, {1, "fbrown"}, {2, "fcrimson"}, 
			{3, "fgrey"}, {4, "fgules"}, {5, "fmidnight"}, {6, "fpurple"}, {7, "frust"}, {8, "fsable"}, {9, "fsteel"}, {10, "fstorm"}, 
			{11, "ftenne"}, {12, "fzombie"}};

		public int iOtherColor		{ get; set; }
		public string sCurColor		{ get { return @"Sprite Data\Troopers\Halberd\Textures\" + _caNameByIdx[_iFrameIdx]; }}
		public int iFrameIdx		{ get { return _iFrameIdx; }}

		public FrontGuys(AnimationData cData, string sColor)
		{
			_cAnimData = cData;

			////ddhj: build static frame list
			_caFrames.AddRange( new Frame[] { _cAnimData.caFrameData[1], _cAnimData.caFrameData[4], _cAnimData.caFrameData[7], _cAnimData.caFrameData[13],
				_cAnimData.caFrameData[14], _cAnimData.caFrameData[16], _cAnimData.caFrameData[18], _cAnimData.caFrameData[20], _cAnimData.caFrameData[21],
				_cAnimData.caFrameData[24], _cAnimData.caFrameData[25], _cAnimData.caFrameData[26], _cAnimData.caFrameData[3] });

			SetByColor(sColor);
		}

		public void IncrementColor()
		{
			int		iTmpIdx = _iFrameIdx + 1;
			if(iTmpIdx == this.iOtherColor)
				++iTmpIdx;

			if(iTmpIdx >= _caFrames.Count) { 
				iTmpIdx = 0;
				if(this.iOtherColor == 0)
					_iFrameIdx = 1;
				else _iFrameIdx = 0;
			} else _iFrameIdx = iTmpIdx;

			this.cFrame = _caFrames[_iFrameIdx];
		}

		public void DecrementColor()
		{
			int		iTmpIdx = _iFrameIdx - 1;
			if(iTmpIdx == this.iOtherColor)
				--iTmpIdx;

			if(iTmpIdx < 0) { 
				iTmpIdx = _caFrames.Count - 1;
				if(this.iOtherColor == iTmpIdx)	
					_iFrameIdx = --iTmpIdx;
				else _iFrameIdx = iTmpIdx;
			} else _iFrameIdx = iTmpIdx;

			this.cFrame = _caFrames[_iFrameIdx];
		}

		public void SetByColor(string sColor)
		{
			this.cFrame = _caFrames[_iFrameIdx = _caNameLookup[sColor.Substring(sColor.LastIndexOf("\\") + 1)]];
		}

		public void SetOpponent(string sColor)
		{
			this.iOtherColor = _caNameLookup[sColor.Substring(sColor.LastIndexOf("\\") + 1)];
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, Color.White, cFrame.bRot ? -(float)Math.PI/2 : 0, cFrame.tTopLeft, 1, 
				// and 2: the direction vector
				SpriteEffects.None, 1);
		}
	}
}
