using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class Trooper : IDrawable, IAnimate
	{
		ActionManager		_cActionMgr;
		AnimationProcessor	_cAnimProc;
		Vector2				_tPos;

		#region IDrawable Members

		public Texture2D cTexRef	{ get; set; }
		public Vector2 tPos			{ get { return _tPos; } set { _tPos = value; }}
		public Frame cFrame			{ get { return _cAnimProc.cCurFrame; } set{}}

		public void Draw(SpriteBatch cBatch)
		{

		}

		#endregion

		#region IAnimate Members

		public AnimationProcessor cAnimationProcessor { get; set; }

		#endregion

		public void Process()
		{
			_cActionMgr.Process();
		}
	}
}
