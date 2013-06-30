// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ProjectMercury;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype	{
	public partial class Priest : IHealer, IDrawable, IDrawableTexture, IAnimate, IActive<Priest>	
	{
	// Data
		int			_iId,
					_iMaxHp;
		float		_fHp,
					_fRegenRate,
					_fHealRate,
					_fZOrder;
		bool		_bCooldown;

		TimeSpan	_tCooldown, 
					_tUniqueIdle;
		Vector2		_tPos,
					_tCenter;
		Team		_cTeam;
		FleeSpot	_cSupportZone;
		Texture2D	_cTexRef;
		EObjectStates		_eObjState;
		EHealerStates		_eState;
		AnimationProcessor	_cAnimProc;
		BattlegroundData	_cBtlGndData;

		ParticleEffect		_cHealingFx,
							_cRechargeFx;
		SoundEffectInstance	_cHealingSfx;
		ActionManager<Priest>	_cActMgr;

	// Properties
		public int iId			{ get { return _iId; }}
		public float fHealth	{ get { return _fHp / _iMaxHp; }}
		public float fHp		{ get { return _fHp; } set { _fHp = value; }}
		public Vector2 tPos		{ get { return _tPos; } set { _tPos = value; UpdateRefPoints(); }}
		public Vector2 tCenter	{ get { return _tCenter; } set { _tCenter = value; }}
		public bool bActive		{ get { return _eState != EHealerStates.Recharging; }}
		public EObjectStates eObjState	{ get { return _eObjState; } set { _eObjState = value; }}
		public bool bAvailableSpots		{ get { return _cSupportZone.bOpen; }}
		public AnimationProcessor cAnimationProcessor { get { return _cAnimProc; } set { _cAnimProc = value; }}

		public Texture2D cTexRef	{ get { return _cTexRef; } set { _cTexRef = value; }}
		public Frame cFrame			{ get { return _cAnimProc.cCurFrame; } set{}}
		public string sTexName		{ get; set; }

		public ActionManager<Priest> cActionManager { get { return _cActMgr; } set { _cActMgr = value; }}

	// Constructor
		public Priest(int iMaxHp, int iMaxSlots, float fHealRate, float fRegenRate, Vector2 tPos, Team cTeam, AnimationData cAnimData, BattlegroundData cBtlGndData)
		{
			_iId = ObjectCreationManager.cInstance.iCurObjId;
			_fHp = _iMaxHp = iMaxHp;
			_fRegenRate = fRegenRate;
			_fHealRate = fHealRate;
			_fZOrder = 1 - _tCenter.Y / 684;
			_cTeam = cTeam;
			_cBtlGndData = cBtlGndData;

			_cAnimProc = new AnimationProcessor(cAnimData);
			_cAnimProc.SetAnimationCriteria("Idle", "Normal", "chaplain_mainidle", -1);
			
			// create the action manager and add the perminant actions
			_cActMgr = new PriestActMgr(_cAnimProc);
			_cActMgr.cData = this;
			_cActMgr.AddPermAction(new Action(Upkeep, null, null));

			// set its states
			_eObjState = EObjectStates.Active | EObjectStates.Draw;

			this.tPos = tPos;
			// had to change this because i scaled the healers
			_cSupportZone = new FleeSpot( iMaxSlots, 60, new Vector2( _tCenter.X - (cTeam.bDirection ? 50 : -80) + ( cTeam.bDirection ? 1 : -1 ) * 100, _tCenter.Y + 10 ));

			_cHealingFx = ObjectCreationManager.cInstance.CreateParticleSystem( "HealingCircle" );
			_cBtlGndData.cObjMgr.AddParticleEffect( _cHealingFx );
			_cRechargeFx = ObjectCreationManager.cInstance.CreateParticleSystem( "HealerRecharge" );
			_cBtlGndData.cObjMgr.AddParticleEffect( _cRechargeFx );

			_cHealingSfx = ObjectCreationManager.cInstance.CreateSfx( "Appear-KP-1137861048" );
//			_cHealingSfx = ObjectCreationManager.cInstance.CreateSfx( "Computer_Magic-Microsift-1901299923" );
//			_cHealingSfx = ObjectCreationManager.cInstance.CreateSfx( "Electrical_Sweep-Sweeper-1760111493" );

			_cHealingSfx.Volume = .4f;

			// add the upkeep to the perm action
		}

	// Functions
		public Vector2 GetOpenLocation( )
		{
			return _cSupportZone.tNextOpenSpot;
		}

		public void TakeSpot( ICombatant nSoldier )
		{
			_cSupportZone.TakeSpot( nSoldier );
			++nSoldier.cExpData.iHealed;
		}

		public void FreeSpot( ICombatant nSoldier )
		{
			_cSupportZone.FreeSpot( nSoldier );
		}

		public void DoDamage( float fDamage )
		{
			if( fDamage < _fHp )
				_fHp -= fDamage;
			else	_fHp = 0;
		}

		public void Heal()
		{
			if( _cHealingSfx.State == SoundState.Stopped )
				_cHealingSfx.Play( );

			// heal all the troopers in the zone
			foreach( KeyValuePair<Vector2,ICombatant> tPair in _cSupportZone.cUsedSpots.Values )
			{
				if( _fHp > 0 ) { 
					// set the exp for times healed by a healer
					++tPair.Value.cExpData.iHealed;

					if( _fHp > _fHealRate )
						_fHp -= tPair.Value.Heal( _fHealRate );
					else	_fHp -= tPair.Value.Heal( _fHp );
				}
			}

			// out of hp to give, toss everyone out
			if( _fHp <= float.Epsilon )
			{
				foreach( KeyValuePair<Vector2,ICombatant> tPair in _cSupportZone.cUsedSpots.Values )
					tPair.Value.RemoveHeal( );
				_cSupportZone.ResetSpots( );
				_eState = EHealerStates.Recharging;
			}
		}

		public void Process( GameTime cTime )
		{
			_cActMgr.Process(cTime);
		}

		public void Draw(SpriteBatch cBatch)
		{
			// store the frame of the sprite for ref in the draw since we hit it a couple of times 
			Frame			cCurFrame = cFrame;
			SpriteEffects	eEffect = SpriteEffects.None;
			Vector2			tTopLeft = cCurFrame.tTopLeft;
			Color			tColor = Color.White;

			// check to see if we need to perform a flip 
			if(!_cTeam.bDirection) {
				// if we are rotated first then we need to flip the frame differently 
				if(cCurFrame.bRot)
					// since the texture packer rotates we need to flip the sprite across the x axis 
					eEffect = SpriteEffects.FlipVertically;
			
				// the frame is not rotated so just flip on y axis
				else eEffect = SpriteEffects.FlipHorizontally;

				// use the flip top left 
				tTopLeft = cCurFrame.tFlipTopLeft;
			}

			// draw is pretty straight forward sans two issues 1: the rotation in the sprite sheet
			cBatch.Draw(_cTexRef, _tPos, cCurFrame.tRect, tColor, cCurFrame.bRot ? -(float)Math.PI/2 : 0, tTopLeft, 1.3f, eEffect, 0);

			// there may be other things to draw here like if we are in a dying state do we want to run a blink or not 

			// or particle effect drawing calls

			// draw the lifebar 
//			if( DataStore.cInstance.bLifeBars )
			{ 
				Texture2D	cBorder = DataStore.cInstance.cBorder;
				Rectangle	tRect;
				if(!_cTeam.bDirection)
					tRect = new Rectangle((int)_tPos.X + 90, (int)_tPos.Y, (int)( _fHp / _iMaxHp * 100 * .3 ), 5);
				else tRect = new Rectangle((int)_tPos.X + 10, (int)_tPos.Y, (int)( _fHp / _iMaxHp * 100 * .3 ), 5);

				Color		cHpColor = Color.Green;
				cHpColor.A = 85;
				cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
			}

			// draw the flee point 
			if( this.bActive )
				if(DataStore.cInstance.bHealSpots) { 
					foreach( Vector2 tSpot in _cSupportZone.taOpenSpots )
					{ 
						Texture2D	cBorder = DataStore.cInstance.cBorder;
						Rectangle	tRect = new Rectangle((int)tSpot.X - 5, (int)tSpot.Y - 5, 10, 10 );
						Color		cHpColor = Color.WhiteSmoke;
						cHpColor.A = 35;
						cBatch.Draw(cBorder, new Vector2(tRect.X, tRect.Y), tRect, cHpColor, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
					}
				}
		}

		void UpdateRefPoints()
		{
			Frame		cCurFrame = cFrame;
			Vector2		tTopLeft = cCurFrame.tTopLeft;
			int			iHeight = cCurFrame.tRect.Height,
						iWidth = cCurFrame.tRect.Width;

			if(_cTeam.bDirection)
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
	}
}
