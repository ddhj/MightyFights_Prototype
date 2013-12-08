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
	public class CampObjectManager
	{
		Dictionary<string, Dictionary<int, IDrawable>>		_cDrawRefList = new Dictionary<string,Dictionary<int,IDrawable>>();
		TerminatingParticleEffectManager	_cParticleSystemMgr;
		Dictionary<int, IActiveBasic>		_cActiveList = new Dictionary<int,IActiveBasic>();
		Dictionary<int, object>				_cMainObjectList = new Dictionary<int,object>();
		Dictionary<int, IClickable>			_cClickable = new Dictionary<int,IClickable>();

		bool		_bProcessClick = true;

		public BattlegroundData cParentData		{ get; set; }

		public void AddObject(object oData)
		{
			IObject nObj = (IObject)oData;
			Dictionary<int, IDrawable> caDrawList = null;

			// add the object to the mater list 
			_cMainObjectList.Add(nObj.iId, oData);

			// check if the object being added has a draw component
			if(oData is IDrawable) { 
				// check to see if the drawable component is a texture
				if(oData is IDrawableTexture) { 
					if(!_cDrawRefList.TryGetValue(((IDrawableTexture)oData).sTexName, out caDrawList))
						_cDrawRefList.Add(((IDrawableTexture)oData).sTexName, caDrawList = new Dictionary<int, IDrawable>());
				// the object is a drawable font
				} else if(oData is IDrawableFont) { 
					if(!_cDrawRefList.TryGetValue(((IDrawableFont)oData).sFontName, out caDrawList))
						_cDrawRefList.Add(((IDrawableFont)oData).sFontName, caDrawList = new Dictionary<int, IDrawable>());
				}

				// check to see if the draw has data 
				if(caDrawList != null) 
					caDrawList.Add(nObj.iId, (IDrawable)oData);
			}

			// check to see if the object has an active component 
			if(oData is IActiveBasic) 
				_cActiveList.Add(nObj.iId, (IActiveBasic)oData);
		}

		public void AddClickObject(object oData, DProcessClick dlProcess)
		{
			// add the object to the process and draw lists if possible
			AddObject(oData);

			// this object should be a clickable object, otherwize why the hell is AddClickObject getting called
			if(oData is IClickable) { 
				((IClickable)oData).dlProcessClick = dlProcess;
				_cClickable.Add(((IObject)oData).iId, (IClickable)oData);
			}
		}

		public void CreateParticleManager( )
		{
			Renderer	cRenderer = new SpriteBatchRenderer( );

			cRenderer.GraphicsDeviceService = DataStore.cInstance.cGfxMgr;
			cRenderer.LoadContent( DataStore.cInstance.cContent );
			_cParticleSystemMgr = new TerminatingParticleEffectManager( cRenderer );
		}

		public void AddParticleEffect( ParticleEffect cEffect )
		{
			_cParticleSystemMgr.Add( cEffect );
		}

		public void Process(GameTime cTime, bool bProcessClickObj)
		{
			List<int>				iaRemList = new List<int>();
			List<IActiveBasic>		naActive = new List<IActiveBasic>();
			IObject					nObj;
			MouseState				cMouseState = Mouse.GetState();
			Point					tPoint = new Point(cMouseState.X, cMouseState.Y);
			List<IClickable>		naClickObjList = new List<IClickable>();

			// preprocess lists
			foreach(object oObj in _cMainObjectList.Values) { 
				nObj = (IObject)oObj;
				// take care of the objects that no longer need to draw
				if(oObj is IDrawable)
					if((nObj.eObjState & EObjectStates.Draw) != EObjectStates.Draw) { 
						if(oObj is IDrawableFont)
							_cDrawRefList[((IDrawableFont)oObj).sFontName].Remove(nObj.iId);
						else _cDrawRefList[((IDrawableTexture)oObj).sTexName].Remove(nObj.iId);

						// remove the clickable since you can't see it 
						_cClickable.Remove(nObj.iId);
					}

				// remove any objects no longer active
				if(oObj is IActiveBasic) { 
					if((nObj.eObjState & EObjectStates.Active) == EObjectStates.Active)
						naActive.Add((IActiveBasic)oObj);
					else _cActiveList.Remove(nObj.iId);
				}

				// if the object has no state left then blow it away
				if(nObj.eObjState == 0)
					iaRemList.Add(nObj.iId);
			}
		
			if(bProcessClickObj) { 
				// check the mouse button click 
				foreach(KeyValuePair<int, IClickable> tClickObj in _cClickable)
					naClickObjList.Add(tClickObj.Value);

				if(cMouseState.LeftButton == ButtonState.Pressed) { 
					if(_bProcessClick == true) { 
						foreach(IClickable nClickObj in naClickObjList)
							if(nClickObj.ContainsPoint(tPoint))
								if(nClickObj.dlProcessClick != null)
									nClickObj.dlProcessClick(this.cParentData, nClickObj);
						
						_bProcessClick = false;
					}
				} else _bProcessClick = true;
			}

			// remove all objects from the main processing list
			foreach(int iObj in iaRemList)
				_cMainObjectList.Remove(iObj);

			// process all the active objects
			foreach(IActiveBasic nActiveObj in naActive)
				nActiveObj.Process(cTime);

			_cParticleSystemMgr.Update( cTime );
		}

		public void Draw(SpriteBatch cBatch)
		{
			// draw each object by their texture
			foreach(Dictionary<int, IDrawable> cDrawList in _cDrawRefList.Values)
				foreach(IDrawable nObj in cDrawList.Values)
					nObj.Draw(cBatch);
		}

		public void DrawParticles( SpriteBatch cBatch )
		{
			// draw each object by their texture
			if( _cParticleSystemMgr.Count > 0 )
				_cParticleSystemMgr.Draw( );
		}

		public void Clear()
		{
			_cActiveList.Clear();
			foreach(Dictionary<int, IDrawable> cDrawList in _cDrawRefList.Values)
				cDrawList.Clear();
			_cDrawRefList.Clear();
			_cMainObjectList.Clear();
			_cParticleSystemMgr.Clear( );

			// clear the effect managers
			foreach( ParticleEffect cEffect in _cParticleSystemMgr )
				cEffect.Terminate( );
			_cParticleSystemMgr.Clear( );
		}
	}
}
