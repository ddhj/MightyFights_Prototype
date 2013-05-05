using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class AnimatingDamage
	{
		TimeSpan	_tTotalDuration,
					_tLifetime;

		string		_sDamage;
		Color		_tTextColor;
		int			_iAlpha,
					_iStartY;
		Vector2		_tPos,
					_tOrigin = new Vector2(0, 0);
		SpriteFont	_cFont;

		public bool bActive		{ get; set; }

		public AnimatingDamage(int iDamage, Vector2 tPos)
		{
			_tTotalDuration = TimeSpan.FromMilliseconds(300);
			_tLifetime = TimeSpan.Zero;
			this.bActive = true;
			_sDamage = Convert.ToString(iDamage);
			_tPos = tPos;
			_cFont = DataStore.cInstance.cFont;
			_tTextColor = Color.White;
			_iStartY = (int)tPos.Y;
		}

		public void Update(GameTime cTime)
		{
			_tLifetime += cTime.ElapsedGameTime;

			if(_tLifetime > _tTotalDuration) {
				this.bActive = false;
				return;
			}

			_tPos.Y = _iStartY - (20 * ((float)_tLifetime.Ticks / _tTotalDuration.Ticks));
		}

		public void Draw(SpriteBatch cBatch)
		{
			cBatch.DrawString(_cFont, _sDamage, _tPos, _tTextColor, 0, _tOrigin, 1, SpriteEffects.None, 0);
		}
	}
}
