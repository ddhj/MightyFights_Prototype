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
					_tRotationFlip,
					_tCurrentFlipCount,
					_tLifetime;

		string		_sDamage;
		Color		_tTextColor;
		int			_iAlpha;
		float		_fRotIncrement,
					_fRot;
		Vector2		_tPos,
					_tOrigin = new Vector2(0, 0);
		SpriteFont	_cFont;

		public bool bActive		{ get; set; }

		public AnimatingDamage(int iDamage, Vector2 tPos)
		{
			_tTotalDuration = TimeSpan.FromMilliseconds(4000);
			_tRotationFlip = TimeSpan.FromMilliseconds(200);
			_tLifetime = TimeSpan.Zero;
			_tCurrentFlipCount = TimeSpan.Zero;
			this.bActive = true;
			_fRot = (float)Math.PI / 4;
			_fRotIncrement = (float)(Math.PI / 2) / 200;
			_sDamage = Convert.ToString(iDamage);
			_tPos = tPos;
			_cFont = DataStore.cInstance.cFont;
			_tTextColor = Color.White;
		}

		public void Update(GameTime cTime)
		{
			_tLifetime += cTime.ElapsedGameTime;
			_tCurrentFlipCount += cTime.ElapsedGameTime;

			if(_tLifetime > _tTotalDuration) {
				this.bActive = false;
				return;
			}

			if(_tCurrentFlipCount > _tRotationFlip) { 
				_fRotIncrement = -_fRotIncrement;
				_tCurrentFlipCount = TimeSpan.Zero;
				_tPos.Y -= 3;
			}

			_fRot += _fRotIncrement;
		}

		public void Draw(SpriteBatch cBatch)
		{
			cBatch.DrawString(_cFont, _sDamage, _tPos, _tTextColor, _fRot, _tOrigin, 1, SpriteEffects.None, 1);
		}
	}
}
