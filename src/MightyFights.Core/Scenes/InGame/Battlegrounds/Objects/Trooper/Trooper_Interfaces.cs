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
	// IActive
		public void Process(GameTime cTime)
		{
			_cActionMgr.Process(cTime);

			// walk the buff list and animate any of them if they are on the combatant
			foreach(KeyValuePair<EBuffEffects, BuffActionData> tBuff in _cBuffList)
				tBuff.Value.cBuffGem.Process(cTime);

			if( _cBloodSpray != null )
				if(((TerminatingParticleEffect)_cBloodSpray ).bTimeElapsed )
					_cBloodSpray = null;
				else	_cBloodSpray.Trigger( new Vector2( _tCenter.X + 0, _tCenter.Y - 18 * _fScale ));
		}

	// IDrawable Members
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
			cBatch.Draw(_cTexRef, _tPos, cCurFrame.tRect, tColor, cCurFrame.bRot ? -(float)Math.PI/2 : 0, tTopLeft, _fScale, 
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

			// draw the buffs above the troopers head unless they are dead ... then don't draw them behind the background because that looks strange
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
		}

	// IBuffable
		public override void AddBuff(BasicBuff cBuff)
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

			++_cExpData.iBuffsApplied;
			switch(cBuff.eType) { 
				case EBuffEffects.Crab_Claw:		++_cExpData.iCrabClaw;	break;
				case EBuffEffects.Dragon_Wing:		++_cExpData.iDragonWing;	break;
				case EBuffEffects.Eagle_Feather:	++_cExpData.iEagleFeather;	break;
				case EBuffEffects.Lion_Paw:			++_cExpData.iLionPaw;	break;
				case EBuffEffects.Snake_Fang:		++_cExpData.iSnakeFang;	break;
				case EBuffEffects.Squirrel_Acorn:	++_cExpData.iSquirrelAcorn;	break;
				case EBuffEffects.Toad_Eye:			++_cExpData.iToadEye;	break;
				case EBuffEffects.Wolf_Ear:			++_cExpData.iWolfEar;	break;
			}
		}

		public override void RemoveBuff(EBuffEffects eType)
		{
			_cBuffList.Remove(eType);

			// since one has now been removed recalibrate the stats
			CalibrateStats();
		}
	}
}