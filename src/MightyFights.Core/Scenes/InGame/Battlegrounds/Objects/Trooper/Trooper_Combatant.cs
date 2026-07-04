// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using ProjectMercury;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype	{
	public partial class Trooper	{
		public override void DealDamage(Combatant nOpponent, int iDamage, bool bCrit)
		{
			Random	cRand = DataStore.cInstance.cRand;
			++_cExpData.iAttacked;
			
			if(cAiData.eState != EBattleAiStates.Defending) {
				// this is a rough percentage of the armor class not taking into account flank
				int iFinalDamage = (int)(iDamage - _cStats.iArmorClass * (cRand.Next(60, 100) / 100f));

				// if they are attacking a monster 
				if(iFinalDamage < 0) iFinalDamage = (int)Math.Ceiling(iDamage * .10);

				_cStats.fHp -= iFinalDamage;

				//// check to see if the blade is poisioned 
				//if(nOpponent.bPoisonBlade) 
				//    // there is a percentage that they get poisioned 
				//    if(cRand.Next(3) == 1)
				//        // there should also be a check if they are already poisioned and just add to the time 
				//        // also possible that it could be a certain threshold 
				++nOpponent.cExpData.iAttackSuccess;
				if(bCrit) { 
					++_cExpData.iCritted;
					++nOpponent.cExpData.iCritSuccess;
					if( _cSfxCrit.State != SoundState.Stopped )
						_cSfxCrit.Stop( );
					_cSfxCrit.Play( );

					_cBloodSpray = DataManager.cInstance.CreateTerminatingParticleSystem( "BloodSpray" );
					_cBattleDataRef.cObjMgr.AddParticleEffect( _cBloodSpray );
				}

				// check to see if we are spawning damage numbers
				if(DataStore.cInstance.bDamageNumbers) 
					DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage(iFinalDamage, _tCenter, bCrit, false, _cTeam));
			} else { 
				// tally experience data
				++_cExpData.iDefendedAttacks;
				++nOpponent.cExpData.iAttackDefended;
				if(bCrit) {
					// this is a rough percentage of the armor class not taking into account flank
					int iFinalDamage = iDamage / 4 - _cStats.iArmorClass * (cRand.Next(80, 100) / 100);

					_cStats.fHp -= iFinalDamage;

					++_cExpData.iDefendedCrits;
					++nOpponent.cExpData.iCritsDefended;

					// check to see if we are spawning damage numbers
					if(DataStore.cInstance.bDamageNumbers) 
						DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage(iFinalDamage, _tCenter, bCrit, false, _cTeam));
				}
				else	{
					// check to see if we are spawning damage numbers
					if(DataStore.cInstance.bDamageNumbers) 
						DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage(0, _tCenter, bCrit, false, _cTeam));
				}
				if( _cSfxParry.State != SoundState.Stopped )
					_cSfxParry.Stop( );
				_cSfxParry.Play( );
			}

			if( _cStats.fHp <= 0 )
			{
				++nOpponent.cExpData.iKills;
				if( cAiData.eState == EBattleAiStates.Panting )
					++nOpponent.cExpData.iHealingKills;
				if( cAiData.eState == EBattleAiStates.Flee )
					++nOpponent.cExpData.iFleeKills;
			}

			// if I am attacking my attacker
			if( this.cTarget == nOpponent )
				return;
			// or if I am attacking someone who is attacking me
			else if( this.cTarget is Combatant && ((Combatant)this.cTarget ).cTarget == this )
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
					if( this.cTarget is Priest )
						return;
					break;
				}

			// if I'm attacking someone that isn't attacking me (and I'm being attacked), go fight one of my attackers
			if( this.cTarget != null )
				if( this.cTarget is Combatant && _bAttacking )
					((Combatant)this.cTarget ).RemoveAttacker( _iAttackingPos );
				// or if I'm healing, but I'm not weak enough to not fight back (no longer under the flee point)
				else if( this.cTarget is Priest )
					((Priest)this.cTarget ).FreeSpot( this );

			RemoveHeal( );
		}

		public override float Heal( float fHp )
		{
			// check to see if we are at max for the healing number 
			if( _cStats.fHp + fHp > _cStats.iMaxHp ) { 
				// there is something happening where its healing zero more than once and sometimes alot
				float f = _cStats.iMaxHp - _cStats.fHp;
				if(f <= 0) f.ToString();
				fHp = _cStats.iMaxHp - _cStats.fHp;
			}

			// increase our stats
			_cStats.fHp += fHp;

			if(DataStore.cInstance.bDamageNumbers) 
				DataStore.cInstance.cBattleData.cObjMgr.AddObject(new AnimatingDamage((int)Math.Round( fHp ), _tCenter, false, true, _cTeam));

			return fHp;
		}

		public override void RemoveHeal( )
		{
			this.cAiData.eState = EBattleAiStates.Ready;
			foreach( Action cAction in _cActionMgr.cActionQueue )
				cAction.bConditionNotMet = false;
			_bAttacking = false;
			this.cTarget = null;
		}

		public override bool InWeaponRange( bool bCollision )
		{
			Combatant	nOpponent = (Combatant)cTarget;
			Vector2		tDest = nOpponent.RequestPersuitPoint(_iAttackingPos);

			if( bCollision )
				return(((tDest - _tCenter).LengthSquared()) < ( this.iWeaponRngSq * 1.3f ));
			return(((tDest - _tCenter).LengthSquared()) < this.iWeaponRngSq );
		}

		public override bool IsDead()
		{
			return cAiData.eState == EBattleAiStates.Dying || cAiData.eState == EBattleAiStates.Dead;
		}

		public override void SetAttacker(Combatant nCombatant, out int iPos)
		{
			Vector2		tDir = _tCenter - nCombatant.tCenter;

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

		public override void RemoveAttacker(int iPos)
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
		}

		Vector2 GetVectByPos(ETrooperAttackPos ePos)
		{
			//// ddhj: kaiju -- the attack-slot ring is built from this unit's own body, so it
			//// scales with _fScale: attackers on an oversized unit form up around its bulk
			//// instead of standing inside it (relative location, not absolute man-sized offsets).
			Vector2 tDir = _tCenter;
			switch(ePos) {
				case ETrooperAttackPos.LeftBottom:
					tDir.X -= ( 10 + iWeaponRange / 2 ) * _fScale;
					tDir.Y += ( 10 + iWeaponRange / 2 ) * _fScale;
				break;
				case ETrooperAttackPos.LeftMid:
					tDir.X -= ( 10 + iWeaponRange ) * _fScale;
				break;
				case ETrooperAttackPos.LeftTop:
					tDir.X -= ( 10 + iWeaponRange / 2 ) * _fScale;
					tDir.Y -= ( 10 + iWeaponRange / 2 ) * _fScale;
				break;
				case ETrooperAttackPos.RightBottom:
					tDir.X += ( 10 + iWeaponRange / 2 ) * _fScale;
					tDir.Y += ( 10 + iWeaponRange / 2 ) * _fScale;
				break;
				case ETrooperAttackPos.RightMid:
					tDir.X += ( 10 + iWeaponRange ) * _fScale;
				break;
				case ETrooperAttackPos.RightTop:
					tDir.X += ( 10 + iWeaponRange / 2 ) * _fScale;
					tDir.Y -= ( 10 + iWeaponRange / 2 ) * _fScale;
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

		void GetAttackPoint( Combatant nCombatant, out int iPos)
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

		public override Vector2 RequestAttackPoint(Combatant nCombatant, out int iPos)
		{
			GetAttackPoint( nCombatant, out iPos );
			return GetVectByPos((ETrooperAttackPos)iPos );
		}

		public override Vector2 RequestPersuitPoint(int iPos)
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

			//// ddhj: kaiju -- these offsets are in unscaled frame pixels but Draw renders at
			//// _fScale, so the reference center must scale too or an oversized unit keeps a
			//// man-sized center down at its feet and every alignment mechanic built on tCenter
			//// (zones, attack slots, range checks) is off by the scale factor.
			if(cFrame.bRot) {
				_tCenter.X += (Math.Abs(tTopLeft.Y) + iHeight / 2) * _fScale;
				_tCenter.Y += (tTopLeft.X - iWidth / 2) * _fScale;
			} else {
				_tCenter.X += (Math.Abs(tTopLeft.X) + iWidth / 2) * _fScale;
				_tCenter.Y += (Math.Abs(tTopLeft.Y) + iHeight / 2) * _fScale;
			}
		}
	}
}