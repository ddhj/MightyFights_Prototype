using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public delegate void DBuffEffect(Stats cStats);

	public class BasicBuff : ClickableSprite, IActiveBasic, IAnimate
	{
		protected AnimationData			_cAnimData;
		protected AnimationProcessor	_cAnimProc;
		protected int				_iItteration = 0,
									_iItterations;
		Vector2			_tDest;
		Color			_tAlpha = Color.White;
		TimeSpan		_tVisible = TimeSpan.FromMilliseconds(3000), 
						_tCurrentLife = TimeSpan.Zero;

		public EBuffEffects	eType		{ get; set; }
		public override Vector2 tPos	{ get; set; }
		public override Frame cFrame	{ get { return _cAnimProc.cCurFrame; } set { }}
		public AnimationProcessor cAnimationProcessor	{ get { return _cAnimProc; } set { _cAnimProc = value; }}

		public BasicBuff(){}
		public BasicBuff(AnimationData cAnimData, EBuffEffects eType, int iItterations) : base()
		{
			string	sBuffRef = Enum.GetName(eType.GetType(), eType).ToLower().Replace("_", "");
			_cAnimData = cAnimData;
			_cAnimProc = new AnimationProcessor(cAnimData);
			_cAnimProc.SetAnimationCriteria("Main", "Sub", sBuffRef, -1);
			_iItterations = iItterations;

			this.eObjState = EObjectStates.Active | EObjectStates.Draw;
			this.eType = eType;

			tPos = GetDestPos();
		}

		protected Vector2 GetDestPos()
		{
			List<Zone>	caZones = new List<Zone>();
			Zone[][]	caZoneArray = DataStore.cInstance.cBattleData.caBattleZones;
			Random		cRand = DataStore.cInstance.cRand;
			Zone		cZone;

			// get the inactive zones
			for(int iXPos = 0; iXPos < (int)EZoneData.ZoneColumns; ++iXPos)
				for(int iYPos = 0; iYPos < (int)EZoneData.ZoneRows; ++iYPos)
					if(caZoneArray[iXPos][iYPos].naCombatantLists[0].Count + caZoneArray[iXPos][iYPos].naCombatantLists[1].Count == 0)
						caZones.Add(caZoneArray[iXPos][iYPos]);

			// randomly select an empty zone
			cZone = caZones[cRand.Next(caZones.Count)];

			// choose a vector from the zone
			return new Vector2(cRand.Next((int)EZoneData.ZoneColWidth - 23) + cZone.iX * (int)EZoneData.ZoneColWidth + 112,
				cRand.Next((int)EZoneData.ZoneRowHeight - 23) + cZone.iY * (int)EZoneData.ZoneRowHeight + 70);
		}

		public virtual void Process(GameTime cTime)
		{
			if(_iItteration > _iItterations) { 
				this.eObjState = 0;
				return;
			}

			_tCurrentLife += cTime.ElapsedGameTime;
			_cAnimProc.Process(cTime);

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
				this.cFrame.tTopLeft, 1, SpriteEffects.None, .01f);

			//// buff rect debug
			//Texture2D cBorder = DataStore.cInstance.cBorder;
			//Vector2 tVect = this.tPos;
			//Frame cCurFrame = this.cFrame;
			//Vector2 tTopLeft = cCurFrame.tTopLeft;
			//if(this.cFrame.bRot) { 
			//    tVect.X += Math.Abs(tTopLeft.Y);
			//    tVect.Y += tTopLeft.X - cCurFrame.tRect.Width;
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, 1, cCurFrame.tRect.Width), Color.White);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X + cCurFrame.tRect.Height, (int)tVect.Y, 1, this.cFrame.tRect.Width), Color.White);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, cCurFrame.tRect.Height, 1), Color.White);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y + cCurFrame.tRect.Width, this.cFrame.tRect.Height, 1), Color.White);
			//} else { 
			//    tVect.X += Math.Abs(tTopLeft.X);
			//    tVect.Y += Math.Abs(tTopLeft.Y);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, 1, cCurFrame.tRect.Height), Color.White);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X + cCurFrame.tRect.Width, (int)tVect.Y, 1, cCurFrame.tRect.Height), Color.White);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y, cCurFrame.tRect.Width, 1), Color.White);
			//    cBatch.Draw(cBorder, new Rectangle((int)tVect.X, (int)tVect.Y + cCurFrame.tRect.Height, cCurFrame.tRect.Width, 1), Color.White);
			//}
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

	public class Drop : BasicBuff
	{
		public Drop(AnimationData cAnimData, string sDrop, int iItterations) : base()
		{			
			_cAnimData = cAnimData;
			_cAnimProc = new AnimationProcessor(cAnimData);
			_cAnimProc.SetAnimationCriteria("Main", "Sub", sDrop, -1);

			_iItterations = iItterations;
			this.eObjState = EObjectStates.Active | EObjectStates.Draw;
			this.eType = eType;

			tPos = GetDestPos();
		}
	}

	public class BuffClickEvent : BasicSprite, IClickable, IDrawable, IDrawableTexture, IObject, IActiveBasic
	{
		int				_iRadius;
		BasicBuff		_cBuff;
		Point			_tClickPoint;
		Color			_tRadTrans = Color.White;
		float			_fScale;
		AnimationData	_cAnimData;

		// create the frame data on the set of the animation data
		public AnimationData cAnimData		{ get { return _cAnimData; }
			set {
				// turn the buff effect enum into buff gem, 
				//// ddhj: this really sucks and I am going to change naming convention so this shit does not happen
				//// but for now just get it working
				string	sBuffRef = Enum.GetName(typeof(EBuffEffects), _cBuff.eType);
				if(sBuffRef.Contains('_'))
					sBuffRef = sBuffRef.Substring(0, sBuffRef.IndexOf('_'));
				sBuffRef = string.Format("{0}gem", sBuffRef).ToLower();

				_cAnimData = value;
			
				int		iStartIdx = _cAnimData.cReferenceList["Main"]["Sub"][sBuffRef].iStartIndex;
				this.cFrame = _cAnimData.caFrameData[iStartIdx];
			}
		}

		public BuffClickEvent(int iRadius, BasicBuff cBuff) 
		{
			_iRadius = iRadius;
			_cBuff = cBuff;

			// get the draw data for this object
			ObjectManager.cInstance.CreateBuffRadiusObj(this);

			this.iId = ObjectManager.cInstance.iCurObjId;
			this.eObjState = EObjectStates.Draw | EObjectStates.Active;

			// set up our color so we have some transparency during draw
			_tRadTrans.A = 15;
			
			// based on the buff we are going to select the first frame of the gems and draw that as our clickable radius 
			// a normal gem has a 3.5 px radius so we are going to use that as the scale 
			_fScale = _iRadius / 3.5f;

			// set the process method for this click event to its own processor
			this.dlProcessClick = ProcessClick;
		}

		#region IClickable Members

		// this is a bit of a trick because its basically an event we want to fire on the mouse click
		// and I am abusing the clickable object system on the object manager
		public bool ContainsPoint(Point tPoint)			{ _tClickPoint = tPoint; return true; }
		public bool ContainsPoint(Vector2 tLocation)	{ _tClickPoint = new Point((int)tLocation.X, (int)tLocation.Y); return true; }

		public DProcessClick dlProcessClick		{ get; set; }

		#endregion

		#region IDrawable Members

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(this.cTexRef, this.tPos, this.cFrame.tRect, _tRadTrans, 0, this.cFrame.tTopLeft, _fScale, SpriteEffects.None, .01f);
		}

		#endregion

		void ProcessClick(object oSender, object oArgs)
		{
			DataStore.cInstance.cBattleData.ApplyBuffTeamRadius(_iRadius, _cBuff, _tClickPoint);
			this.eObjState = 0;
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			// set the position on the mouse position 
			MouseState tState = Mouse.GetState();
			int iDx = (int)(3.5 * _fScale),
				iDy = (int)(3.5 * _fScale);

			this.tPos = new Vector2(tState.X - iDx, tState.Y - iDy);
		}

		#endregion
	}

	public class BuffGem : BasicSprite, IDrawable, IDrawableTexture, IActiveBasic
	{
		Color			_tRadTrans = Color.White;
		AnimationData		_cAnimData;
		AnimationProcessor	_cAnimProc;

		public float fZorder		{ get; set; }
		public override Frame cFrame	{ get { return _cAnimProc.cCurFrame; } set { }}

		public BuffGem(AnimationData cAnimData, EBuffEffects eType)
		{
			string	sBuffRef = Enum.GetName(typeof(EBuffEffects), eType);
			if(sBuffRef.Contains('_'))
				sBuffRef = sBuffRef.Substring(0, sBuffRef.IndexOf('_'));
			sBuffRef = string.Format("{0}gem", sBuffRef).ToLower();

			_cAnimData = cAnimData;
			_cAnimProc = new AnimationProcessor(cAnimData);
			_cAnimProc.SetAnimationCriteria("Main", "Sub", sBuffRef, -1);
		}

		#region IDrawable Members

		public override void Draw(SpriteBatch cBatch)
		{
			cBatch.Draw(this.cTexRef, this.tPos, this.cFrame.tRect, _tRadTrans, 0, this.cFrame.tTopLeft, 1, SpriteEffects.None, this.fZorder);
		}

		#endregion

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			_cAnimProc.Process(cTime);
		}

		#endregion
	}

	public class BuffActionData
	{
		TimeSpan	_tLifetime,
					_tCurrentSpan = TimeSpan.Zero;

		BuffGem		_cBuffGem;

		EBuffEffects	_eType;
		
		public DBuffEffect	dlBuffEffect		{ get; set; }
		public TimeSpan		tCurrentSpan		{ get { return _tCurrentSpan; } set { _tCurrentSpan = value; }}
		public BuffGem		cBuffGem			{ get { return _cBuffGem; }}
		
		public BuffActionData(TimeSpan tLifetime, EBuffEffects eType, DBuffEffect dlBuffEffect)
		{
			_tLifetime = tLifetime;

			_eType = eType; 
			this.dlBuffEffect = dlBuffEffect;

			_cBuffGem = ObjectManager.cInstance.CreateBuffGem(eType);
		}

		public bool BuffAction(Action cAction, GameTime cTime)
		{
			_tCurrentSpan += cTime.ElapsedGameTime;
			
			if(_tCurrentSpan > _tLifetime) { 
				((Dictionary<EBuffEffects, BuffActionData>)cAction.oCanvas).Remove(_eType);
				cAction.bConditionNotMet = false;
				return false;
			}

			return true;
		}
	}

	public class BuffAnimal : BasicSprite
	{
		TimeSpan	_tCurrentLife = TimeSpan.Zero;
		Color		_tColor = Color.White;
		float		_fScale = 0f;

		public BuffAnimal(Texture2D cTexRef) 
		{
			this.cTexRef = cTexRef;
			cFrame = new Frame(cTexRef.Bounds, new Vector2(cTexRef.Bounds.Width / 2, cTexRef.Bounds.Height / 2), new Vector2(0, 0), new Vector2(0, 0), new Vector2(), 
				new Vector2(), new Vector2(), null, false, false);

			_tColor.A = 50;

			DataStore.cInstance.bSlowMo = true;
		}

		public bool Process(GameTime cTime) 
		{
			_tCurrentLife += cTime.ElapsedGameTime;

			_fScale += .025f;

			if(_tCurrentLife > TimeSpan.FromMilliseconds(2000)) { 
				this.eObjState = 0;
				DataStore.cInstance.bSlowMo = false;
				return false;
			}

			return true;
		}

		public override void Draw(SpriteBatch cBatch) 
		{
			cBatch.Draw(this.cTexRef, this.tPos, this.cFrame.tRect, _tColor, this.cFrame.bRot ? -(float)Math.PI/2 : 0, 
				// and 2: the direction vector
				this.cFrame.tCenter, _fScale, SpriteEffects.None, .01f);
		}
	}

	public static class BuffActions 
	{
		static Dictionary<EBuffEffects, TimeSpan> _cLifeTimes = new Dictionary<EBuffEffects,TimeSpan> { 
			{ EBuffEffects.Dragon_Wing, TimeSpan.FromMilliseconds(4000) }, 
			{ EBuffEffects.Toad_Eye, TimeSpan.FromMilliseconds(4000) }, 
			{ EBuffEffects.Wolf_Ear, TimeSpan.FromMilliseconds(4000) }, 
			{ EBuffEffects.Lion_Paw, TimeSpan.FromMilliseconds(4000) }, 
			{ EBuffEffects.Snake_Fang, TimeSpan.FromMilliseconds(4000) }, 
			{ EBuffEffects.Crab_Claw, TimeSpan.FromMilliseconds(4000) },
			{ EBuffEffects.Squirrel_Acorn, TimeSpan.FromMilliseconds(4000) }, 
			{ EBuffEffects.Eagle_Feather, TimeSpan.FromMilliseconds(4000) }};

		static void Dragon_Wing(Stats cStats)
		{
			cStats.iArmorClass += 3;
			cStats.iPower += 5;
			cStats.iMovement -= 1;
		}

		static void Toad_Eye(Stats cStats)
		{

		}

		static void Wolf_Ear(Stats cStats)
		{
			cStats.fCrit += .15f;
			cStats.iMovement += 3;
		}

		static void Lion_Paw(Stats cStats)
		{
			cStats.iPower += 3;
			cStats.iAtkSpeed += 4;
		}

		static void Snake_Fang(Stats cStats)
		{

		}

		static void Eagle_Feather(Stats cStats)
		{
			cStats.iMovement += 2;
			cStats.iAtkSpeed += 2;
		}

		static void Crab_Claw(Stats cStats)
		{
			cStats.iMovement -= 2;
			cStats.iPower += 4;
		}

		static void Squirrel_Acorn(Stats cStats)
		{
			cStats.iPower -= 1;
			cStats.iAtkSpeed += 4;
			cStats.iMovement += 3;
		}

		public static DBuffEffect GetMethod(EBuffEffects eType)
		{
			switch(eType) { 
				case EBuffEffects.Crab_Claw: return Crab_Claw;
				case EBuffEffects.Dragon_Wing: return Dragon_Wing;
				case EBuffEffects.Eagle_Feather: return Eagle_Feather;
				case EBuffEffects.Lion_Paw: return Lion_Paw;
				case EBuffEffects.Snake_Fang: return Snake_Fang;
				case EBuffEffects.Squirrel_Acorn: return Squirrel_Acorn;
				case EBuffEffects.Toad_Eye: return Toad_Eye;
				case EBuffEffects.Wolf_Ear: return Wolf_Ear;
			}
			return null;
		}

		public static TimeSpan GetTimeSpan(EBuffEffects eType)
		{
			return _cLifeTimes[eType];
		}
	}
}