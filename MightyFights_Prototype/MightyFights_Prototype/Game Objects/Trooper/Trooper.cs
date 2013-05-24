using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public partial class Trooper : IDrawable, IDrawableTexture, IAnimate, ICombatant, IActive<Trooper>, IObject, IBuffableObject
	{	
		Vector2		_tPos,
					_tCenter;
		Texture2D	_cTexRef;
		Stats		_cStats,
					_cInitialStats;
		Team		_cTeam;
		int			_iAvailablePositions = 6,
					_iCurLeftAttackers = 0,
					_iCurRightAttakers = 0,
					_iId,
					_iAttackingPos;
		float		_fZorder;
		byte		_byAttakPos;
		bool		_bAttacking;

		EObjectStates				_eObjState;
		ActionManager<Trooper>		_cActionMgr;
		AnimationProcessor			_cAnimProc;
		BattlegroundData			_cBattleDataRef = null;

		Dictionary<EBuffEffects, BuffActionData>			_cBuffList = new Dictionary<EBuffEffects,BuffActionData>();
		Dictionary<ETrooperAttackPos, ICombatant>	_cAttackers = new Dictionary<ETrooperAttackPos,ICombatant>();

		////ddhj template stuff... not sure if it should go on trooper proper
		float						_fFinalMovementSpeed;

		public int iId					{ get; set; }
		public bool bActive				{ get; set; }
		public bool bDir				{ get; set; }
		public bool bPoisonBlade		{ get; set; }
		public IBattleObj nTarget		{ get; set; }
		public AiBattleData cAiData		{ get; set; }
		public int iWeaponRange			{ get; set; }
		public int iWeaponRngSq			{ get; set; }
		public Zone cZone				{ get; set; }
		public Team cTeam				{ get { return _cTeam; }}
		public EObjectStates eObjState	{ get { return _eObjState; } set { _eObjState = value; }}
		public Stats cStats				{ get { return _cStats; } set { _cStats = value; }}
		public bool bAvailablePos		{ get { return _iCurLeftAttackers + _iCurRightAttakers < _iAvailablePositions; }} 
		public Vector2 tAttackPos		{ get; set; }
		public Vector2 tCenter			{ get { return _tCenter; } set { _tCenter = value; }}
		public float fZorder			{ get { return _fZorder; }}

		//public Dictionary<EBuffEffects, BuffActionData> cBuffList		{ get { return _cBuffList; }}
		public ActionManager<Trooper>	cActionManager	{ get { return _cActionMgr; } set { _cActionMgr = value; }}

		public Trooper(int iId, Team cTeam, TrooperTemplate cTemplate)
		{
			_iId = iId;
			_cTeam = cTeam;

			_cAnimProc = cTemplate.cAnimProcessorRef;
			_cTexRef = cTemplate.cTextureRef;
			_cActionMgr = cTemplate.cActionMgr;
			_cActionMgr.cData = this;
			_cInitialStats = new Stats(cTemplate.cStats);
			_cStats = cTemplate.cStats;
			cAiData = cTemplate.cAiData;
			sTexName = cTemplate.sTexName;
			
			_cActionMgr.AddPermAction(new Action(this.TrooperUpkeep, null, null));

			////ddhj: this is the initial area for the template config, this will probably change over time
			CalcMovementSpeed();

			// use the template data to set up the heuristics... 
			//// there is a problem here since the object manager should have set this up but the heuristics are methods on an instance of trooper,
			//// considered static methods but I am not sure I like that solution
			cAiData.cHeurisitics[EBattleHeuristics.Attack] = Attack_Basic;
			cAiData.cHeurisitics[EBattleHeuristics.ChooseOpponent] = ChooseOpponent;
			cAiData.cHeurisitics[EBattleHeuristics.Flee] = Flee;
			cAiData.cHeurisitics[EBattleHeuristics.Idle] = Idle;
			cAiData.cHeurisitics[EBattleHeuristics.Pant] = Pant;
			cAiData.cHeurisitics[EBattleHeuristics.Persue] = Persue;
	
			// this is going to come from somewhere
			this.iWeaponRange = 10;
			this.iWeaponRngSq = this.iWeaponRange * this.iWeaponRange;

			// set its states
			_eObjState = EObjectStates.Active | EObjectStates.Draw;

			// get the id from the object manager proper
			this.iId = ObjectManager.cInstance.iCurObjId;
		}
		
		void CalcMovementSpeed()
		{
			////ddhj: this is the initial area for the template config, this will probably change over time
			_fFinalMovementSpeed = 2.5f * (1.0f + _cStats.iMovement / 100f);
		}

		void CalcAtackSpeeds()
		{
			int iMod;
			
			// since we are a trooper and we know that we have basic attacks we are going to walk the list of them 
			// and modify their values by our attack speed
			foreach(KeyValuePair<string, ActionData> tAction in _cAnimProc.cAnimData.cReferenceList["Attack"]["Basic"]) { 
				iMod = (int)(-tAction.Value.iIncrement * ((float)_cStats.iAtkSpeed / 100));
				_cAnimProc.cActionIncrement[tAction.Key] = iMod;
			}
		}

		#region IDrawable Members

		public Texture2D cTexRef	{ get { return _cTexRef; } set { _cTexRef = value; }}
		public Vector2 tPos			{ get { return _tPos; } set { _tPos = value; UpdateRefPoints(); }}
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

			// draw the lifebar 
			if(DataStore.cInstance.bLifeBars) { 
				Texture2D	cBorder = DataStore.cInstance.cBorder;
				Rectangle	tRect = new Rectangle((int)_tPos.X + 20, (int)_tPos.Y + 30, (int)(((_cStats.fHp / (float)_cStats.iMaxHp) * 100) * .3), 5);
				Color		cHpColor = Color.Green;
				cHpColor.A = 85;
				cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, _fZorder);
			}

			// draw the buffs above the troopers head 
			if(_cBuffList.Count > 0 && this.cAiData.eState != EBattleAiStates.Dead) { 
				int iDx = (int)this.tCenter.X;
				BuffGem		cTmpGem;
				if(_cBuffList.Count > 1) 
					iDx -= (_cBuffList.Count - 1) * 5;

				foreach(KeyValuePair<EBuffEffects, BuffActionData> tBuff in _cBuffList) { 
					cTmpGem = tBuff.Value.cBuffGem;
					cTmpGem.tPos = new Vector2(iDx, tCenter.Y - 20);
					cTmpGem.fZorder = _fZorder + .01f;
					cTmpGem.Draw(cBatch);
					iDx += 12;
				}
			}


			//{ 
			//    Texture2D	cBorder = DataStore.cInstance.cBorder;
			//    Rectangle	tRect = new Rectangle((int)_tCenter.X - 5, (int)_tCenter.Y - 5, 10, 10);
			//    Color		cHpColor = Color.White;
			//    cHpColor.A = 85;
			//    cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, _fZorder);
			//}
			//if( this.nTarget != null && this.nTarget is ICombatant )
			//{ 
			//    Texture2D	cBorder = DataStore.cInstance.cBorder;
			//    Vector2		tDest = ((ICombatant)this.nTarget ).RequestPersuitPoint( _iAttackingPos );
			//    if( tDest.X < 0 || tDest.Y < 0 || tDest.X > 1000 || tDest.Y > 600 )
			//    {
			//        Rectangle	tRect = new Rectangle((int)900, (int)0, 100, 100);
			//        Color		cHpColor = Color.Blue;
			//        cHpColor.A = 85;
			//        cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, _fZorder);
				
			//    }
			//    else	{
			//        Rectangle	tRect = new Rectangle((int)tDest.X - 5, (int)tDest.Y - 5, 10, 10);
			//        Color		cHpColor = Color.Blue;
			//        cHpColor.A = 85;
			//        cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, _fZorder);
			//    }
			//}
			//if( _cActionMgr != null )
			//    if( _cActionMgr.cActionQueue.Count > 0 )
			//    {
			//        if( _cActionMgr.cActionQueue[0].oData != null && _cActionMgr.cActionQueue[0].oData is Vector2 )
			//        {
			//            Texture2D	cBorder = DataStore.cInstance.cBorder;
			//            Rectangle	tRect = new Rectangle((int)((Vector2)_cActionMgr.cActionQueue[0].oData ).X - 5, (int)((Vector2)_cActionMgr.cActionQueue[0].oData ).Y - 5, 10, 10);
			//            Color		cHpColor = Color.Red;
			//            cHpColor.A = 85;
			//            cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, _fZorder);
			//        }
			//    }

			//// ddhj: debug draw data
			// lets draw our attack positions
			//Vector2 tDir = _tPos;
			//tColor = Color.White;
			//tColor.A = 80;
			//if((_byAttakPos & (byte)ETrooperAttackPos.LeftBottom) == (byte)ETrooperAttackPos.LeftBottom) { 
			//    tDir = _tCenter;
			//    tDir.X -= cFrame.tRect.Width / 2 + iWeaponRange;
			//    tDir.Y += 20;
			//    cBatch.Draw(cBorder, new Rectangle((int)tDir.X, (int)tDir.Y, 3, 3), tColor);
			//}
			//if((_byAttakPos & (byte)ETrooperAttackPos.LeftMid) == (byte)ETrooperAttackPos.LeftMid) { 
			//    tDir = _tCenter;
			//    tDir.X -= cFrame.tRect.Width / 2 + iWeaponRange;
			//    cBatch.Draw(cBorder, new Rectangle((int)tDir.X, (int)tDir.Y, 3, 3), tColor);				
			//}
			//if((_byAttakPos & (byte)ETrooperAttackPos.LeftTop) == (byte)ETrooperAttackPos.LeftTop) { 
			//    tDir = _tCenter;
			//    tDir.X -= cFrame.tRect.Width / 2 + iWeaponRange;
			//    tDir.Y -= 20;
			//    cBatch.Draw(cBorder, new Rectangle((int)tDir.X, (int)tDir.Y, 3, 3), tColor);				
			//}
			//if((_byAttakPos & (byte)ETrooperAttackPos.RightBottom) == (byte)ETrooperAttackPos.RightBottom) { 
			//    tDir = _tCenter;
			//    tDir.X += cFrame.tRect.Width / 2 + iWeaponRange;
			//    tDir.Y += 20;
			//    cBatch.Draw(cBorder, new Rectangle((int)tDir.X, (int)tDir.Y, 3, 3), tColor);				
			//}
			//if((_byAttakPos & (byte)ETrooperAttackPos.RightMid) == (byte)ETrooperAttackPos.RightMid) { 
			//    tDir = _tCenter;
			//    tDir.X += cFrame.tRect.Width / 2 + iWeaponRange;
			//    cBatch.Draw(cBorder, new Rectangle((int)tDir.X, (int)tDir.Y, 3, 3), tColor);				
			//}
			//if((_byAttakPos & (byte)ETrooperAttackPos.RightTop) == (byte)ETrooperAttackPos.RightTop) { 
			//    tDir = _tCenter;
			//    tDir.X += cFrame.tRect.Width / 2 + iWeaponRange;
			//    tDir.Y -= 20;
			//    cBatch.Draw(cBorder, new Rectangle((int)tDir.X, (int)tDir.Y, 3, 3), tColor);				
			//}

			// lets draw our sprite rect
			//Vector2 tVect = _tPos;
			//if(cCurFrame.bRot) { 
			//    tVect.X += Math.Abs(tTopLeft.Y);
			//    tVect.Y += tTopLeft.X - cCurFrame.tRect.Width;
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, 1, cCurFrame.tRect.Width), tColor);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X + cCurFrame.tRect.Height, (int)tVect.Y, 1, cCurFrame.tRect.Width), tColor);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, cCurFrame.tRect.Height, 1), tColor);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y + cCurFrame.tRect.Width, cCurFrame.tRect.Height, 1), tColor);
			//} else { 
			//    tVect.X += Math.Abs(tTopLeft.X);
			//    tVect.Y += Math.Abs(tTopLeft.Y);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, 1, cCurFrame.tRect.Height), tColor);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X + cCurFrame.tRect.Width, (int)tVect.Y, 1, cCurFrame.tRect.Height), tColor);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, cCurFrame.tRect.Width, 1), tColor);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y + cCurFrame.tRect.Height, cCurFrame.tRect.Width, 1), tColor);
			//}
		}

		#endregion

		#region IAnimate Members

		public AnimationProcessor cAnimationProcessor { get { return _cAnimProc; } set { _cAnimProc = value; }}

		#endregion

		#region ICombatant Methods

		public void DealDamage(ICombatant nOpponent, int iDamage, bool bCrit)
		{
			//// ddhj: yep armor class and all that shit 
			if(cAiData.eState != EBattleAiStates.Defending) { 
				Random cRand = DataStore.cInstance.cRand;
				
				// this is a rough percentage of the armor class not taking into account flank
				int iFinalDamage = iDamage - _cStats.iArmorClass * (cRand.Next(80, 100) / 100);

				_cStats.fHp -= iFinalDamage;

				// check to see if the blade is poisioned 
				if(nOpponent.bPoisonBlade) 
					// there is a percentage that they get poisioned 
					if(cRand.Next(3) == 1)
						// there should also be a check if they are already poisioned and just add to the time 
						// also possible that it could be a certain threshold 


				// check to see if we are spawning damage numbers
				if(DataStore.cInstance.bDamageNumbers) 
					DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage(iFinalDamage, _tCenter, bCrit, false, _cTeam));
			}
			else
				// check to see if we are spawning damage numbers
				if(DataStore.cInstance.bDamageNumbers) 
					DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage(0, _tCenter, bCrit, false, _cTeam));

			// if I am attacking my attacker
			if( this.nTarget == nOpponent )
				return;
			// or if I am attacking someone who is attacking me
			else if( this.nTarget is ICombatant && ((ICombatant)this.nTarget ).nTarget == this )
				return;
			// if I am not in a state to attack
			else
				switch( this.cAiData.eState )
				{
				case EBattleAiStates.Flee:
				case EBattleAiStates.Dying:
				case EBattleAiStates.Defending:
					return;

				case EBattleAiStates.Panting:
					if( _cStats.fHp < _cStats.iFleePoint )
						return;
					break;
				}

			// if I'm attacking someone that isn't attacking me (and I'm being attacked), go fight one of my attackers
			if( this.nTarget != null )
				if( this.nTarget is ICombatant && _bAttacking )
					((ICombatant)this.nTarget ).RemoveAttacker( _iAttackingPos );
				// or if I'm healing, but I'm not weak enough to not fight back (no longer under the flee point)
				else if( this.nTarget is IHealer )
					((IHealer)this.nTarget ).FreeSpot( this );

			this.cAiData.eState = EBattleAiStates.Ready;
			foreach( Action cAction in _cActionMgr.cActionQueue )
				cAction.bConditionNotMet = false;
			_bAttacking = false;
			this.nTarget = null;
		}

		public float Heal( float fHp )
		{
			//// hp should be a float at some point
			if( _cStats.fHp + fHp > _cStats.iMaxHp )
				fHp = _cStats.iMaxHp - _cStats.fHp;
			_cStats.fHp += fHp;

			if(DataStore.cInstance.bDamageNumbers) 
				DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage((int)Math.Round( fHp ), _tCenter, false, true, _cTeam));
			return fHp;
		}

		public void RemoveHeal( )
		{
			this.nTarget = null;
		}

		public bool InWeaponRange( bool bCollision )
		{
			ICombatant	nOpponent = (ICombatant)nTarget;
			Vector2		tDest = nOpponent.RequestPersuitPoint(_iAttackingPos);

			if( bCollision )
				return(((tDest - _tCenter).LengthSquared()) < ( this.iWeaponRngSq * 1.3f ));
			return(((tDest - _tCenter).LengthSquared()) < this.iWeaponRngSq );
		}

		public bool IsDead()
		{
			return cAiData.eState == EBattleAiStates.Dying || cAiData.eState == EBattleAiStates.Dead;
		}

		public void SetAttacker(ICombatant nCombatant, out int iPos)
		{
			Vector2		tDir = _tCenter - nCombatant.tCenter;

			if( _iCurLeftAttackers + _iCurRightAttakers != _cAttackers.Count )
				_byAttakPos.ToString( );
			GetAttackPoint(nCombatant, out iPos);
			_cAttackers.Add((ETrooperAttackPos)iPos, nCombatant);
			_byAttakPos |= (byte)iPos;

			// set left or right
			switch((ETrooperAttackPos)iPos) { 
				case ETrooperAttackPos.LeftBottom:
				case ETrooperAttackPos.LeftMid:
				case ETrooperAttackPos.LeftTop:
					++_iCurLeftAttackers;
				break;

				case ETrooperAttackPos.RightBottom:
				case ETrooperAttackPos.RightMid:
				case ETrooperAttackPos.RightTop:
					++_iCurRightAttakers;
				break;
			}
		}

		public void RemoveAttacker(int iPos)
		{
			_byAttakPos &= (byte)~iPos;
			_cAttackers.Remove((ETrooperAttackPos)iPos);
			
			switch((ETrooperAttackPos)iPos) { 
				case ETrooperAttackPos.LeftBottom:
				case ETrooperAttackPos.LeftMid:
				case ETrooperAttackPos.LeftTop:
					--_iCurLeftAttackers;
				break;

				case ETrooperAttackPos.RightBottom:
				case ETrooperAttackPos.RightMid:
				case ETrooperAttackPos.RightTop:
					--_iCurRightAttakers;
				break;
			}
			if( _iCurLeftAttackers + _iCurRightAttakers != _cAttackers.Count )
				_byAttakPos.ToString( );
		}

		Vector2 GetVectByPos(ETrooperAttackPos ePos)
		{
			Vector2 tDir = _tCenter;
			switch(ePos) { 
				case ETrooperAttackPos.LeftBottom:
					tDir.X -= ( 10 + iWeaponRange / 2 );
					tDir.Y += ( 10 + iWeaponRange / 2 );
				break;
				case ETrooperAttackPos.LeftMid:
					tDir.X -= 10 + iWeaponRange;
				break;
				case ETrooperAttackPos.LeftTop:
					tDir.X -= ( 10 + iWeaponRange / 2 );
					tDir.Y -= ( 10 + iWeaponRange / 2 );
				break;
				case ETrooperAttackPos.RightBottom:
					tDir.X += ( 10 + iWeaponRange / 2 );
					tDir.Y += ( 10 + iWeaponRange / 2 );
				break;
				case ETrooperAttackPos.RightMid:
					tDir.X += 10 + iWeaponRange;
				break;
				case ETrooperAttackPos.RightTop:
					tDir.X += ( 10 + iWeaponRange / 2 );
					tDir.Y -= ( 10 + iWeaponRange / 2 );
				break;
			}

			return tDir;
		}

		void RightAttackPos(Vector2 tDir, out int iPos, int iWeaponRange) 
		{
			ETrooperAttackPos	ePos;
			switch(_iCurRightAttakers) { 
			// if its zero its always the midpoint
			case 0:
				ePos = ETrooperAttackPos.RightMid;
				break;
						
			// it cant be more than 1 (for now there may be more but that will be on a different type of class and not a trooper 
				// probably)
			default:
				// check if the opponent is above or below
				if(tDir.Y >= 0.0 + float.Epsilon) { 
					// check to see if the right bottom is taken 
					if((_byAttakPos & (int)ETrooperAttackPos.RightBottom) != (int)ETrooperAttackPos.RightBottom)
						ePos = ETrooperAttackPos.RightBottom;
					// send out right bottom
					else if((_byAttakPos & (int)ETrooperAttackPos.RightTop) != (int)ETrooperAttackPos.RightTop)
						ePos = ETrooperAttackPos.RightTop;
					else	ePos = ETrooperAttackPos.RightMid;
				} else { 
					// the opponent is above us, check to see if our right top is taken
					if((_byAttakPos & (int)ETrooperAttackPos.RightTop) != (int)ETrooperAttackPos.RightTop)
						ePos = ETrooperAttackPos.RightTop;
					else if((_byAttakPos & (int)ETrooperAttackPos.RightBottom) != (int)ETrooperAttackPos.RightBottom)
						ePos = ETrooperAttackPos.RightBottom;
					else	ePos = ETrooperAttackPos.RightMid;
				}
				break;
			}
			iPos = (int)ePos;
		}

		void LeftAttackPos(Vector2 tDir, out int iPos, int iWeaponRange)
		{
			ETrooperAttackPos	ePos;
			switch(_iCurLeftAttackers) { 
			// if its zero its always the midpoint
			case 0:
				ePos = ETrooperAttackPos.LeftMid;
				break;
						
			// it cant be more than 1 (for now there may be more but that will be on a different type of class and not a trooper 
				// probably)
			default:
				// check if the opponent is above or below
				if(tDir.Y >= 0.0 + float.Epsilon) { 
					// check to see if the right bottom is taken 
					if((_byAttakPos & (int)ETrooperAttackPos.LeftBottom) != (int)ETrooperAttackPos.LeftBottom)
						ePos = ETrooperAttackPos.LeftBottom;
					// send out left bottom
					else if((_byAttakPos & (int)ETrooperAttackPos.LeftTop) != (int)ETrooperAttackPos.LeftTop)
						ePos = ETrooperAttackPos.LeftTop;
					else	ePos = ETrooperAttackPos.LeftMid;
				} else { 
					// the opponent is above us, check to see if our left top is taken
					if((_byAttakPos & (int)ETrooperAttackPos.LeftTop) != (int)ETrooperAttackPos.LeftTop)
						ePos = ETrooperAttackPos.LeftTop;
					else if((_byAttakPos & (int)ETrooperAttackPos.LeftBottom) != (int)ETrooperAttackPos.LeftBottom)
						ePos = ETrooperAttackPos.LeftBottom;
					else	ePos = ETrooperAttackPos.LeftMid;
				}
				break;
			}
			iPos = (int)ePos;
		}

		void GetAttackPoint( ICombatant nCombatant, out int iPos)
		{
			// determine up down left an right from combatant
			Vector2		tDir = nCombatant.tCenter - _tCenter;

			// check left or right 
			if(tDir.X >= 0.0 + float.Epsilon) { 
				// check to see if our right positions are filled 
				if(_iCurRightAttakers < 3) { 
					RightAttackPos(tDir, out iPos, nCombatant.iWeaponRange);
				} else LeftAttackPos(tDir, out iPos, nCombatant.iWeaponRange);
			// we are left
			} else { 
				if(_iCurLeftAttackers < 3) { 
					LeftAttackPos(tDir, out iPos, nCombatant.iWeaponRange);
				} else RightAttackPos(tDir, out iPos, nCombatant.iWeaponRange);
			}
		}

		public Vector2 RequestAttackPoint(ICombatant nCombatant, out int iPos)
		{
			GetAttackPoint( nCombatant, out iPos );
			return GetVectByPos((ETrooperAttackPos)iPos );
		}

		public Vector2 RequestPersuitPoint(int iPos)
		{
			return GetVectByPos((ETrooperAttackPos)iPos);
		}

		void UpdateRefPoints()
		{
			Frame		cCurFrame = cFrame;
			Vector2		tTopLeft = cCurFrame.tTopLeft;
			int			iHeight = cCurFrame.tRect.Height,
						iWidth = cCurFrame.tRect.Width;

			if(bDir)
				tTopLeft = cCurFrame.tFlipTopLeft;

			_tCenter = _tPos;

			if(cFrame.bRot) { 
				_tCenter.X += Math.Abs(tTopLeft.Y) + iHeight / 2;
				_tCenter.Y += tTopLeft.X - iWidth / 2;
			} else { 
				_tCenter.X += Math.Abs(tTopLeft.X) + iWidth / 2;
				_tCenter.Y += Math.Abs(tTopLeft.Y) + iHeight / 2;
			}
		}

		#endregion

		void CalibrateStats()
		{
			// if there are buffs to process
			IBattleStats nStats = new Stats(_cInitialStats);

			foreach(KeyValuePair<EBuffEffects, BuffActionData> tBuff in _cBuffList)
				tBuff.Value.dlBuffEffect(nStats);

			_cStats.SetStats(nStats);

			CalcMovementSpeed();
			CalcAtackSpeeds();
		}

		public void AddBuff(BasicBuff cBuff)
		{
			// check to see if the trooper has the buff in question 
			if(_cBuffList.ContainsKey(cBuff.eType)) 
				// extend the time of the buff on the guy ( probably some upper limit??
				_cBuffList[cBuff.eType].tCurrentSpan -= BuffActions.GetTimeSpan(cBuff.eType);
			// there is no buff so create one 
			else  { 
				BuffActionData cActionData;
				_cBuffList.Add(cBuff.eType, cActionData = new BuffActionData(BuffActions.GetTimeSpan(cBuff.eType), cBuff.eType, BuffActions.GetMethod(cBuff.eType)));
				_cActionMgr.AddPermAction(new Action(cActionData.BuffAction, cActionData, this));

				// a new stat buff has been added calibrate the stats to reflect it
				CalibrateStats();
			}

		}

		public void RemoveBuff(EBuffEffects eType)
		{
			_cBuffList.Remove(eType);

			// since one has now been removed recalibrate the stats
			CalibrateStats();
		}

		public void Process(GameTime cTime)
		{
			_cActionMgr.Process(cTime);

			// walk the buff list and animate any of them if they are on the combatant
			foreach(KeyValuePair<EBuffEffects, BuffActionData> tBuff in _cBuffList)
				tBuff.Value.cBuffGem.Process(cTime);
		}
	}
}
