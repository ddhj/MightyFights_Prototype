using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using ProjectMercury;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public partial class Priest : IHealer, IDrawable, IDrawableTexture, IAnimate, IActiveBasic
	{
		bool Upkeep(Action cAction, GameTime cTime) 
		{
			Random		cRand = DataStore.cInstance.cRand;

			if(_cBtlGndData.eState == EBattlegroundState.Battle) { 
				switch(_eState) { 
					case EHealerStates.Healing: { 
						// process the effects 
						_cHealingFx.Trigger( new Vector2( _tCenter.X - (_cTeam.bDirection ? 50 : -80) + ( _cTeam.bDirection ? 1 : -1 ) * 100, _tCenter.Y + 70 ));
		
						if(!_cAnimProc.bActive) { 
							if(cRand.Next(5) == 1)
								_cAnimProc.SetAnimationCriteria("Idle", "Normal", "chaplain_blink", 1);
							else _cAnimProc.SetAnimationCriteria("Idle", "Normal", "chaplain_mainidle", -1);
	
							_eState = EHealerStates.Idle;
						}
					} break;

					case EHealerStates.Idle: { 
						// check to see if we have some guys in our circle
						if(_cSupportZone.cUsedSpots.Count > 0) { 
							// can we actually heal at all
							if(_fHp > 0) { 
								// set the healing action 
								_cActMgr.AddAction(new Action(HealInit, null, null));
							} else { 
								// tell everyone to go away
								foreach( KeyValuePair<Vector2,ICombatant> tPair in _cSupportZone.cUsedSpots.Values )
									tPair.Value.RemoveHeal( );
								_cSupportZone.ResetSpots( );
							}
						// we are recharging our hp
						} else { 
							// check to see if we are at our max, if we are then no need to continue, 
							if( _fHp < _iMaxHp )
								if( _fHp + _fRegenRate > _iMaxHp )
									_fHp = _iMaxHp;
								else	_fHp += _fRegenRate;
						}
					} break;

					case EHealerStates.Recharging:
						// process the effects 
						_cRechargeFx.Trigger( new Vector2( _tCenter.X + ( _cTeam.bDirection ? -1 : 1 ) * 50, _tCenter.Y ));

						if( _fHp > _iMaxHp / 2 )
							_eState = EHealerStates.Idle;
						else	_fHp += _fRegenRate;
					break;
				}
			}

			return true;
		}

		bool HealInit(Action cAction, GameTime cTime) 
		{
			Random	cRand = DataStore.cInstance.cRand;
			if(cAction.bInit) { 
				_eState = EHealerStates.Healing;
				if(cRand.Next(4) == 1)
					_cAnimProc.SetAnimationCriteria("Heal", "Basic", "chaplain_healb", 1);
				else _cAnimProc.SetAnimationCriteria("Heal", "Basic", "chaplain_heal", 1);

				_bCooldown = true;
			}

			cAction.bConditionNotMet = false;
			return false;
		}
	}
}
