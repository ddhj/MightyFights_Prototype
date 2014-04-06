// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Renderers;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class CampMenuManager
	{
		List<IMenuObj>		_cMenuObjects = new List<IMenuObj>();
		Camp				_cCamp;

		public bool bNoMenus			{ get { return _cMenuObjects.Count == 0; }}

		public CampMenuManager(Camp cCamp)
		{
			_cCamp = cCamp;
		}

		public void Process(GameTime cTime)
		{
			if(_cMenuObjects.Count > 0)
				_cMenuObjects[_cMenuObjects.Count - 1].Process(cTime);
		}

		public void Draw(SpriteBatch cBatch)
		{
			if(_cMenuObjects.Count > 0)
				_cMenuObjects[_cMenuObjects.Count - 1].Draw(cBatch);
		}

		public void AddMenuObject(IMenuObj cMenuObj)
		{
			// check to see if we need to process camp events
			if(_cMenuObjects.Count == 0)
				_cCamp.UnRegisterHandlersMenu();

			// set the menu object draw layer
			cMenuObj.fZRange = .6f;

			_cMenuObjects.Add(cMenuObj);
		
			// register the handlers for the new menu object
			cMenuObj.RegeisterEvents();
			
			// if we have a pre existing menu we want to take the events off 
			if(_cMenuObjects.Count > 1) 
				_cMenuObjects[_cMenuObjects.Count - 2].UnRegisterEvents();
		}

		public void RemoveMenuObject(IMenuObj cMenuObj)
		{
			// this might just be that simple
			_cMenuObjects.RemoveAt(_cMenuObjects.Count - 1);
			cMenuObj.UnRegisterEvents();

			// check to see if we need to process camp events
			if(_cMenuObjects.Count == 0)
				_cCamp.RegisterHandlersMenu();
			// we still have menu objects re-register the events for that menu object
			else _cMenuObjects[_cMenuObjects.Count - 1].RegeisterEvents();
		}
	}
}