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
		List<BasicSprite>	_caBuffList = new List<BasicSprite>();

		public void AddBuff(BasicSprite cBuff)
		{
			_caBuffList.Add(cBuff);
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
			
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

			if(_caBuffList.Count > 0)
				_caBuffList[_caBuffList.Count - 1].Draw(cBatch);
		}

		public override bool ContainsPoint(Point tPoint)
		{
			if(base.ContainsPoint(tPoint)) { 
				if(_caBuffList.Count > 0) { 
					// do the quicktime event thing ... not sure how i want to do that ... probabbly spawn a 
					// quicktime object that has an animation manager and is clickable

					// remove a sprite from the list
					_caBuffList.RemoveAt(0);

					return true;
				}
			}

			return false;
		}
	}
}
