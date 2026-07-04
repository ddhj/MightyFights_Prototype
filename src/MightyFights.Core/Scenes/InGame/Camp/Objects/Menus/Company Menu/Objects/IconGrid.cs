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
	public class IconGrid : ClickableSprite
	{
		List<ClickableSprite>		_cIcons = new List<ClickableSprite>();
		Dictionary<string, int>		_hIconResources = new Dictionary<string,int>();

		public bool bDraw		{ get; set; }
		public ClickableSprite cClickedIcon	{ get; set; }

		public IconGrid() 
		{
			_hIconResources.Add(@"In Game\Camp\Company Dialog\axe", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\battle_axe", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\blue_jester", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\falchion", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\goblin", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\green_genie", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\horned_skull", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\kite_shield", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\knight_sword", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\purple_demon", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\red_demon", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\red_skull", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\sabre", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\sai", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\sallet", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\skull", 1);
			_hIconResources.Add(@"In Game\Camp\Company Dialog\target", 1);

			dlProcessClick = ProcessClick;
		}

		public bool Init()
		{
			try { 
				DataStore cData = DataStore.cInstance;
				ContentManager cContent = cData.cContent;
				int iRow = 0, iCol = 0;
				ClickableSprite cTmp;
				foreach(string sIconResource in _hIconResources.Keys) { 
					if(!cData.cLSteward.hCompaniesByIconName.ContainsKey(sIconResource)) { 
						cTmp = new ClickableSprite();
						cTmp.cTexRef = cContent.Load<Texture2D>(sIconResource);
						cTmp.tPos = new Vector2(tPos.X + iCol * 20, tPos.Y + iRow * 19);
						cTmp.sTexName = sIconResource;
						cTmp.fZRange = .3f;
						cTmp.cFrame = new Frame(cTmp.cTexRef.Bounds, 
							new Vector2(cTmp.cTexRef.Bounds.Width / 2, cTmp.cTexRef.Bounds.Height / 2), 
							new Vector2(0, 0), new Vector2(0, 0), 
							new Vector2(cTmp.cTexRef.Bounds.Width, cTmp.cTexRef.Bounds.Height), 
							new Vector2(0, 0), new Vector2(0, 0), null, false, false);
						_cIcons.Add(cTmp);

					}
					++iCol;

					if(iCol > 5) { ++iRow; iCol = 0; };
				}

				return true;
			} catch(Exception) {
				return false;
			}
		}

		public override void Draw(SpriteBatch cBatch)
		{
			base.Draw(cBatch);
			foreach(ClickableSprite cIcon in _cIcons)
				cIcon.Draw(cBatch);
		}

		void ProcessClick(object oSender, object oArgs)
		{
			MouseEventArgs vEventArgs = (MouseEventArgs)oArgs;
			this.cClickedIcon = null;
			foreach(ClickableSprite cIcon in _cIcons)
				if(cIcon.ContainsPoint(vEventArgs.Location)) { 
					this.cClickedIcon = cIcon;
					this.bDraw = false;
					return;
				}
		}
	}
}
