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
		Vector2			_cPosition;

		public AnimationProcessor(AnimationData cAnimData)
		{
			_cAnimDataRef = cAnimData;
		}
	}
}
