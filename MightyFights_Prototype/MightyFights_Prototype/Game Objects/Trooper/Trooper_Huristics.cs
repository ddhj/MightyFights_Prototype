using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public partial class Trooper
	{
		public object Idle(BattlegroundData cData)
		{
			return null;
		}

		public object Attack_Basic(BattlegroundData cData)
		{
			Random		cRand = DataStore.cInstance.cRand;
			int			iAttackPercent;
			ICombatant	nOpponent = (ICombatant)this.nTarget;

			// check to see if our opponent is living 
			if(nOpponent.IsDead()) { 
				// null out my opponent because they are dying 
				nTarget = null;
				_bAttacking = false;

				// move our state to ready which will choose another opponent
				cAiData.eState = EBattleAiStates.Ready;
				return null;
			}

			// check to see if our opponent is running by distance check
			if(nOpponent.cAiData.eState == EBattleAiStates.Flee || !InWeaponRange( true )) { 
				// one in three chance to persue rather than attack somone else 
				nOpponent.RemoveAttacker( _iAttackingPos );
				nTarget = null;
				_bAttacking = false;
				cAiData.eState = EBattleAiStates.Ready;
				return null;
			}

			// get our percent for this attack
			////ddhj: good time for the other stuff for percent attack crit weighting and all that jaz
			iAttackPercent = cRand.Next(100);

			cAiData.eState = EBattleAiStates.Attacking;

			// we are going to crit
			if(iAttackPercent > 96) { 
				++_cExpData.iCrits;
				switch(cRand.Next(2)) { 
					case 0: _cAnimProc.SetAnimationCriteria("Attack", "Critical", "bigchop", 1); break;
					case 1: _cAnimProc.SetAnimationCriteria("Attack", "Critical", "lunge", 1); break;
				}
			}
			// check how many attackers are on us
				// tank if there are too many
			else if( _iAvailablePositions < 4 )
			{
				if( iAttackPercent > ( 50 - ( 3 - _iAvailablePositions ) * 15 ))
				{ 
					++_cExpData.iDefenceAttempts;
					cAiData.eState = EBattleAiStates.Defending;
					switch(cRand.Next(2)) { 
						case 0: _cAnimProc.SetAnimationCriteria("Defend", "Parry", "lp", 1); break;
						case 1: _cAnimProc.SetAnimationCriteria("Defend", "Parry", "sp", 1); break;
					}
				}
				else	{
					++_cExpData.iBasicAttacks;
					switch(cRand.Next(3)) { 
						case 0: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "low", 1); break;
						case 1: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stab", 1); break;
						case 2: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stick", 1); break;
					}
				}
			} else { 
				++_cExpData.iBasicAttacks;
				switch(cRand.Next(3)) { 
					case 0: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "low", 1); break;
					case 1: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stab", 1); break;
					case 2: _cAnimProc.SetAnimationCriteria("Attack", "Basic", "stick", 1); break;
				}
			}

			return null;
		}

		public object Flee(BattlegroundData cData)
		{
			// check if the hp is within the run away threshold
			if(_cStats.fHp < _cStats.iFleePoint) { 
				if(cAiData.eState == EBattleAiStates.Flee || cAiData.eState == EBattleAiStates.Panting)
					return null;

				// check to see if we are engaged in an attack 
				if(_bAttacking) { 
					((ICombatant)this.nTarget ).RemoveAttacker(_iAttackingPos);
					_bAttacking = false;
				}
				this.nTarget = null;

				// remove all actions
				_cActionMgr.cActionQueue.Clear();

				Flee( );

				cAiData.eState = EBattleAiStates.Flee;
			}

			return null;
		}

		void Flee( )
		{
			SortedList<int,IHealer>	cHealersByDist = new SortedList<int,IHealer>( );
			Random		cRand = DataStore.cInstance.cRand;
			Vector2		tPos;
			int			iDst;

			++_cExpData.iFlee;
			this.nTarget = null;

			// build up available healer list by distance
			foreach( IHealer nHealer in _cTeam.cHealerList.Values )
				if( nHealer.bActive && nHealer.bAvailableSpots )
				{
					tPos = nHealer.GetOpenLocation( );
					tPos = _tPos - tPos;
					iDst = (int)tPos.LengthSquared( );
					
					// in the unlikely event that the healer is actually the same int distance away from 
					// another healer 
					if(!cHealersByDist.ContainsKey(iDst))
						cHealersByDist.Add(iDst, nHealer );
					else cHealersByDist.Add(iDst + 1, nHealer);
				}

			// add the flee to healer/point action 
			if( cHealersByDist.Count > 0 )
			{
//				this.nTarget = cHealersByDist.Values[0];
				this.nTarget = cHealersByDist.Values[cRand.Next( cHealersByDist.Count )];
				_cActionMgr.cActionQueue.Add( new Action( FleeToHealer, null, null ));
			}
			else	{
				Action	cAction = new Action(FleeToPoint, _cBattleDataRef.GetFleeSpot( this ), null);
				_cActionMgr.cActionQueue.Add(cAction);
			}
			cHealersByDist.Clear( );
			cHealersByDist = null;
		}

		public object Persue(BattlegroundData cData)
		{
			// this is where we would put in some state changing code for 
			// stamina checks and checks to see if we are going to keep persuit

			// default is charge for ever

			return null;
		}

		public object Pant(BattlegroundData cData)
		{
			// check to see if we should still be healing 
			if( this.nTarget != null )
			{
				if(_cStats.fHp > _cStats.iHealPoint)
				{
					((IHealer)this.nTarget ).FreeSpot( this );

					RemoveHeal( );
				}
			}
			// use a lower heal point for self-healing
			else if( _cStats.fHp > _cStats.iFleePoint * 4 )
				RemoveHeal( );

			// if the zone is threatened, switch to ready to choose opponent or re-flee
				// technically, you can be attacked from a nearby zone if near the edges, so there is that
				// being healed by a healer disengages the threatened response
			if( this.nTarget == null && cAiData.eState != EBattleAiStates.Ready && BuildNearbyZoneOpponenents( cZone, _cTeam.iId ^ 1 ).Count > 0 )
			{
				RemoveHeal( );
				if( _cStats.fHp < _cStats.iFleePoint )
					_cStats.fHp = _cStats.iFleePoint;
//				if( this.nTarget != null && this.nTarget is IHealer )
//					((IHealer)this.nTarget ).FreeSpot( this );
			}

			return null;
		}

		ICombatant ChooseZoneCombatantDst(List<ICombatant> naCombatants)
		{
			// lets grab the closest guy 
			////ddhj: this could technically be a heuristic if we wanted 
			Vector2		tTarget;
			int			iTmp = int.MaxValue,
						iTmp2;
			ICombatant	nNewOpponent = null;

			foreach(ICombatant nCombatant in naCombatants) { 
				// get the vector to the combatant and test if its the shortest
				tTarget = nCombatant.tPos - _tPos;
				if((iTmp2 = (int)tTarget.LengthSquared()) < iTmp) { 
					nNewOpponent = nCombatant;
					iTmp = iTmp2;
				}
			}

			return nNewOpponent;
		}

		ICombatant ChooseZoneCombatantRand(List<ICombatant> naCombatants)
		{
			int			iIndex;
			Random		cRand = DataStore.cInstance.cRand;
			ICombatant	nCombatant;
								// copy list for removal
			List<ICombatant>	naTmpList = new List<ICombatant>( naCombatants ),
								naFleeing = new List<ICombatant>( );

			// skip fleeing guys at first
			for( int iCount = 0; iCount < naTmpList.Count; ++iCount )
				if( naTmpList[iCount].cAiData.eState == EBattleAiStates.Flee )
				{
					naFleeing.Add( naTmpList[iCount] );
					naTmpList.RemoveAt( iCount );
				}

			// while there are opponents in the list
			while(naTmpList.Count > 0) { 
				// remove testing combatant
				iIndex = cRand.Next(naTmpList.Count);
				nCombatant = naTmpList[iIndex];
				naTmpList.RemoveAt( iIndex );

				// check to see if there are any positions available
				if(nCombatant.bAvailablePos)
					return nCombatant;
			}

			// if there are fleeing opponents in the list
			if(naFleeing.Count > 0) { 
				// remove testing combatant
				iIndex = cRand.Next(naFleeing.Count);
				return naFleeing[iIndex];
			}

			return null;
		}

		ICombatant ChooseZoneCombatantRand_NoFlee( List<ICombatant> naCombatants)
		{
			int			iIndex;
			Random		cRand = DataStore.cInstance.cRand;
			ICombatant	nCombatant;
								// copy list for removal
			List<ICombatant>	naTmpList = new List<ICombatant>( naCombatants );

			// skip fleeing guys at first
			for( int iCount = 0; iCount < naTmpList.Count; ++iCount )
				if( naTmpList[iCount].cAiData.eState == EBattleAiStates.Flee )
					naTmpList.RemoveAt( iCount );

			// while there are opponents in the list
			while(naTmpList.Count > 0) { 
				// remove testing combatant
				iIndex = cRand.Next(naTmpList.Count);
				nCombatant = naTmpList[iIndex];
				naTmpList.RemoveAt( iIndex );

				// check to see if there are any positions available
				if(nCombatant.bAvailablePos)
					return nCombatant;
			}

			return null;
		}

		List<ICombatant> BuildNearbyZoneOpponenents( Zone cZone, int iOppIdx )
		{
			List<ICombatant>	naNearbyList = new List<ICombatant>( ),
								naZoneList;

			for( int iY = -1; iY <= 1; ++iY )
			{
				if( this.cZone.iY + iY < 0 || this.cZone.iY + iY >= _cBattleDataRef.caBattleZones[0].Length )
					continue;

				for( int iX = -1; iX <= 1; ++iX )
				{
					if( this.cZone.iX + iX < 0 || this.cZone.iX + iX >= _cBattleDataRef.caBattleZones.Length )
						continue;
					naZoneList = _cBattleDataRef.caBattleZones[this.cZone.iX + iX][this.cZone.iY + iY].naCombatantLists[iOppIdx];
					if( naZoneList.Count > 0 )
						naNearbyList.AddRange( naZoneList );
				}
			}

			return naNearbyList;
		}
		ICombatant ChooseZoneCombatantRand_NearbyZones( Zone cZone, int iOppIdx )
		{
			int			iIndex;
			Random		cRand = DataStore.cInstance.cRand;
			ICombatant	nCombatant;
			List<ICombatant>	naNearbyList = new List<ICombatant>( ),
								naFleeing = new List<ICombatant>( );

			naNearbyList = BuildNearbyZoneOpponenents( cZone, iOppIdx );
			if( naNearbyList.Count > 0 )
			{
				// skip fleeing guys at first
				for( int iCount = 0; iCount < naNearbyList.Count; ++iCount )
					if( naNearbyList[iCount].cAiData.eState == EBattleAiStates.Flee )
					{
						naFleeing.Add( naNearbyList[iCount] );
						naNearbyList.RemoveAt( iCount );
					}

				// while there are opponents in the list
				while(naNearbyList.Count > 0) { 
					// remove testing combatant
					iIndex = cRand.Next(naNearbyList.Count);
					nCombatant = naNearbyList[iIndex];
					naNearbyList.RemoveAt( iIndex );

					// check to see if there are any positions available
					if(nCombatant.bAvailablePos)
						return nCombatant;
				}

				// if there are fleeing opponents in the list
				if(naFleeing.Count > 0) { 
					// remove testing combatant
					iIndex = cRand.Next(naFleeing.Count);
					return naFleeing[iIndex];
				}
			}
			return null;
		}

		public object ChooseOpponent(BattlegroundData cData)
		{
			int		iTmp = int.MaxValue,
					iOpponentIdx = _cTeam.iId ^ 1;

			// not sure if this is required or not but just in case 
			if(nTarget != null)
				return null;

			// check to see if I have any attackers currently attacking me 
			if(_cAttackers.Count > 0) { 
				// check the weakest of my opponents and attack them
				foreach(ICombatant nCombatant in _cAttackers.Values) { 
					if(nCombatant.cStats.fHp < iTmp) { 
						nTarget = nCombatant;
						iTmp = (int)nCombatant.cStats.fHp;
					}
				}
			// we dont have any attakers so lets check our current zone for an opponent			
			} else if(this.cZone.naCombatantLists[iOpponentIdx].Count > 0) {
				// find someone not fleeing in my own zone
				nTarget = ChooseZoneCombatantRand_NoFlee(this.cZone.naCombatantLists[iOpponentIdx]);

				if( nTarget == null )
					nTarget = ChooseZoneCombatantRand_NearbyZones( this.cZone, iOpponentIdx );
			}
			// there was either no dudes in my zone or they do not have available attack points
			if(nTarget == null) { 
				// check our army for which zones they are in and if there are any opponents there
				// lets get the closest zone to our own
				Vector2 tZone = new Vector2(cZone.iX, cZone.iY),
						tNewZone;

				Dictionary<int, Zone>	caActiveZones = cData.caActiveZones[iOpponentIdx];
				SortedList<int, List<Zone>>	cClosestZones = new SortedList<int,List<Zone>>();
				List<Zone>	caZoneList = null;

				// walk through the active zones 
				foreach(Zone cActiveZone in caActiveZones.Values) { 
					// check to see if we have any opponents in this zone 
					if(cActiveZone.naCombatantLists[iOpponentIdx].Count > 0) {
						// make a vecotor and store the zone in a sorted list by distance
						tNewZone = new Vector2(cActiveZone.cPoint.iX, cActiveZone.cPoint.iY);
						tNewZone = tZone - tNewZone;
						
						// get distnace and add to sorted list
						iTmp = (int)tNewZone.LengthSquared();
						if(!cClosestZones.TryGetValue(iTmp, out caZoneList))
							cClosestZones.Add(iTmp, caZoneList = new List<Zone>());
						
						// add zone to internal list for collision on distance
						caZoneList.Add(cActiveZone);
					}
				}

				// walk the sorted zones 
				foreach(List<Zone> caZones in cClosestZones.Values) { 
					foreach(Zone cNewZone in caZones) 
						if((nTarget = ChooseZoneCombatantRand(cNewZone.naCombatantLists[iOpponentIdx])) != null) {
							// if we have an opponent at this point we need to charge them
							cActionManager.AddAction(new Action(ChargeOpponent, null, null));
							cAiData.eState = EBattleAiStates.Pursuit;
							return null;
						}						
				}
			}

			if(nTarget != null) { 
				// if we have an opponent at this point we need to charge them
				cActionManager.AddAction(new Action(ChargeOpponent, null, null));
				cAiData.eState = EBattleAiStates.Pursuit;
			}

			return null;
		}
	}
}
