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
		Dictionary<string, bool>	_cTakenColors;

		static Dictionary<string, int> _caNameLookup = new Dictionary<string,int>() { {@"Sprite Data\Troopers\Halberd\Textures\fazure", 0}, {@"Sprite Data\Troopers\Halberd\Textures\fbrown", 1}, {@"Sprite Data\Troopers\Halberd\Textures\fcrimson", 2}, 
			{@"Sprite Data\Troopers\Halberd\Textures\fgrey", 3}, {@"Sprite Data\Troopers\Halberd\Textures\fgules", 4}, {@"Sprite Data\Troopers\Halberd\Textures\fmidnight", 5}, {@"Sprite Data\Troopers\Halberd\Textures\fpurple", 6}, {@"Sprite Data\Troopers\Halberd\Textures\frust", 7}, {@"Sprite Data\Troopers\Halberd\Textures\fsable", 8}, {@"Sprite Data\Troopers\Halberd\Textures\fsteel", 9}, {@"Sprite Data\Troopers\Halberd\Textures\fstorm", 10},
 			{@"Sprite Data\Troopers\Halberd\Textures\fvert", 11} , {@"Sprite Data\Troopers\Halberd\Textures\fvert2", 12}, {@"Sprite Data\Troopers\Halberd\Textures\fzombie", 13}};
		static Dictionary<int, string> _caNameByIdx = new Dictionary<int,string>() { {0, @"Sprite Data\Troopers\Halberd\Textures\fazure"}, {1, @"Sprite Data\Troopers\Halberd\Textures\fbrown"}, {2, @"Sprite Data\Troopers\Halberd\Textures\fcrimson"}, 
			{3, @"Sprite Data\Troopers\Halberd\Textures\fgrey"}, {4, @"Sprite Data\Troopers\Halberd\Textures\fgules"}, {5, @"Sprite Data\Troopers\Halberd\Textures\fmidnight"}, {6, @"Sprite Data\Troopers\Halberd\Textures\fpurple"}, {7, @"Sprite Data\Troopers\Halberd\Textures\frust"}, {8, @"Sprite Data\Troopers\Halberd\Textures\fsable"}, {9, @"Sprite Data\Troopers\Halberd\Textures\fsteel"}, {10, @"Sprite Data\Troopers\Halberd\Textures\fstorm"}, 
 			{11, @"Sprite Data\Troopers\Halberd\Textures\fvert"} , {12, @"Sprite Data\Troopers\Halberd\Textures\fvert2"}, {13, @"Sprite Data\Troopers\Halberd\Textures\fzombie"}};

		public int iOtherColor		{ get; set; }
		public string sCurColor		{ get { return _caNameByIdx[_iFrameIdx]; }}
		public int iFrameIdx		{ get { return _iFrameIdx; }}
		public float fScale			{ get; set; }

		public FrontGuys(AnimationData cData, string sColor, Dictionary<string, bool> cTakenColors)
		{
			_cAnimData = cData;
			SetByColor(sColor);
			_cTakenColors = cTakenColors;
			this.fScale = 1.0f;
		}

		public void IncrementColor()
		{
			int		iTmpIdx = _iFrameIdx + 1;
			if(iTmpIdx > _cAnimData.caFrameData.Count - 1)
				iTmpIdx = 0;

			while(_cTakenColors.ContainsKey(_caNameByIdx[iTmpIdx])) { 
				++iTmpIdx;
				if(iTmpIdx > _cAnimData.caFrameData.Count - 1)
					iTmpIdx = 0;
			}
			
			_iFrameIdx = iTmpIdx;

			this.cFrame = _cAnimData.caFrameData[_iFrameIdx];
		}

		public void DecrementColor()
		{
			int		iTmpIdx = _iFrameIdx - 1;
			if(iTmpIdx < 0)
				iTmpIdx = _cAnimData.caFrameData.Count - 1;
			while(_cTakenColors.ContainsKey(_caNameByIdx[iTmpIdx])) { 
				--iTmpIdx;
				if(iTmpIdx < 0)
					iTmpIdx = _cAnimData.caFrameData.Count - 1;
			}
			
			_iFrameIdx = iTmpIdx;

			this.cFrame = _cAnimData.caFrameData[_iFrameIdx];
		}

		public void SetByColor(string sColor)
		{
			this.cFrame = _cAnimData.caFrameData[_iFrameIdx = _caNameLookup[sColor]];
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(cTexRef, tPos, cFrame.tRect, tColor, cFrame.bRot ? -(float)Math.PI/2 : 0, cFrame.tTopLeft, this.fScale, 
				// and 2: the direction vector
				SpriteEffects.None, fZRange);
		}
	}
}
