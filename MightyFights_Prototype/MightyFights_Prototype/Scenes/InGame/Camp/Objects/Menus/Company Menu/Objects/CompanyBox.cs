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
		
		public Company cCompany		{ get { return _cCompany; }}

		public CompanyBox(Company cCompany) : base()
		{
			_cCompany = cCompany;
			_cFont = DataStore.cInstance.cContent.Load<SpriteFont>(@"Shared\TestFon");
			_cTexRefAlt = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanySelect");
		}

		public CompanyBox() : base()
		{
			_cFont = DataStore.cInstance.cContent.Load<SpriteFont>(@"Shared\TestFon");
			_cTexRefAlt = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\CompanySelect");
		}

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

		}

		public void Toggle()
		{
			Texture2D cTmp = this.cTexRef;
			this.cTexRef = _cTexRefAlt;
			_cTexRefAlt = cTmp;
		}
	}
}
