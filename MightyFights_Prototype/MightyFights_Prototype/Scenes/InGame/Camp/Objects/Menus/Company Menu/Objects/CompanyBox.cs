using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Windows.Forms;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class CompanyBox: ClickableSprite
	{
		Company		_cCompany;
		SpriteFont	_cFont;
		
		Texture2D	_cTexRefAlt;
		BasicSprite	_cIcon;
		
		public Company cCompany		{ get { return _cCompany; }}
		public Texture2D cTexRefAlt		{ get { return _cTexRefAlt; } set { _cTexRefAlt = value; }}
		public BasicSprite cIcon		{ get { return _cIcon; } set { _cIcon = value; }}
		
		public CompanyBox(Company cCompany) : base()
		{
			_cCompany = cCompany;
			_cFont = DataStore.cInstance.cContent.Load<SpriteFont>(@"Shared\smallfont");
			_cTexRefAlt = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanySelect");
		}

		public CompanyBox() : base()
		{
			_cFont = DataStore.cInstance.cContent.Load<SpriteFont>(@"Shared\smallfont");
			_cTexRefAlt = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanySelect");
		}

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
			if(_cIcon != null) _cIcon.Draw(cBatch);
			cBatch.DrawString(_cFont, _cCompany.sName, new Vector2(_cIcon.tPos.X + _cIcon.cTexRef.Width + 3, tPos.Y + 5), Color.White);
		}

		public void Toggle()
		{
			Texture2D cTmp = this.cTexRef;
			this.cTexRef = _cTexRefAlt;
			_cTexRefAlt = cTmp;
		}
	}
}
