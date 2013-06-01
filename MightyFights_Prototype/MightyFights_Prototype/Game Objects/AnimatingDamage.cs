using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class AnimatingDamage : IDrawable, IDrawableFont, IActiveBasic, IObject
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

		public int iDrawIdx		{ get; set; }
		public int iActiveIdx	{ get; set; }
		public bool bActive		{ get; set; }
		public int iId			{ get; set; }
		public EObjectStates eObjState	{ get; set; }
		public SpriteFont cFont	{ get { return _cFont; } set { _cFont = value; }}

		// not sure that the font will be something that we optimize or not
		public string sFontName { get { return @"Shared\DebugFont"; } set { }}

		public AnimatingDamage(int iDamage, Vector2 tPos, bool bCrit, bool bHeal, Team cTeam)
		{
			_tTotalDuration = TimeSpan.FromMilliseconds(300);
			_tLifetime = TimeSpan.Zero;
			this.bActive = true;
			_sDamage = Convert.ToString(iDamage);
			_tPos = tPos;
			_cFont = DataStore.cInstance.cFont;

			if( iDamage == 0 )
				_tTextColor = Color.DarkGray;
			// get text color from nature of damage and which team it was done to
			else if( !bHeal )
				if( cTeam.bDirection )
					if( bCrit )
						_tTextColor = Color.Orange;
					else	_tTextColor = Color.LightGoldenrodYellow;
				else 
					if( bCrit )
						_tTextColor = Color.Red;
					else	_tTextColor = Color.White;
			else 
				if( cTeam.bDirection )
					_tTextColor = Color.LightGreen;
				else	_tTextColor = Color.Turquoise;

			_iStartY = (int)tPos.Y;

			// set the states for this object
			this.eObjState = EObjectStates.Active | EObjectStates.Draw;

			// get the main object id
			this.iId = ObjectCreationManager.cInstance.iCurObjId;
		}

		public void Process(GameTime cTime)
		{
			_tLifetime += cTime.ElapsedGameTime;

			if(_tLifetime > _tTotalDuration) {
				this.eObjState = 0;
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
