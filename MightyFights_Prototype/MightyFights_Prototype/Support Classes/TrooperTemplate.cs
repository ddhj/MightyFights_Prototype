using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	public class TrooperTemplate
	{
		public AnimationProcessor	cAnimProcessorRef	{ get; set; }
		public Texture2D			cTextureRef			{ get; set; }
		public ActionManager		cActionMgr			{ get; set; }
		public Stats				cStats				{ get; set; }
	}
}
