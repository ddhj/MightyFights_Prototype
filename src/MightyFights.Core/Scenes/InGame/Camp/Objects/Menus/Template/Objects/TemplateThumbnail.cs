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

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class TemplateThumbnail : ClickableSprite
	{
		CampMenuManager _cMgr;
		TemplateConfig	_cConfig;
		FrontGuys		_cGuy;
		SpriteFont		_cFont;

		public TemplateThumbnail(TemplateConfig cConfig, CampMenuManager cMgr)
		{
			_cConfig = cConfig;
			_cMgr = cMgr;
		}

		public void Init()
		{
			ContentManager cContent = DataStore.cInstance.cContent;
			dlProcessClick = ProcessClick;

			_cGuy = new FrontGuys(DataManager.cInstance.LoadAnimationData(@"Sprite Data\Troopers\Halberd\Front Facing\frontarray"),
				_cConfig.sColor, new Dictionary<string, bool>());
			_cGuy.cTexRef = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\front");
			_cGuy.fZRange = .4f;
			// the scale throws this off quite a bit
			_cGuy.tPos = new Vector2(tPos.X - 30, tPos.Y - 5);

			_cFont = cContent.Load<SpriteFont>(@"Shared\smallfont");
		}

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
			_cGuy.Draw(cBatch);
			cBatch.DrawString(_cFont, _cConfig.sTemplateName, new Vector2(tPos.X + 3, tPos.Y + 55), Color.White);
		}

		void ProcessClick(object oSender, object oArgs)
		{
			Template		cTemplate = new Template(_cMgr, _cConfig);
			Viewport		cView = DataStore.cInstance.cGraphics.Viewport;

			cTemplate.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\Template\template_window");
			cTemplate.tPos = new Vector2(cView.Width / 2 - cTemplate.cTexRef.Width / 2, cView.Height / 2 - cTemplate.cTexRef.Height / 2);
			cTemplate.sTexName = @"In Game\Camp\Company Dialog\company_window";
			cTemplate.cFrame = new Frame(cTemplate.cTexRef.Bounds, 
				new Vector2(cTemplate.cTexRef.Bounds.Width / 2, cTemplate.cTexRef.Bounds.Height / 2), 
				new Vector2(0, 0), new Vector2(0, 0), 
				new Vector2(cTemplate.cTexRef.Bounds.Width, cTemplate.cTexRef.Bounds.Height), 
				new Vector2(0, 0), new Vector2(0, 0), null, false, false);

			// set the background sprite first because everything in the menu will be based on the position of the top left point
			if(cTemplate.InitMenu())
				_cMgr.AddMenuObject(cTemplate);
		}

		public void SetPos(Vector2 tNewPos)
		{
			tPos = tNewPos;
			_cGuy.tPos = new Vector2(tPos.X - 30, tPos.Y - 5);
		}
	}

	public class CompanyTemplate : ClickableSprite
	{
		TemplateConfig	_cConfig;
		FrontGuys		_cGuy;
		SpriteFont		_cFont;
		Vector2			_tPos;

		public override Vector2 tPos	{ get { return _tPos; } 
			set {
				_tPos = value; 
				if(cTexRef != null) 
					cDrawnRect = new Rectangle((int)_tPos.X, (int)_tPos.Y, cTexRef.Bounds.Width, cTexRef.Bounds.Height);
			}
		}

		public FrontGuys cGuy			{ get { return _cGuy; }}

		public CompanyTemplate(TemplateConfig cConfig) : base()
		{
			_cConfig = cConfig;
		}

		public void Init()
		{
			ContentManager cContent = DataStore.cInstance.cContent;

			_cGuy = new FrontGuys(DataManager.cInstance.LoadAnimationData(@"Sprite Data\Troopers\Halberd\Front Facing\frontarray"),
				_cConfig.sColor, new Dictionary<string, bool>());
			_cGuy.cTexRef = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\front");
			_cGuy.fZRange = .4f;
			// the scale throws this off quite a bit
			_cGuy.tPos = new Vector2(tPos.X - 30, tPos.Y - 5);
			cDrawnRect = new Rectangle((int)_tPos.X, (int)_tPos.Y, 43, 70);
			_cFont = cContent.Load<SpriteFont>(@"Shared\smallfont");
		}

		public override void Draw(SpriteBatch cBatch)
		{
			if(cFrame != null)	base.Draw(cBatch);

			if(_cConfig != null) { 
				_cGuy.Draw(cBatch);
				cBatch.DrawString(_cFont, _cConfig.sTemplateName, new Vector2(tPos.X + 3, tPos.Y + 55), Color.White, 0f, new Vector2(0,0), 1.0f, SpriteEffects.None, .4f);
				cBatch.DrawString(_cFont, _cConfig.iCount.ToString(), new Vector2(tPos.X + 6, tPos.Y + 64), Color.White, 0f, new Vector2(0,0), 1.0f, SpriteEffects.None, .4f);
			}
		}

		void ProcessClick(object oSender, object oArgs){}

		public void SetPos(Vector2 tNewPos)
		{
			tPos = tNewPos;
			_cGuy.tPos = new Vector2(tPos.X - 30, tPos.Y - 5);
		}
	}

	public class CompanySelectedTemplate : ClickableSprite
	{
		SelectedTemplate	_cTemplate = new SelectedTemplate(0, 0, 0);

		void ProcessClick(object oSender, object oArgs)
		{

		}

		public void SetTemplate(TemplateConfig cConfig)
		{
			_cTemplate.iTemplateId = cConfig.iId;
			_cTemplate.iCurrentCount = 0;
		}
	}
}
