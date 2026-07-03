using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class ClickableKnight : ClickableSprite, IActiveBasic, IMouseInteractive
	{
		CampMenuManager		_cMgr;

		public ClickableKnight(CampMenuManager cMgr)
		{
			_cMgr = cMgr;
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
		}

		#endregion

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			if(ContainsPoint(eMouseEvt.Location)) { 
				KnightMenu		cKnight = new KnightMenu(_cMgr);

				cKnight.InitMenu();
				cKnight.cTexRef = DataStore.cInstance.cContent.Load<Texture2D>(@"In Game\Camp\KnightMenu\knight_box");
				cKnight.tPos = new Vector2(83, 223);
				cKnight.sTexName = @"In Game\Camp\KnightMenu\knight_box";
				cKnight.cFrame = new Frame(cKnight.cTexRef.Bounds, 
					new Vector2(cKnight.cTexRef.Bounds.Width / 2, cKnight.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), 
					new Vector2(cKnight.cTexRef.Bounds.Width, cKnight.cTexRef.Bounds.Height), 
					new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cMgr.AddMenuObject(cKnight);
			}
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
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
	}

	public class ClickableSmith : ClickableSprite, IActiveBasic, IMouseInteractive
	{
		CampMenuManager		_cMgr;

		public ClickableSmith(CampMenuManager cMgr)
		{
			_cMgr = cMgr;
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
		}

		#endregion

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			if(ContainsPoint(eMouseEvt.Location)) { 

			}
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
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
	}

	public class ClickablePikard : ClickableSprite, IActiveBasic, IMouseInteractive
	{
		CampMenuManager		_cMgr;

		public ClickablePikard(CampMenuManager cMgr)
		{
			_cMgr = cMgr;
		}

		#region IActiveBasic Members

		public void Process(GameTime cTime)
		{
		}

		#endregion

		#region IMouseInteractive Members

		public void MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
		}

		public void MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			if(ContainsPoint(eMouseEvt.Location)) { 
				IGameScene nScene = new BattleGround_Basic();
				DataStore.cInstance.cLSteward.cTemplates[0].iCount = 100;
				//DataStore.cInstance.cLSteward.cTemplates[1].iCount = 45;
			//	DataStore.cInstance.cLSteward.cTemplates[2].iCount = 45;
				DataStore.cInstance.cRSteward.cTemplates[0].iCount = 100;
				//DataStore.cInstance.cRSteward.cTemplates[1].iCount = 45;
				//DataStore.cInstance.cRSteward.cTemplates[2].iCount = 45;
			
				if(nScene.Init()) 
					DataStore.cInstance.cSceneMgr.AddScene(nScene);
			}
		}

		public void MouseUp(object oSender, MouseEventArgs eMouseEvt)
		{
			throw new NotImplementedException();
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
	}
}