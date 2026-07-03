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
	public class TemplateSelect : ClickableSprite, IMenuObj, IMouseInteractive
	{
		CampMenuManager _cMgr;

		ClickableSprite	_cDownArrow,
						_cUpArrow;

		int				_iRow = 0,
						_iRows;

		List<TemplateThumbnail>	_caTemplates = new List<TemplateThumbnail>();

		public TemplateSelect(CampMenuManager cMgr)
		{
			_cMgr = cMgr;
		}

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);

			if(_iRow != _iRows)	_cDownArrow.Draw(cBatch);
			if(_iRow != 0) _cUpArrow.Draw(cBatch);

			foreach(TemplateThumbnail cTemplate in _caTemplates)
				if(cTemplate.tPos.Y > tPos.Y && cTemplate.tPos.Y < _cDownArrow.tPos.Y)
					cTemplate.Draw(cBatch);
		}

		#region IMenuObj Members

		public object oMenuObject {	get { throw new NotImplementedException(); }}
		public object oResultData { get { throw new NotImplementedException(); }}

		public bool InitMenu()
		{
			try { 
				List<TemplateCfgMaster> cTemplateList = DataStore.cInstance.cLSteward.cTemplates;
				ContentManager cContent = DataStore.cInstance.cContent;
				int iCount = 0,
					iY = 15,
 					iX = 15;
				TemplateThumbnail cTmp;

				foreach(TemplateCfgMaster cTmpCfg in cTemplateList) { 
					if(iCount > 3) { iY += 73; ++_iRows; iCount = 0; iX = 15; }

					cTmp = new TemplateThumbnail((TemplateConfig)cTmpCfg, _cMgr);
					cTmp.cTexRef = cContent.Load<Texture2D>(@"In Game\Camp\Template\TemplateSelect");
					cTmp.tPos = new Vector2(tPos.X + iX, tPos.Y + iY);
					cTmp.sTexName = @"In Game\Camp\Template\TemplateSelect";
					cTmp.fZRange = .5f;
					cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
						new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
						new Vector2(0, 0), new Vector2(0, 0), 
						new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
						new Vector2(0, 0), new Vector2(0, 0), null, false, false);
					cTmp.Init();
					_caTemplates.Add(cTmp);
					iX += 46;
					
					++iCount;
				}

				_cDownArrow = new ClickableSprite();
				_cDownArrow.cTexRef = cContent.Load<Texture2D>(@"Shared\company_arrowdownx");
				_cDownArrow.tPos = new Vector2(tPos.X + 230, tPos.Y + 230);
				_cDownArrow.sTexName = @"Shared\company_arrowdownx";
				_cDownArrow.fZRange = .5f;
				_cDownArrow.cFrame = new Frame(_cDownArrow.cTexRef.Bounds, 
					new Vector2(_cDownArrow.cTexRef.Bounds.Width / 2, _cDownArrow.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cDownArrow.cTexRef.Bounds.Width, _cDownArrow.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);

				_cUpArrow = new ClickableSprite();
				_cUpArrow.cTexRef = cContent.Load<Texture2D>(@"Shared\company_arrowupx");
				_cUpArrow.tPos = new Vector2(tPos.X + 230, tPos.Y + 25);
				_cUpArrow.sTexName = @"Shared\company_arrowupx";
				_cUpArrow.fZRange = .5f;
				_cUpArrow.cFrame = new Frame(_cUpArrow.cTexRef.Bounds, 
					new Vector2(_cUpArrow.cTexRef.Bounds.Width / 2, _cUpArrow.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(_cUpArrow.cTexRef.Bounds.Width, _cUpArrow.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);


				return true;
			} catch(Exception xEx) { 
				return false;
			}
		}

		public void RegeisterEvents()
		{
			InputSystem.MouseUp += new Microsoft.Xna.Framework.Input.MouseEventHandler(MouseUp);
		}

		public void UnRegisterEvents()
		{
			InputSystem.MouseUp -= MouseUp;
		}
		
		#endregion

		#region IActiveBasic Members

		public void Process(GameTime cTime)	{}

		#endregion

		#region IMouseInteractive Members

		public void MouseMove(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseUp(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			if(_iRow != _iRows)
				if(_cDownArrow.ContainsPoint(eMouseEvt.Location)) { 
					++_iRow;
					foreach(TemplateThumbnail cTemplate in _caTemplates)
						cTemplate.SetPos(new Vector2(cTemplate.tPos.X, cTemplate.tPos.Y - 73));
				}

			if(_iRow != 0) 
				if(_cUpArrow.ContainsPoint(eMouseEvt.Location)) { 
					--_iRow;
					foreach(TemplateThumbnail cTemplate in _caTemplates)
						cTemplate.SetPos(new Vector2(cTemplate.tPos.X, cTemplate.tPos.Y + 73));
				}

			foreach(TemplateThumbnail cTemplate in _caTemplates)
				if(ContainsPoint(cTemplate.tPos)) 
					if(cTemplate.ContainsPoint(eMouseEvt.Location))
						cTemplate.dlProcessClick(null, null);

			if(!ContainsPoint(eMouseEvt.Location)) { 
				_cMgr.RemoveMenuObject(this);
				return;
			}
		}

		public void MouseHover(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseWheel(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDoubleClick(object oSender, Microsoft.Xna.Framework.Input.MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
