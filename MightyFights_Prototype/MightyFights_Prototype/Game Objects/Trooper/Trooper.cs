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
		int							_iAvailablePositions = 6,
									_iCurLeftAttackers = 0,
									_iCurRightAttakers = 0;
		float						_fZorder;
		byte						_byAttakPos;
		BattlegroundData			_cBattleDataRef = null;
		ETrooperAttackPos			_eAttackingPos;

		Dictionary<ETrooperAttackPos, ICombatant>	_caAttackers = new Dictionary<ETrooperAttackPos,ICombatant>();

		//// ddhj: debug data
		

		public bool bActive				{ get; set; }
		public bool bDir				{ get; set; }
		public ICombatant nOpponent		{ get; set; }
		public AiBattleData cAiData		{ get; set; }
		public int iArmyIndex			{ get; set; }
		public int iOpponentIndex		{ get; set; }
		public Zone cZone				{ get; set; }
		public Stats cStats				{ get { return _cStats; } set { _cStats = value; }}
		public bool bAvailablePos		{ get { return _iCurLeftAttackers + _iCurRightAttakers < _iAvailablePositions; }} 
		public Vector2 tAttackPos		{ get; set; }
		public Dictionary<ETrooperAttackPos, ICombatant> caAttackers	{ get { return _caAttackers; }}
		
		public ActionManager<Trooper>	cActionManager	{ get { return _cActionMgr; } set { _cActionMgr = value; }}

		public Trooper(TrooperTemplate cTemplate)
		{
			_cAnimProc = cTemplate.cAnimProcessorRef;
			_cTexRef = cTemplate.cTextureRef;
			_cActionMgr = cTemplate.cActionMgr;
			_cActionMgr.cData = this;
			_cStats = cTemplate.cStats;
			cAiData = cTemplate.cAiData;
			sTexName = cTemplate.sTexName;
			
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
		public string sTexName		{ get; set; }

		public void Draw(SpriteBatch cBatch)
		{
			// store the frame of the sprite for ref in the draw since we hit it a couple of times 
			Frame			cCurFrame = cFrame;
			SpriteEffects	eEffect = SpriteEffects.None;
			Vector2			tTopLeft = cCurFrame.tTopLeft;
			Color			tColor = Color.White;

			// check to see if we need to perform a flip 
			if(bDir) {
				// if we are rotated first then we need to flip the frame differently 
				if(cCurFrame.bRot)
					// since the texture packer rotates we need to flip the sprite across the x axis 
					eEffect = SpriteEffects.FlipVertically;
			
				// the frame is not rotated so just flip on y axis
				else eEffect = SpriteEffects.FlipHorizontally;

				// use the flip top left 
				tTopLeft = cCurFrame.tFlipTopLeft;
			}

			// check to see if we are dead
			if(cAiData.eState == EBattleAiStates.Dead) { 
				tColor.A = 85;
				_fZorder = .99f;
			}

			// draw is pretty straight forward sans two issues 1: the rotation in the sprite sheet
			cBatch.Draw(_cTexRef, _tPos, cCurFrame.tRect, tColor, cCurFrame.bRot ? -(float)Math.PI/2 : 0, tTopLeft, 1, 
				// and 2: the direction vector
				eEffect, _fZorder);

			// there may be other things to draw here like if we are in a dying state do we want to run a blink or not 

			// or particle effect drawing calls

			//// ddhj: debug draw data
			// lets draw our attack positions
			Vector2 tDir = _tPos;
			Texture2D	cBorder = DataStore.cInstance.cBorder;
			if((_byAttakPos & (byte)ETrooperAttackPos.LeftBottom) == (byte)ETrooperAttackPos.LeftBottom) { 
				tDir.X -= 27;
				tDir.Y += 20;
				//cBatch.Draw(cBorder, new Rectangle((int)_tPos.X, (int)_tPos.Y, 1, 64), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)_tPos.X + 100, (int)_tPos.Y, 1, 64), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)_tPos.X, (int)_tPos.Y, 100, 1), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)_tPos.X, (int)_tPos.Y + 64, 100, 1), tColor);
				
			}
			if((_byAttakPos & (byte)ETrooperAttackPos.LeftMid) == (byte)ETrooperAttackPos.LeftMid) { 
				tDir.X += 25;
			}
			if((_byAttakPos & (byte)ETrooperAttackPos.LeftTop) == (byte)ETrooperAttackPos.LeftTop) { 
				tDir.X -= 27;
				tDir.Y += 20;

			}
			if((_byAttakPos & (byte)ETrooperAttackPos.RightBottom) == (byte)ETrooperAttackPos.RightBottom) { 
				tDir.X += 27;
				tDir.Y += 20;

			}
			if((_byAttakPos & (byte)ETrooperAttackPos.RightMid) == (byte)ETrooperAttackPos.RightMid) { 
				tDir.X += 25;

			}
			if((_byAttakPos & (byte)ETrooperAttackPos.RightTop) == (byte)ETrooperAttackPos.RightTop) { 
				tDir.X += 27;
				tDir.Y -= 20;

			}

			// lets draw our sprite rect
			Vector2 tVect = _tPos;

			if(cCurFrame.bRot) { 
				//tVect.X += Math.Abs(tTopLeft.Y);
				//tVect.Y += tTopLeft.X - cCurFrame.tRect.Width;
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, 1, cCurFrame.tRect.Width), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X + cCurFrame.tRect.Height, (int)tVect.Y, 1, cCurFrame.tRect.Width), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, cCurFrame.tRect.Height, 1), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y + cCurFrame.tRect.Width, cCurFrame.tRect.Height, 1), tColor);
			} else { 
				//tVect.X += Math.Abs(tTopLeft.X);
				//tVect.Y += Math.Abs(tTopLeft.Y);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, 1, cCurFrame.tRect.Height), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X + cCurFrame.tRect.Width, (int)tVect.Y, 1, cCurFrame.tRect.Height), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, cCurFrame.tRect.Width, 1), tColor);
				//cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y + cCurFrame.tRect.Height, cCurFrame.tRect.Width, 1), tColor);
			}
		}

		#endregion

		#region IAnimate Members

		public AnimationProcessor cAnimationProcessor { get { return _cAnimProc; } set { _cAnimProc = value; }}

		#endregion

		#region ICombatant Methods

		public void DealDamage(int iDamage)
		{
			//// ddhj: yep armor class and all that shit 
			if(cAiData.eState != EBattleAiStates.Defending)
				_cStats.iHp -= iDamage;
		}

		public bool IsDead()
		{
			return cAiData.eState == EBattleAiStates.Dying || cAiData.eState == EBattleAiStates.Dead;
		}

		public void SetAttacker(ICombatant nCombatant)
		{
			Vector2		tDir = _tPos - nCombatant.tPos;
			ETrooperAttackPos ePos;

			RequestAttackPoint(nCombatant, out ePos);
			_caAttackers.Add(ePos, nCombatant);
			_byAttakPos &= (byte)ePos;

			// set left or right
			if(tDir.X >= 0.0 + float.Epsilon)
				++_iCurRightAttakers;
			else ++_iCurLeftAttackers;
		}

		Vector2 RightAttackPos(Vector2 tDir, out ETrooperAttackPos ePos) 
		{
			switch(_iCurRightAttakers) { 
				// if its zero its always the midpoint
				case 0:
					tDir = _tPos;
					tDir.X += 25;
					ePos = ETrooperAttackPos.RightMid;
					return tDir;
						
				// it cant be more than 1 (for now there may be more but that will be on a different type of class and not a trooper 
					// probably)
				default:
					// check if the opponent is above or below
					if(tDir.Y >= 0.0 + float.Epsilon) { 
						// check to see if the right bottom is taken 
						if((_byAttakPos & (int)ETrooperAttackPos.RightBottom) == (int)ETrooperAttackPos.RightBottom) {
							// set the point to be right top
							tDir = _tPos;
							tDir.X += 27;
							tDir.Y -= 20;
							ePos = ETrooperAttackPos.RightTop;
							return tDir;
						// send out right bottom
						} else { 
							tDir = _tPos;
							tDir.X += 27;
							tDir.Y += 20;
							ePos = ETrooperAttackPos.RightBottom;
							return tDir;
						}
					} else { 
						// the opponent is above us, check to see if our right top is taken
						if((_byAttakPos & (int)ETrooperAttackPos.RightTop) == (int)ETrooperAttackPos.RightTop) { 
							tDir = _tPos;
							tDir.X += 27;
							tDir.Y += 20;
							ePos = ETrooperAttackPos.RightBottom;
							return tDir;
						} else { 
							// set the point to be right top
							tDir = _tPos;
							tDir.X += 27;
							tDir.Y -= 20;
							ePos = ETrooperAttackPos.RightTop;
							return tDir;
						}
					}
			}
		}

		Vector2 LeftAttackPos(Vector2 tDir, out ETrooperAttackPos ePos)
		{
			switch(_iCurLeftAttackers) { 
				// if its zero its always the midpoint
				case 0:
					tDir = _tPos;
					tDir.X -= 25;
					ePos = ETrooperAttackPos.LeftMid;
					return tDir;
						
				// it cant be more than 1 (for now there may be more but that will be on a different type of class and not a trooper 
					// probably)
				default:
					// check if the opponent is above or below
					if(tDir.Y >= 0.0 + float.Epsilon) { 
						// check to see if the right bottom is taken 
						if((_byAttakPos & (int)ETrooperAttackPos.LeftBottom) == (int)ETrooperAttackPos.LeftBottom) {
							// set the point to be left top
							tDir = _tPos;
							tDir.X -= 27;
							tDir.Y -= 20;
							ePos = ETrooperAttackPos.LeftTop;
							return tDir;
						// send out left bottom
						} else { 
							tDir = _tPos;
							tDir.X -= 27;
							tDir.Y += 20;
							ePos = ETrooperAttackPos.LeftBottom;
							return tDir;
						}
					} else { 
						// the opponent is above us, check to see if our left top is taken
						if((_byAttakPos & (int)ETrooperAttackPos.LeftTop) == (int)ETrooperAttackPos.LeftTop) { 
							// send out the bottom left 
							tDir = _tPos;
							tDir.X -= 27;
							tDir.Y += 20;
							ePos = ETrooperAttackPos.LeftBottom;
							return tDir;
						} else { 
							// set the point to be right top
							tDir = _tPos;
							tDir.X -= 27;
							tDir.Y -= 20;
							ePos = ETrooperAttackPos.LeftTop;
							return tDir;
						}
					}
			}
		}

		public Vector2 RequestAttackPoint(ICombatant nCombatant, out ETrooperAttackPos ePos)
		{
			// determine up down left an right from combatant
			Vector2		tDir = _tPos - nCombatant.tPos;
			
			if(nCombatant.tPos.X < 0 || nCombatant.tPos.Y < 0) 
				tDir.ToString();

			// check left or right 
			if(tDir.X >= 0.0 + float.Epsilon) { 
				// check to see if our right positions are filled 
				if(_iCurRightAttakers < 2) { 
					return RightAttackPos(tDir, out ePos);
				} else return LeftAttackPos(tDir, out ePos);
			// we are left
			} else { 
				if(_iCurLeftAttackers < 2) { 
					return LeftAttackPos(tDir, out ePos);
				} else return RightAttackPos(tDir, out ePos);
			}
		}

		public void RemoveAttacker(ETrooperAttackPos ePos, float fXPos)
		{
			_byAttakPos &= (byte)~ePos;
			if(fXPos < 0 - float.Epsilon)
				--_iCurRightAttakers;
			else --_iCurLeftAttackers;
		}

		#endregion

		public void Process(GameTime cTime)
		{
			_cActionMgr.Process(cTime);
		}
	}
}
