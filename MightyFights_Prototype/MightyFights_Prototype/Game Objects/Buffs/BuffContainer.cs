using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MightyFights_Prototype
{
	public class BuffContainer : ClickableSprite, IActiveBasic
	{
		BasicBuff	_cBuff;
		int			_iBuffCount;
		SpriteFont	_cFont;
		Vector2		_tBuffCountPos;

		public override Vector2 tPos { get; set; }

		public BuffContainer(Vector2 tPos)
		{
			_cFont = DataStore.cInstance.cFont;
			this.tPos = tPos;

			// set the text draw offset in the upper right 
			_tBuffCountPos = new Vector2(tPos.X + 50, tPos.Y + 5);
			this.cDrawnRect = new Rectangle((int)tPos.X, (int)tPos.Y, (int)EConstants.BuffContainerWidth, (int)EConstants.BuffContainerHeight);
		}

		public void AddBuff(BasicBuff cBuff)
		{
			if(_cBuff == null)
				_cBuff = cBuff;
			++_iBuffCount;

			if(cBuff.cFrame.bRot) 
			    cBuff.tPos = new Vector2(this.tPos.X + this.cTexRef.Bounds.Width / 2 - cBuff.cFrame.tRect.Height / 2, 
			        this.tPos.Y + this.cTexRef.Bounds.Height / 2 - cBuff.cFrame.tRect.Width / 2);
			else cBuff.tPos = new Vector2(this.tPos.X + this.cTexRef.Bounds.Width / 2 - cBuff.cFrame.tRect.Width / 2, 
			        this.tPos.Y + this.cTexRef.Bounds.Height / 2 - cBuff.cFrame.tRect.Height / 2);

		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

			if(_iBuffCount > 0) { 
				_cBuff.Draw(cBatch);
				cBatch.DrawString(_cFont, _iBuffCount.ToString(), _tBuffCountPos, Color.White);
			}
		}

		public void ProcessClick(object oSender, object oArgs)
		{
			BattlegroundData		cBattleData = (BattlegroundData)oSender;

			if(_iBuffCount > 0) { 
				// remove a sprite from the list
				--_iBuffCount;

				if(_iBuffCount == 0)
					_cBuff = null;
			} 
		}
	}
}
