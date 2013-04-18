using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public delegate object DBattleHuristic(BattlegroundData cData);

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
		int		_iX, 
				_iY;
		
		public List<List<ICombatant>>	naCombatantLists { get; set; }

		public Zone(int iX, int iY)
		{
			_iX = iX; 
			_iY = iY;
		}

		public int iX		{ get { return _iX; } set { _iX = value; }}
		public int iY		{ get { return _iY; } set { _iY = value; }}
	}

	public class BattlegroundData
	{	
		Zone[][]		_caBattleZones = new Zone[8][];
		List<List<ICombatant>>		_naMasterLists = new List<List<ICombatant>>();

		public EBattlegroundState eState			{ get; set; }
		public List<ICombatant>	naArmyRef			{ get { return _naMasterLists[0]; }}
		public List<ICombatant> naOpponentsRef		{ get { return _naMasterLists[1]; }}
		public List<List<ICombatant>> naMasterLists	{ get { return _naMasterLists; }}
		public Zone[][] caBattleZones				{ get { return _caBattleZones; }}

		public BattlegroundData()
		{
			Zone	cTmpZone;

			// init the zone objects
			for(int iZoneCol = 0; iZoneCol < (int)EZoneData.ZoneColumns; ++iZoneCol) { 
 				_caBattleZones[iZoneCol] = new Zone[(int)EZoneData.ZoneRows];
				for(int iZoneRow = 0; iZoneRow < (int)EZoneData.ZoneRows; ++iZoneRow) { 
					cTmpZone = new Zone(iZoneCol, iZoneRow);
					cTmpZone.naCombatantLists = new List<List<ICombatant>>();
					
					// allocate the army list 
					cTmpZone.naCombatantLists.Add(new List<ICombatant>());
					// allocate the opponent list
					cTmpZone.naCombatantLists.Add(new List<ICombatant>());

					// set the zone 
					_caBattleZones[iZoneCol][iZoneRow] = cTmpZone;
				}
			}

			// init the master army list 
			_naMasterLists.Add(new List<ICombatant>());

			// init the master opponent list
			_naMasterLists.Add(new List<ICombatant>());
		}

		public void SetZone(ICombatant nCombatant)
		{
			// get the position of the trooper in the zones 
			int iXPos = (int)nCombatant.tPos.X / (int)EZoneData.ZoneColWidth, 
				iYPos = (int)nCombatant.tPos.Y / (int)EZoneData.ZoneRowHeight;

			// check to see if we have a zone at all 
			if(nCombatant.tZone == null) { 
				if(iXPos < 0 || iYPos < 0 || iXPos >= (int)EZoneData.ZoneColumns || iYPos >= (int)EZoneData.ZoneRows)
					return;

				nCombatant.tZone = caBattleZones[iXPos][iYPos];
				
				// add combatant to the zone list 
				caBattleZones[iXPos][iYPos].naCombatantLists[nCombatant.iArmyIndex].Add(nCombatant);
			} else {
				// check to see if we are in the same zone 
				if(nCombatant.tZone.iX != iXPos || nCombatant.tZone.iY != iYPos) { 
					// remove the combatant from the zone
					nCombatant.tZone.naCombatantLists[nCombatant.iArmyIndex].Remove(nCombatant);

					// set the new zone 
					nCombatant.tZone = caBattleZones[iXPos][iYPos];

					// add combatant to the zone list 
					caBattleZones[iXPos][iYPos].naCombatantLists[nCombatant.iArmyIndex].Add(nCombatant);
				}
			}
		}

		public void RemoveDeadCombatant(ICombatant nCombatant)
		{
			// decrement the active count on the army 
			_naMasterLists[nCombatant.iArmyIndex].Remove(nCombatant);
			
			// use the combatants zone reference to remove them from the list
			caBattleZones[nCombatant.tZone.iX][nCombatant.tZone.iY].naCombatantLists[nCombatant.iArmyIndex].Remove(nCombatant);
		}

		public Zone GetZoneByCoords(int iX, int iY)
		{
			if(iX < 0 || iY < 0 || iX >= (int)EZoneData.ZoneColumns || iY >= (int)EZoneData.ZoneRows)
				return null;

			return _caBattleZones[iX][iY];
		}

		public void Clear()
		{
			foreach(Zone[] caZoneCols in _caBattleZones)
				foreach(Zone cZone in caZoneCols)
					foreach(List<ICombatant> naCombatants in cZone.naCombatantLists)
						naCombatants.Clear();

			this.naArmyRef.Clear();
			this.naOpponentsRef.Clear();
		}
	}

	public class AiBattleData
	{
		Dictionary<EBattleHuristics, DBattleHuristic>	_cHuristics = new Dictionary<EBattleHuristics,DBattleHuristic>();

		public Dictionary<EBattleHuristics, DBattleHuristic>	cHurisitics  { get { return _cHuristics; }}
		public EBattleAiStates									eState		 {get; set;}
	}
}
