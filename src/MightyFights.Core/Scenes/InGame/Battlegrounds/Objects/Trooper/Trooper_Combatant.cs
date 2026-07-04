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

			//// ddhj: kaiju -- GetVectByPos positions attack points using the TARGET's own
			//// scale (an oversized target's rendezvous points sit proportionally farther from
			//// its true center), but this arrival tolerance was still the attacker's small,
			//// fixed base reach. With only 6 named attack slots (Trooper_Combatant.cs's
			//// Left/RightAttackPos) and a kaiju allowing far more simultaneous attackers than
			//// that (_iAvailablePositions), several troopers can end up assigned the literal
			//// same point once Top/Bottom fill and everyone else falls back to Mid -- they
			//// can't all physically stand there, so they kept drifting outside the old fixed
			//// tolerance and endlessly re-entering/re-choosing (looked like "attacking but
			//// missing"). Scaling the tolerance by the target's own size gives real slack for
			//// that unavoidable crowding around a much bigger target's rendezvous points.
			float		fRangeScale = nOpponent.fCombatantScale;

			if( bCollision )
				return(((tDest - _tCenter).LengthSquared()) < ( this.iWeaponRngSq * 1.3f * fRangeScale ));
			return(((tDest - _tCenter).LengthSquared()) < this.iWeaponRngSq * fRangeScale );
		}

		public override bool IsDead()
		{
			return cAiData.eState == EBattleAiStates.Dying || cAiData.eState == EBattleAiStates.Dead;
		}

		public override void SetAttacker(Combatant nCombatant, out int iPos)
		{
			GetAttackPoint(nCombatant, out iPos);
			_cAttackers.Add(iPos, nCombatant);
			_baSlotTaken[iPos] = true;
		}

		public override void RemoveAttacker(int iPos)
		{
			if(_baSlotTaken != null && iPos >= 0 && iPos < _baSlotTaken.Length)
				_baSlotTaken[iPos] = false;
			_cAttackers.Remove(iPos);
		}

		//// ddhj: kaiju (owner-directed) -- replaces the old fixed 6-slot Left/Right x Top/Mid/
		//// Bottom enum+bitmask system. _iAvailablePositions is the "rated strength" knob (a
		//// design decision, independent of visual _fScale): however many attack slots it's set
		//// to, that many rendezvous points are generated, evenly spaced in a full circle around
		//// this unit's tCenter. A normal Trooper's default (6) reproduces roughly the old
		//// system's spacing; a kaiju set much higher actually gets that many distinct points
		//// instead of everyone piling onto a handful of fixed ones once Top/Bottom filled (the
		//// old fallback-to-Mid behavior, which is what was causing "attacking but missing" --
		//// several attackers assigned the literal same coordinate can't all stand there, so they
		//// kept drifting outside InWeaponRange's tolerance). Bonus: the old system had a blind
		//// spot at exactly +/-90 degrees (no Top/Bottom-only slot existed, only Left/Right diagonals)
		//// -- full-circle placement fills that in naturally instead of leaving it as a gap.
		void EnsureSlotArray()
		{
			if(_baSlotTaken == null || _baSlotTaken.Length != _iAvailablePositions)
				_baSlotTaken = new bool[_iAvailablePositions];
		}

		Vector2 GetVectByPos(int iSlotIndex)
		{
			EnsureSlotArray();

			float	fAngle = MathHelper.TwoPi * iSlotIndex / _iAvailablePositions;
			float	fRadius = ( 10 + iWeaponRange ) * _fScale;
			Vector2	tDir = _tCenter;

			tDir.X += (float)Math.Cos(fAngle) * fRadius;
			tDir.Y += (float)Math.Sin(fAngle) * fRadius;

			return tDir;
		}

		// shortest signed angular distance from fFrom to fTo, wrapped into [-pi, pi]
		static float AngleDelta(float fFrom, float fTo)
		{
			float	fDelta = fTo - fFrom;
			while(fDelta > Math.PI) fDelta -= MathHelper.TwoPi;
			while(fDelta < -Math.PI) fDelta += MathHelper.TwoPi;
			return fDelta;
		}

		void GetAttackPoint( Combatant nCombatant, out int iPos)
		{
			EnsureSlotArray();

			// the direction the attacker is actually approaching from decides which free slot
			// is the best fit -- closest angle to their real position, not a fixed category
			Vector2	tDir = nCombatant.tCenter - _tCenter;
			float	fIdealAngle = (float)Math.Atan2(tDir.Y, tDir.X);

			int		iBest = -1;
			float	fBestDelta = float.MaxValue;

			for(int i = 0; i < _iAvailablePositions; ++i) {
				if(_baSlotTaken[i])
					continue;

				float	fSlotAngle = MathHelper.TwoPi * i / _iAvailablePositions;
				float	fDelta = Math.Abs(AngleDelta(fIdealAngle, fSlotAngle));

				if(fDelta < fBestDelta) {
					fBestDelta = fDelta;
					iBest = i;
				}
			}

			// every slot taken shouldn't normally happen (bAvailablePos gates assignment before
			// this is ever called) but fall back to slot 0 rather than an invalid index
			iPos = iBest >= 0 ? iBest : 0;
		}

		public override Vector2 RequestAttackPoint(Combatant nCombatant, out int iPos)
		{
			GetAttackPoint( nCombatant, out iPos );
			return GetVectByPos(iPos);
		}

		public override Vector2 RequestPersuitPoint(int iPos)
		{
			return GetVectByPos(iPos);
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