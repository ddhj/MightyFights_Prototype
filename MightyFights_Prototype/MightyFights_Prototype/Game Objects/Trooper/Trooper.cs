using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public partial class Trooper : IDrawable, IAnimate, ICombatant, IActive<Trooper>
	{
		ActionManager<Trooper>		_cActionMgr;
		AnimationProcessor			_cAnimProc;
		Vector2						_tPos;
		Texture2D					_cTexRef;
		Stats						_cStats;

		public bool bActive				{ get; set; }
		public bool bDir				{ get; set; }
		public ICombatant nOpponent		{ get; set; }
		public AiBattleData cAiData		{ get; set; }
		
		public ActionManager<Trooper>	cActionManager	{ get { return _cActionMgr; } set { _cActionMgr = value; }}

		public Trooper(TrooperTemplate cTemplate)
		{
			_cAnimProc = cTemplate.cAnimProcessorRef;
			_cTexRef = cTemplate.cTextureRef;
			_cActionMgr = cTemplate.cActionMgr;
			
			bActive	= true;

			_cActionMgr.AddPermAction(new Action(this.TrooperUpkeep, null, null));

			// use the template data to set up the huristics... 
			//// there is a problem here since the object manager should have set this up but the huristics are methods on an instance of trooper,
			//// considered static methods but I am not sure I like that solution
			cAiData.cHurisitics[EBattleHuristics.Attack] = Attack_Basic;
			cAiData.cHurisitics[EBattleHuristics.ChooseOpponent] = ChooseOpponent;
			cAiData.cHurisitics[EBattleHuristics.Flee] = Flee;
			cAiData.cHurisitics[EBattleHuristics.Idle] = Idle;
			cAiData.cHurisitics[EBattleHuristics.Pant] = Pant;
			cAiData.cHurisitics[EBattleHuristics.Persue] = Persue;
	
		}

		#region IDrawable Members

		public Texture2D cTexRef	{ get { return _cTexRef; } set { _cTexRef = value; }}
		public Vector2 tPos			{ get { return _tPos; } set { _tPos = value; }}
		public Frame cFrame			{ get { return _cAnimProc.cCurFrame; } set{}}

		public void Draw(SpriteBatch cBatch)
		{
			// store the frame of the sprite for ref in the draw since we hit it a couple of times 
			Frame	cCurFrame = cFrame;
			
			// draw is pretty straight forward sans two issues 1: the rotation in the sprite sheet
			cBatch.Draw(_cTexRef, _tPos, cCurFrame.tRect, Color.White, cCurFrame.bRot ? -(float)Math.PI/2 : 0, cCurFrame.tTopLeft, 1, 
				// and 2: the direction vector
				bDir == false ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);

			// there may be other things to draw here like if we are in a dying state do we want to run a blink or not 

			// or particle effect drawing calls
		}

		#endregion

		#region IAnimate Members

		public AnimationProcessor cAnimationProcessor { get { return _cAnimProc; } set { _cAnimProc = value; }}

		#endregion

		#region IOpponent Members

		public void DealDamage()
		{

		}

		#endregion

		public void Process(GameTime cTime)
		{
			_cActionMgr.Process(cTime);
		}
	}
}
