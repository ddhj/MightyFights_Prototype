using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class BasicBuff : ClickableSprite, IActiveBasic
	{
		AnimationData	_cAnimData;
		Vector2			_tDest;
		Color			_tAlpha = Color.White;
		TimeSpan		_tVisible = TimeSpan.FromMilliseconds(3000), 
						_tCurrentLife = TimeSpan.Zero;
		int				_iItteration = 0,
						_iItterations;

		//// this will be replaced when the buffs animate
		public string sType		{ get; set; }
		public override Vector2 tPos { get; set; }

		public BasicBuff(AnimationData cAnimData, string sBuff, int iItterations) : base()
		{
			_cAnimData = cAnimData;
			this.cFrame = _cAnimData.GetFrame(_cAnimData.GetActionData("Main", "Sub", sBuff), 0);
			_iItterations = iItterations;
			this.eObjState = EObjectStates.Active | EObjectStates.Draw;
			this.sType = sBuff;
		}

		Vector2 GetDestPos()
		{
			List<Zone>	caZones = new List<Zone>();
			Zone[][] caZoneArray = DataStore.cInstance.cBattleData.caBattleZones;
			Random cRand = new Random();
			Zone	cZone;

			// get the inactive zones
			for(int iXPos = 0; iXPos < (int)EZoneData.ZoneColumns; ++iXPos)
				for(int iYPos = 0; iYPos < (int)EZoneData.ZoneRows; ++iYPos)
					if(caZoneArray[iXPos][iYPos].naCombatantLists[0].Count + caZoneArray[iXPos][iYPos].naCombatantLists[1].Count == 0)
						caZones.Add(caZoneArray[iXPos][iYPos]);

			// randomly select an empty zone
			cZone = caZones[cRand.Next(caZones.Count)];

			// choose a vector from the zone
			return new Vector2(cRand.Next(cZone.iX * (int)EZoneData.ZoneColWidth + ((int)EZoneData.ZoneColWidth - 23)) + 112,
				cRand.Next(cZone.iY * (int)EZoneData.ZoneRowHeight + ((int)EZoneData.ZoneRowHeight - 23)) + 70);
		}

		public virtual void Process(GameTime cTime)
		{
			if(_iItteration > _iItterations) { 
				this.eObjState = 0;
				return;
			}

			_tCurrentLife += cTime.ElapsedGameTime;

			if(_tCurrentLife > _tVisible) {
				++_iItteration;
				_tCurrentLife = TimeSpan.Zero;
				this.tPos = GetDestPos();
			}
		}

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(this.cTexRef, this.tPos, this.cFrame.tRect, Color.White, this.cFrame.bRot ? -(float)Math.PI/2 : 0, 
				// and 2: the direction vector
				this.cFrame.tTopLeft, 1, SpriteEffects.None, .99f);
		}

		public override bool ContainsPoint(Point tPoint)
		{
			Rectangle tRect;
			if(this.cFrame.bRot) 
				tRect = new Rectangle((int)this.tPos.X, (int)this.tPos.Y, this.cFrame.tRect.Height, this.cFrame.tRect.Width);
			else tRect = new Rectangle((int)this.tPos.X, (int)this.tPos.Y, this.cFrame.tRect.Width, this.cFrame.tRect.Height);

			if(tRect.Contains(tPoint)) { 
				// the object has been clicked on so lets set it to be removed
				this.eObjState = 0;
				return true;
			} else return false;
		}
	}
}
