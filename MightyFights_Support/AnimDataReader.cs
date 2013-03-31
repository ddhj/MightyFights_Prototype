using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Support
{
	/// <summary>
	/// This class will be instantiated by the XNA Framework Content
	/// Pipeline to read the specified data type from binary .xnb format.
	/// 
	/// Unlike the other Content Pipeline support classes, this should
	/// be a part of your main game project, and not the Content Pipeline
	/// Extension Library project.
	/// </summary>
	public class AnimDataReader : ContentTypeReader<AnimationData>
	{
		protected override AnimationData Read(ContentReader input, AnimationData existingInstance)
		{
			Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>> cRefList = 
				input.ReadObject<Dictionary<string, Dictionary<string, Dictionary<string, ActionData>>>>();

			List<Frame> cFrames = input.ReadObject<List<Frame>>();
			return new AnimationData(cFrames, cRefList);
		}
	}
}
