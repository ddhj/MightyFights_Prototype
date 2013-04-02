using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class Trooper : IDrawable, IAnimate, IOpponent
	{
		ActionManager		_cActionMgr;
		AnimationProcessor	_cAnimProc;
		Vector2				_tPos;
		Texture2D			_cTexRef;

		public Trooper(TrooperTemplate cTemplate)
		{
			_cAnimProc = cTemplate.cAnimProcessorRef;
			_cActionMgr = new TrooperActMgr(this);
			_cTexRef = cTemplate.cTextureRef;
		}

		#region IDrawable Members

		public Texture2D cTexRef	{ get { return _cTexRef; } set { _cTexRef = value; }}
		public Vector2 tPos			{ get { return _tPos; } set { _tPos = value; }}
		public Frame cFrame			{ get { return _cAnimProc.cCurFrame; } set{}}

		public void Draw(SpriteBatch cBatch)
		{
			Frame	cCurFrame = cFrame;
			cBatch.Draw(_cTexRef, _tPos, cCurFrame.tRect, Color.White, cCurFrame.bRot ? -(float)Math.PI/2 : 0, cCurFrame.tTopLeft, 1, SpriteEffects.None, 0);
		}

		#endregion

		#region IAnimate Members

		public AnimationProcessor cAnimationProcessor { get { return _cAnimProc; } set { _cAnimProc = value; }}

		#endregion

		public void Process(GameTime cTime)
		{
			_cActionMgr.Process();
		}

		#region IOpponent Members

		public void DealDamage()
		{

		}

		#endregion
	}
}
