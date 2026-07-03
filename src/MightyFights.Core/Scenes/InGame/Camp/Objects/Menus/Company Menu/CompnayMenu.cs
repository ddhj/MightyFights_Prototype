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
	public class CompanyMenu : ClickableSprite, IMenuObj, IMouseInteractive
	{
		public object			oMenuObject	{ get { return this; }}
		public object			oResultData	{ get { return null; }}

		CampMenuManager			_cMgr;

		List<MenuControlBase>	_caControls = new List<MenuControlBase>();
		List<MenuButton>		_caButtons = new List<MenuButton>();
		List<CompanyBox>		_caCompanyBoxes = new List<CompanyBox>();
		List<BasicSprite>		_caBasicSprites = new List<BasicSprite>();
		List<CompanyTemplate>	_caTemplates = new List<CompanyTemplate>();
		List<CompanyTemplate>	_caSelTemplates = new List<CompanyTemplate>();
		SpriteFont				_cFont;
		CompanyName				_cName;
		CompanyBox				_cSelectedCompany;
		CompanyTemplate			_cSelectedTemplate;
		bool					_bNew;
		FrontGuys				_cGuy;

		int						_iRows = 0,
								_iRow = 0;

		IconGrid				_cIconGrid;

		enum EMouseState { 
			DragTemplate,
			IconGrid,
			Normal
		};

		EMouseState				_eMouseState = EMouseState.Normal;

		public CompanyMenu(CampMenuManager cMgr) : base()
		{
			_cMgr = cMgr;
		}
		
		public bool InitMenu()
		{
			try { 
				DataStore		cData = DataStore.cInstance;
				ContentManager	cContent = cData.cContent;
				int				iCompanyPos = (int)tPos.Y + 40;
				
				CompanyName cTmpSpr = new CompanyName(_cMgr);
				_cName = cTmpSpr;
				cTmpSpr.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\company_name");
				cTmpSpr.tPos = new Vector2(tPos.X + 66, tPos.Y + 7);
				cTmpSpr.sTexName = @"In Game\Camp\Company Dialog\company_name";
				cTmpSpr.fZRange = .5f;
				cTmpSpr.cFrame = new Frame(cTmpSpr.cTexRef.Bounds, 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width / 2, cTmpSpr.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSpr.cTexRef.Bounds.Width, cTmpSpr.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caControls.Add(cTmpSpr);

				BasicSprite cTmpSprite = new BasicSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\size_box");
				cTmpSprite.tPos = new Vector2(tPos.X + 234, tPos.Y + 17);
				cTmpSprite.sTexName = @"In Game\Camp\Company Dialog\size_box";
				cTmpSprite.fZRange = .5f;
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caBasicSprites.Add(cTmpSprite);

				cTmpSprite = new BasicSprite();
				cTmpSprite.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\click_to_drag");
				cTmpSprite.tPos = new Vector2(tPos.X + 113, tPos.Y + 118);
				cTmpSprite.sTexName = @"In Game\Camp\Company Dialog\click_to_drag";
				cTmpSprite.fZRange = .5f;
				cTmpSprite.cFrame = new Frame(cTmpSprite.cTexRef.Bounds, 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width / 2, cTmpSprite.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpSprite.cTexRef.Bounds.Width, cTmpSprite.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caBasicSprites.Add(cTmpSprite);

				MenuButton	cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"Shared\company_arrowupx");
				cTmpBtn.tPos = new Vector2(tPos.X + 28, tPos.Y + 10);
				cTmpBtn.sTexName = @"Shared\company_arrowupx";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\reset_gray");
				cTmpBtn.tPos = new Vector2(tPos.X + 278, tPos.Y + 21);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\reset_gray";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\template_arrow_left");
				cTmpBtn.tPos = new Vector2(tPos.X + 76, tPos.Y + 77);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\template_arrow_left";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\template_arrow_right");
				cTmpBtn.tPos = new Vector2(tPos.X + 316, tPos.Y + 77);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\template_arrow_right";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\grid_arrow_left");
				cTmpBtn.tPos = new Vector2(tPos.X + 76, tPos.Y + 205);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\grid_arrow_left";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\grid_arrow_right");
				cTmpBtn.tPos = new Vector2(tPos.X + 316, tPos.Y + 205);
				cTmpBtn.sTexName = @"In Game\Camp\Company Dialog\grid_arrow_right";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

// company boxes along the left side				
				CompanyBox cTmpCompany;
				string sCompanyBoxTex = @"In Game\Camp\Company Dialog\CompanySelect",
					   sCompanyBoxTexAlt = @"In Game\Camp\Company Dialog\CompanyBox";
				foreach(KeyValuePair<string, Company> tCompany in cData.cLSteward.hCompaniesByIconName) { 
					cTmpCompany = new CompanyBox(tCompany.Value);
					cTmpCompany.cTexRef = cContent.Load<Texture2D>(sCompanyBoxTex);
					cTmpCompany.cTexRefAlt = cContent.Load<Texture2D>(sCompanyBoxTexAlt);
					cTmpCompany.tPos = new Vector2(tPos.X + 10, iCompanyPos);
					cTmpCompany.sTexName = sCompanyBoxTex;
					cTmpCompany.fZRange = .5f;
					cTmpCompany.cFrame = new Frame(cTmpCompany.cTexRef.Bounds, 
						new Vector2(cTmpCompany.cTexRef.Bounds.Width / 2, cTmpCompany.cTexRef.Bounds.Height / 2), 
						new Vector2(0, 0), new Vector2(0, 0), 
						new Vector2(cTmpCompany.cTexRef.Bounds.Width, cTmpCompany.cTexRef.Bounds.Height), 
						new Vector2(0, 0), new Vector2(0, 0), null, false, false);

					cTmpCompany.cIcon = new BasicSprite();
					cTmpCompany.cIcon.cTexRef = cContent.Load<Texture2D>(tCompany.Key);
					cTmpCompany.cIcon.tPos = new Vector2(cTmpCompany.tPos.X + 5, cTmpCompany.tPos.Y + 5);
					cTmpCompany.cIcon.sTexName = tCompany.Key;
					cTmpCompany.cIcon.fZRange = .5f;
					cTmpCompany.cIcon.cFrame = new Frame(cTmpCompany.cIcon.cTexRef.Bounds, 
						new Vector2(cTmpCompany.cIcon.cTexRef.Bounds.Width / 2, cTmpCompany.cIcon.cTexRef.Bounds.Height / 2), 
						new Vector2(0, 0), new Vector2(0, 0), 
						new Vector2(cTmpCompany.cIcon.cTexRef.Bounds.Width, cTmpCompany.cIcon.cTexRef.Bounds.Height), 
						new Vector2(0, 0), new Vector2(0, 0), null, false, false);

					_caCompanyBoxes.Add(cTmpCompany);
					sCompanyBoxTex = @"In Game\Camp\Company Dialog\CompanyBox";
					sCompanyBoxTexAlt = @"In Game\Camp\Company Dialog\CompanySelect";
					iCompanyPos += 30;
				}
				
// bottom arrow for the company select
				cTmpBtn = new MenuButton();
				cTmpBtn.cTexRef = cContent.Load<Texture2D>(@"Shared\company_arrowdownx");
				cTmpBtn.tPos = new Vector2(tPos.X + 28, tPos.Y + 249);
				cTmpBtn.sTexName = @"Shared\company_arrowdownx";
				cTmpBtn.fZRange = .5f;
				cTmpBtn.cFrame = new Frame(cTmpBtn.cTexRef.Bounds, 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width / 2, cTmpBtn.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmpBtn.cTexRef.Bounds.Width, cTmpBtn.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caButtons.Add(cTmpBtn);

// draw the templates 
				int iX = 95,
					iY = 125;
				CompanyTemplate	cTmp;
				List<TemplateCfgMaster> cTemplates = DataStore.cInstance.cLSteward.cTemplates;
				for(int iTemplate = 0; iTemplate < cTemplates.Count;) { 
					cTmp = new CompanyTemplate((TemplateConfig)cTemplates[iTemplate++]);
					cTmp.tPos = new Vector2(tPos.X + iX, tPos.Y + iY);
					cTmp.fZRange = .5f;
					cTmp.Init();
					_caTemplates.Add(cTmp);

					if(iTemplate < cTemplates.Count) { 
						iY += 76;
						cTmp = new CompanyTemplate((TemplateConfig)cTemplates[iTemplate++]);
						cTmp.tPos = new Vector2(tPos.X + iX, tPos.Y + iY);
						cTmp.fZRange = .5f;
						cTmp.Init();
						_caTemplates.Add(cTmp);
					}

					iY = 125;
					iX += 46;
				}

// draw the select templates 
				cTmp = new CompanyTemplate(null);
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\TemplateSelect");
				cTmp.tPos = new Vector2(tPos.X + 100, tPos.Y + 45);
				cTmp.sTexName = @"In Game\Camp\Template\TemplateSelect";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caSelTemplates.Add(cTmp);

				cTmp = new CompanyTemplate(null);
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\TemplateSelect");
				cTmp.tPos = new Vector2(tPos.X + 155, tPos.Y + 45);
				cTmp.sTexName = @"In Game\Camp\Template\TemplateSelect";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caSelTemplates.Add(cTmp);

				cTmp = new CompanyTemplate(null);
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\TemplateSelect");
				cTmp.tPos = new Vector2(tPos.X + 210, tPos.Y + 45);
				cTmp.sTexName = @"In Game\Camp\Template\TemplateSelect";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caSelTemplates.Add(cTmp);

				cTmp = new CompanyTemplate(null);
				cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\TemplateSelect");
				cTmp.tPos = new Vector2(tPos.X + 265, tPos.Y + 45);
				cTmp.sTexName = @"In Game\Camp\Template\TemplateSelect";
				cTmp.fZRange = .5f;
				cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
					new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_caSelTemplates.Add(cTmp);

//// front guys for the drag draw on the template
				_cGuy = new FrontGuys(DataManager.cInstance.LoadAnimationData(@"Sprite Data\Troopers\Halberd\Front Facing\frontarray"),
					@"Sprite Data\Troopers\Halberd\Textures\fazure", new Dictionary<string, bool>());
				_cGuy.cTexRef = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\front");
				_cGuy.fZRange = .3f;
				_cGuy.tColor = new Color(_cGuy.tColor.R, _cGuy.tColor.G, _cGuy.tColor.B, 50);
				// the scale throws this off quite a bit
				_cGuy.tPos = new Vector2(tPos.X - 30, tPos.Y - 5);

//// icon grid
				_cIconGrid = new IconGrid();
				_cIconGrid.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Company Dialog\icon_window");
				_cIconGrid.tPos = new Vector2(_cName.tPos.X + 44, _cName.tPos.Y);
				_cIconGrid.sTexName = @"In Game\Camp\Company Dialog\icon_window";
				_cIconGrid.fZRange = .4f;
				_cIconGrid.cFrame = new Frame(_cIconGrid.cTexRef.Bounds, 
					new Vector2(_cIconGrid.cTexRef.Bounds.Width / 2, _cIconGrid.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cIconGrid.cTexRef.Bounds.Width, _cIconGrid.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cIconGrid.bDraw = false;
				_cIconGrid.Init();

//// auto select the first company
				_cSelectedCompany = _caCompanyBoxes[0];
				//LoadSelectedCompany();				
			} catch(Exception xEx) { 
				return false;
			}
			
			return true;
		}

		void LoadSelectedCompany()
		{
			_cName.sName = _cSelectedCompany.cCompany.sName;
			_cName.SetIcon(_cSelectedCompany.cIcon);

			for(int iTemplate = 0; iTemplate < _cSelectedCompany.cCompany.caTemplates.Count; ++iTemplate) { 

			}
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{	
		}

		#endregion

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

			// draw the managable controls (i think its basically just the name dialog)
			foreach(ClickableSprite cButton in _caControls) cButton.Draw(cBatch);
			
			// draw all the other buttons
			foreach(ClickableSprite cButton in _caButtons) cButton.Draw(cBatch);

			// draw all the non interactive sprites
			foreach(BasicSprite cSprite in _caBasicSprites) cSprite.Draw(cBatch);

			// draw the string in the name dialog
			//cBatch.DrawString(_cFont, _cName.sName, new Vector2(tPos.X + 110, tPos.Y + 18), Color.White, 0f, new Vector2(0,0), 1.0f, SpriteEffects.None, .4f);
			_cName.Draw(cBatch);

			// draw the available templates
			foreach(CompanyTemplate cTemplate in _caTemplates)
				if(cTemplate.tPos.X > 200 && cTemplate.tPos.X < tPos.X + 316)
					cTemplate.Draw(cBatch);

			// draw the teplates in the company
			foreach(CompanyTemplate cTemplate in _caSelTemplates)
				if(cTemplate.tPos.X > 200 && cTemplate.tPos.X < tPos.X + 316)
					cTemplate.Draw(cBatch);

			// draw the company boxes 
			foreach(CompanyBox cCompany in _caCompanyBoxes)
				if(cCompany.tPos.Y > tPos.Y + 10 && cCompany.tPos.Y < tPos.Y + 249)
					cCompany.Draw(cBatch);

			switch(_eMouseState) { 
				case EMouseState.DragTemplate: 	_cGuy.Draw(cBatch); break;
				case EMouseState.IconGrid:	_cIconGrid.Draw(cBatch); break;
			}
		}

		void ProcessClick()
		{
			// check to see if we are off the menu

			// loop through the buttons 
		}

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			if(_eMouseState == EMouseState.DragTemplate) 
				_cGuy.tPos = new Vector2(eMouseEvt.Location.X - 50, eMouseEvt.Location.Y - 30);
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			switch(_eMouseState) { 
				case EMouseState.Normal: { 
					foreach(CompanyTemplate cTemplate in _caTemplates) 
						if(cTemplate.ContainsPoint(eMouseEvt.Location)) { 
							_cSelectedTemplate = cTemplate;
							_cGuy.SetByColor(cTemplate.cGuy.sCurColor);
							_eMouseState = EMouseState.DragTemplate;
							_cGuy.tPos = new Vector2(eMouseEvt.Location.X - 50, eMouseEvt.Location.Y - 30);
						}
				} break;
			}
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			switch(_eMouseState) { 
				case EMouseState.Normal: { 
					if(!ContainsPoint(eMouseEvt.Location)) { 
						_cMgr.RemoveMenuObject(this);
						return;
					}

					// check to see if we have clicked on the name and we are in add mode
					if(_bNew) { 
					}

					if(_cName.ContainsPoint(eMouseEvt.Location)) { 
						_eMouseState = EMouseState.IconGrid;
						_cIconGrid.bDraw = true;
					}
				} break;

				case EMouseState.DragTemplate: { 
					_eMouseState = EMouseState.Normal;

					// walk the list of visiable selected template boxes
					//foreach(
				} break;

				case EMouseState.IconGrid: { 
					if(_cIconGrid.ContainsPoint(eMouseEvt.Location)) { 
						_cIconGrid.dlProcessClick(this, eMouseEvt);
						if(_cIconGrid.cClickedIcon != null) { 
							_cName.SetIcon(_cIconGrid.cClickedIcon);
							DataStore.cInstance.cLSteward.hCompaniesByIconName.Add(_cIconGrid.cClickedIcon.sTexName, _cSelectedCompany.cCompany);
						}
						_eMouseState = EMouseState.Normal;
					}
				} break;
			}
		}

		public void MouseHover(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseWheel(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IMenuObj Members


		public void RegeisterEvents()
		{
			/* register all the buttons in the company menu 
			foreach(IMouseInteractive nButton in _caControls) { 
				InputSystem.MouseUp += new MouseEventHandler(nButton.MouseUp);
				InputSystem.MouseMove += new MouseEventHandler(nButton.MouseMove);
			}*/

			// the menu gets a click for close
			InputSystem.MouseUp += new MouseEventHandler(MouseUp);
			InputSystem.MouseDown += new MouseEventHandler(MouseDown);
			InputSystem.MouseMove += new MouseEventHandler(MouseMove);
			InputSystem.KeyDown += new KeyEventHandler(_cName.KeyDown);
			InputSystem.KeyUp += new KeyEventHandler(_cName.KeyUp);
		}

		public void UnRegisterEvents()
		{
			/* unregister the buttons in the company menu
			foreach(IMouseInteractive nButton in _caControls) { 
				InputSystem.MouseUp -= new MouseEventHandler(nButton.MouseUp);
				InputSystem.MouseMove -= new MouseEventHandler(nButton.MouseMove);
			}*/

			// the menu gets a click for close
			InputSystem.MouseUp -= MouseUp;
			InputSystem.MouseMove -= MouseMove;
			InputSystem.KeyDown -= _cName.KeyDown;
			InputSystem.KeyUp -= _cName.KeyUp;
		}

		#endregion
	}
}