using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public delegate object DBattleHeuristic(BattlegroundData cData);

	public class AiBattleData
	{
		Dictionary<EBattleHeuristics, DBattleHeuristic>	_cHeuristics = new Dictionary<EBattleHeuristics,DBattleHeuristic>();

		public Dictionary<EBattleHeuristics, DBattleHeuristic>	cHeurisitics	{ get { return _cHeuristics; }}
		public EBattleAiStates									eState			{get; set;}
	}

	public class IntPoint : IComparable<IntPoint>
	{
		int			_iX,
					_iY;

		public int iX { get { return _iX; } set { _iX = value; }}
		public int iY { get { return _iY; } set { _iY = value; }}

		public IntPoint(int iX, int iY)
		{
			_iX = iX;
			_iY = iY;
		}

		public IntPoint(Point tPoint)
		{
			_iX = tPoint.X;
			_iY = tPoint.Y;
		}

		public static bool operator ==(IntPoint cPoint1, IntPoint cPoint2)
		{
			return cPoint1.iX == cPoint2.iX && cPoint1.iY == cPoint2.iY;
		}

		public static bool operator !=(IntPoint cPoint1, IntPoint cPoint2)
		{
			return cPoint1.iX != cPoint2.iX || cPoint1.iY != cPoint2.iY;
		}

		public override bool Equals(object obj)
		{
			if(obj is IntPoint)
				return this == (IntPoint)obj;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IComparable<IntPoint> Members

		public int CompareTo(IntPoint cPoint)
		{
			if(_iX == cPoint.iX) { 
				return _iY - cPoint.iY;
			} else { 
				return _iX - cPoint.iX;
			}
		}

		#endregion
	}
	
	public class Zone 
	{
		int			_iId;
		IntPoint	_cPoint;
	
		public int iId	{ get { return _iId; }}
		public int iX	{ get { return _cPoint.iX; } set { _cPoint.iX = value; }}
		public int iY	{ get { return _cPoint.iY; } set { _cPoint.iY = value; }}
		public IntPoint	cPoint		{ get { return _cPoint; } set { _cPoint = value; }}
		public List<List<ICombatant>>	naCombatantLists { get; set; }

		public Zone(int iId, int iX, int iY)
		{
			_iId = iId;
			_cPoint = new IntPoint(iX, iY);
		}
	}

	public class BattlegroundData
	{	
		Zone[][]		_caBattleZones = new Zone[8][];
		List<Team>		_caTeams = new List<Team>( );
		List<Dictionary<int, Zone>>	_caActiveZones = new List<Dictionary<int, Zone>>();


		public Cursor cCursor		{ get; set; }
		public List<Team> caTeams	{ get { return _caTeams; }}
		public Dictionary<EBuffEffects, BuffContainer> cBuffContainers	{ get; set; }
		public Zone[][] caBattleZones			{ get { return _caBattleZones; }}
		public EBattlegroundState eState		{ get; set; }
		public ObjectManager cObjMgr	{ get; set; }
		public DProcessClick dlBuffClick		{ get; set; }
		public DProcessClick dlDropClick		{ get; set; }
		public BuffAnimal cAnimalEffect			{ get; set; }
		
		public List<Dictionary<int, Zone>>	caActiveZones		{ get { return _caActiveZones; }}


		public BattlegroundData()
		{
			Zone	cTmpZone;

			// init the zone objects
			for(int iZoneCol = 0; iZoneCol < (int)EZoneData.ZoneColumns; ++iZoneCol) { 
				_caBattleZones[iZoneCol] = new Zone[(int)EZoneData.ZoneRows];
				for(int iZoneRow = 0; iZoneRow < (int)EZoneData.ZoneRows; ++iZoneRow) { 
					cTmpZone = new Zone(iZoneRow * (int)EZoneData.ZoneRows + iZoneCol, iZoneCol, iZoneRow);
					cTmpZone.naCombatantLists = new List<List<ICombatant>>();
					
					// allocate the army list 
					cTmpZone.naCombatantLists.Add(new List<ICombatant>());
					// allocate the opponent list
					cTmpZone.naCombatantLists.Add(new List<ICombatant>());

					// set the zone 
					_caBattleZones[iZoneCol][iZoneRow] = cTmpZone;
				}
			}

			// init the master army lists
			_caTeams = new List<Team>( );
			_caTeams.Add( new Team( 0, true ));
			_caTeams.Add( new Team( 1, false ));

			// add the army active zone list
			_caActiveZones.Add(new Dictionary<int, Zone>());

			// add the opponent active zone list
			_caActiveZones.Add(new Dictionary<int, Zone>());

		}

		public void SetZone(ICombatant nCombatant)
		{
			// get the position of the trooper in the zones 
			int		iXPos = ((int)nCombatant.tCenter.X - 112) / (int)EZoneData.ZoneColWidth, 
					iYPos = ((int)nCombatant.tCenter.Y - 70) / (int)EZoneData.ZoneRowHeight;

			Zone	cZone = nCombatant.cZone;

			// check to see if we have a zone at all 
			if(nCombatant.cZone == null) { 
				// this check is for the start 
				if(iXPos < 0 || iYPos < 0 || iXPos >= (int)EZoneData.ZoneColumns || iYPos >= (int)EZoneData.ZoneRows)
					return;

				cZone = nCombatant.cZone = caBattleZones[iXPos][iYPos];

				// add combatant to the zone list 
				caBattleZones[iXPos][iYPos].naCombatantLists[nCombatant.cTeam.iId].Add(nCombatant);
			} else {
				// check to see if we are in the same zone 
				if(nCombatant.cZone.iX != iXPos || nCombatant.cZone.iY != iYPos) { 
					// remove the combatant from the zone
					nCombatant.cZone.naCombatantLists[nCombatant.cTeam.iId].Remove(nCombatant);

					// check to see if the zone is empty 
					if(nCombatant.cZone.naCombatantLists[nCombatant.cTeam.iId].Count == 0) 
						// remove this zone as an active zone for the army 
						_caActiveZones[nCombatant.cTeam.iId].Remove(nCombatant.cZone.iId);

					// set the new zone 
					cZone = nCombatant.cZone = caBattleZones[iXPos][iYPos];

					// add combatant to the zone list 
					caBattleZones[iXPos][iYPos].naCombatantLists[nCombatant.cTeam.iId].Add(nCombatant);
				}
			}
							
			// check to see if this zone is already in the list 
			if(!_caActiveZones[nCombatant.cTeam.iId].ContainsKey(cZone.iId))
				_caActiveZones[nCombatant.cTeam.iId].Add(cZone.iId, cZone);
		}

		public void RemoveDeadCombatant(ICombatant nCombatant)
		{
			Zone	cZone;

			// decrement the active count on the army 
			nCombatant.cTeam.cActiveList.Remove(nCombatant.iId);
			
			// use the combatants zone reference to remove them from the list
			cZone = caBattleZones[nCombatant.cZone.iX][nCombatant.cZone.iY];
			cZone.naCombatantLists[nCombatant.cTeam.iId].Remove(nCombatant);

			// check to see if the zone is empty for this army 
			if(cZone.naCombatantLists[nCombatant.cTeam.iId].Count == 0)
				_caActiveZones[nCombatant.cTeam.iId].Remove(cZone.iId);
		}

		public Zone GetZoneByCoords(int iX, int iY)
		{
			if(iX < 0 || iY < 0 || iX >= (int)EZoneData.ZoneColumns || iY >= (int)EZoneData.ZoneRows)
				return null;

			return _caBattleZones[iX][iY];
		}

		public Zone GetZoneByPosition(int iX, int iY)
		{
			int iXPos = (iX - 112) / (int)EZoneData.ZoneColWidth, 
				iYPos = (iY - 70) / (int)EZoneData.ZoneRowHeight;

			if(iXPos < 0)
				iXPos = 0;
			else if(iXPos >= (int)EZoneData.ZoneColumns)
				iXPos = (int)EZoneData.ZoneColumns - 1;

			if(iYPos < 0)
				iYPos = 0;
			else if(iYPos >= (int)EZoneData.ZoneRows)
				iYPos = (int)EZoneData.ZoneRows - 1;

			return _caBattleZones[iXPos][iYPos];
		}

		public void Clear()
		{
			foreach(Zone[] caZoneCols in _caBattleZones)
				foreach(Zone cZone in caZoneCols)
					foreach(List<ICombatant> naCombatants in cZone.naCombatantLists)
						naCombatants.Clear();

			foreach(Dictionary<int, Zone> caActiveZones in _caActiveZones)
				caActiveZones.Clear();

			foreach( Team cTeam in _caTeams )
				cTeam.Clear( );
		}

		void ApplyBuff(BasicBuff cBuff, IBuffableObject nCom)
		{
			// put the buff on the combatant
			nCom.AddBuff(cBuff);

			// set the animal effect
			this.cAnimalEffect = ObjectCreationManager.cInstance.CreateAnimalEffect(cBuff.eType);
		}

		public void ApplyBuffTeam(BasicBuff cBuff)
		{
			// walk the team and 
			foreach(KeyValuePair<int, ICombatant> tCombatant in _caTeams[0].cActiveList) 
				ApplyBuff(cBuff, tCombatant.Value);
		}

		public void ApplyBuffTeamRadius(int iRadius, BasicBuff cBuff, Point tStartPos)
		{
			// get the zone we are in and the surrounding ones based on the radius
			Zone	cZone = GetZoneByPosition(tStartPos.X, tStartPos.Y),
					cLeftZone,
					cRightZone,
					cTopZone,
					cBottomZone;

			Vector2	tRadVect = new Vector2(tStartPos.X + iRadius, tStartPos.Y),
					tCentVect = new Vector2(tStartPos.X, tStartPos.Y);
			
			float	fDstSq = iRadius * iRadius;

			List<ICombatant>	naTeamInRad = new List<ICombatant>();

			// check to see if we are out of the zoned areas
			if(cZone == null) return;

			// get the zones by radius
			cLeftZone = GetZoneByPosition(tStartPos.X - iRadius, tStartPos.Y);
			cRightZone = GetZoneByPosition(tStartPos.X + iRadius, tStartPos.Y);
			cTopZone = GetZoneByPosition(tStartPos.X, tStartPos.Y - iRadius);
			cBottomZone = GetZoneByPosition(tStartPos.X, tStartPos.Y + iRadius);

			if(cRightZone.iX - cLeftZone.iX > 0) {
				// walk the zones and apply the buff
				for(int iX = cLeftZone.iX; iX <= cRightZone.iX; ++iX)
					for(int iY = cTopZone.iY; iY < cBottomZone.iY; ++iY) { 
						// get all the guys on the left team in this zone
						if(_caBattleZones[iX][iY].naCombatantLists[0].Count > 0) 
							naTeamInRad.AddRange(_caBattleZones[iX][iY].naCombatantLists[0]);
					}
			} else	naTeamInRad.AddRange( cZone.naCombatantLists[0] );

			// walk the list of guys 
			foreach(ICombatant nCom in naTeamInRad) 
				// check dst squared 
				if((tCentVect - nCom.tCenter).LengthSquared() < fDstSq)
					// we are in our circle, so apply the buff to the guy
					ApplyBuff(cBuff, nCom);
		}

		public void CreateDrop(ICombatant nCombatant)
		{
			if(DataStore.cInstance.cRand.Next(10) > 6)
				this.cObjMgr.AddClickObject(ObjectCreationManager.cInstance.CreateDrop("hammer", this), dlDropClick);
			else this.cObjMgr.AddClickObject(ObjectCreationManager.cInstance.CreateBuff(nCombatant, this), dlBuffClick);
		}

		public Vector2 GetFleeSpot( ICombatant nCombatant )
		{
			Random		cRand = DataStore.cInstance.cRand;
			int		iX = 0,
					iY = 0,
					iXOff = 0,
					iYOff = 0;
			switch( cRand.Next( 3 ))
			{
			// side
			case 0:
				iX = cRand.Next( 2 );
				iX = nCombatant.cTeam.iId == 0 ? iX : ((int)EZoneData.ZoneColumns - 2 + iX );
				iY = 1 + cRand.Next((int)EZoneData.ZoneRows - 2 );
				break;

			// top
			case 1:
				iX = cRand.Next( 2 );
				iX = nCombatant.cTeam.iId == 0 ? iX : ((int)EZoneData.ZoneColumns - 2 + iX );
				iY = 0;
				break;

			// bottom
			case 2:
				iX = cRand.Next( 2 );
				iX = nCombatant.cTeam.iId == 0 ? iX : ((int)EZoneData.ZoneColumns - 2 + iX );
				iY = (int)EZoneData.ZoneRows - 1;
				break;
			}
			if( iX == (int)EZoneData.ZoneColumns || iY == (int)EZoneData.ZoneRows )
				iX.ToString( );
			if( iX == 0 )
				iXOff = 20;
			else if( iX == ((int)EZoneData.ZoneColumns - 1 ))
				iXOff = -20;

			if( iY == 0 )
				iYOff = 20;
			else if( iY == ((int)EZoneData.ZoneRows - 1 ))
				iYOff = -20;

			return new Vector2( iX * (int)EZoneData.ZoneColWidth + 112 + cRand.Next((int)EZoneData.ZoneColWidth - Math.Abs( iXOff )) + iXOff,
											iY * (int)EZoneData.ZoneRowHeight + 70 + cRand.Next((int)EZoneData.ZoneRowHeight - Math.Abs( iYOff )) + iYOff );
		}
	}
}
