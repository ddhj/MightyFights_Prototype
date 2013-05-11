using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class ObjectManager
	{
		#region Singleton Implementation
		
		private static readonly ObjectManager _cInstance = new ObjectManager();
		public static ObjectManager cInstance	{ get { return _cInstance; }} 
		private ObjectManager() {}
		
		#endregion

		Dictionary<string, AnimationData>	_cAnimationDataList = new Dictionary<string,AnimationData>();
		Dictionary<string, Texture2D>		_cTextureList = new Dictionary<string,Texture2D>();
		ContentManager						_cContent;
		int									_iCurObjId = int.MinValue;
		string[]							_saBuffs = new string[] { "acorn", "claw", "ear", "eye", "fang", "feather", "paw", "wing" };

		public int iCurObjId	{ get { return ++_iCurObjId; }}

		public bool Init()
		{
			_cContent = DataStore.cInstance.cContent; 
			return true;
		}

		public TrooperTemplate CreateTemplate(TemplateConfig cTemplateData)
		{
			TrooperTemplate	cTemplate = new TrooperTemplate();
			AnimationData	cAnimData;
			AiBattleData	cBattleAi;
			Texture2D		cTexData;
			int				iMod;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(cTemplateData.sTrooperType, out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(cTemplateData.sTrooperType, cAnimData = _cContent.Load<AnimationData>(cTemplateData.sTrooperType));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(cTemplateData.sColor, out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(cTemplateData.sColor, cTexData = _cContent.Load<Texture2D>(cTemplateData.sColor));

			// build in the systems for the trooper
			cTemplate.cAnimProcessorRef = new AnimationProcessor(cAnimData, cTemplateData);
			cTemplate.cTextureRef = cTexData;
			cTemplate.sTexName = cTemplateData.sColor;

			////ddhj modifications to the attack speed increment based on the template data. not sure if this 
			// is where we are going to want this to go 
			foreach(KeyValuePair<string, ActionData> tAction in cAnimData.cReferenceList["Attack"]["Basic"]) { 
				iMod = -tAction.Value.iIncrement * (cTemplateData.cStats.iAtkSpeed / 100);
				cTemplate.cAnimProcessorRef.cActionIncrement.Add(tAction.Key, iMod);
			}

			// set the stats for the 
			cTemplate.cStats = new Stats(cTemplateData.cStats);

			// set the action manager, likely this will have some stuff from the template too
			cTemplate.cActionMgr = new TrooperActMgr(cTemplate.cAnimProcessorRef);

			// the battle huristics are also going to be set here or in some other area based on template config data
			cTemplate.cAiData = new AiBattleData();

			return cTemplate;
		}

		public BasicBuff CreateBuff(ICombatant nCombatant)
		{
			AnimationData		cAnimData;
			Random				cRand = DataStore.cInstance.cRand;
			Texture2D			cTexData;
			BasicBuff			cBasicBuff;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue("Buffs", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add("Buffs", cAnimData = _cContent.Load<AnimationData>(@"In Game\Buffs\BufItemsArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"In Game\Buffs\BufItems", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"In Game\Buffs\BufItems", cTexData = _cContent.Load<Texture2D>(@"In Game\Buffs\BufItems"));

			// buff data
			cBasicBuff = new BasicBuff(cAnimData, _saBuffs[cRand.Next(_saBuffs.Length)], 3);
			cBasicBuff.cTexRef = cTexData;
			cBasicBuff.tPos = nCombatant.tCenter;
			cBasicBuff.sTexName = @"In Game\Buffs\BufItems";

			return cBasicBuff;
		}
	}

	public class ObjectManagerInstance
	{
		Dictionary<string, Dictionary<int, IDrawable>>		_caDrawRefList = new Dictionary<string,Dictionary<int,IDrawable>>();
		Dictionary<int, IActiveBasic>						_caActiveList = new Dictionary<int,IActiveBasic>();
		Dictionary<int, object>								_caMainObjectList = new Dictionary<int,object>();
		Dictionary<int, IClickable>							_caClickable = new Dictionary<int,IClickable>();

		public void AddObject(object oData)
		{
			IObject nObj = (IObject)oData;
			Dictionary<int, IDrawable> caDrawList = null;

			// add the object to the mater list 
			_caMainObjectList.Add(nObj.iId, oData);

			// check if the object being added has a draw component
			if(oData is IDrawable) { 
				// check to see if the drawable component is a texture
				if(oData is IDrawableTexture) { 
					if(!_caDrawRefList.TryGetValue(((IDrawableTexture)oData).sTexName, out caDrawList))
						_caDrawRefList.Add(((IDrawableTexture)oData).sTexName, caDrawList = new Dictionary<int, IDrawable>());
				// the object is a drawable font
				} else if(oData is IDrawableFont) { 
					if(!_caDrawRefList.TryGetValue(((IDrawableFont)oData).sFontName, out caDrawList))
						_caDrawRefList.Add(((IDrawableFont)oData).sFontName, caDrawList = new Dictionary<int, IDrawable>());
				}

				// check to see if the draw has data 
				if(caDrawList != null) 
					caDrawList.Add(nObj.iId, (IDrawable)oData);
			}

			// check to see if the object has an active component 
			if(oData is IActiveBasic) 
				_caActiveList.Add(nObj.iId, (IActiveBasic)oData);

			if(oData is IClickable)
				_caClickable.Add(nObj.iId, (IClickable)oData);
		}

		public void Process(GameTime cTime)
		{
			List<int>				iaRemList = new List<int>();
			List<IActiveBasic>		naActive = new List<IActiveBasic>();
			IObject		nObj;

			// preprocess lists
			foreach(object oObj in _caMainObjectList.Values) { 
				nObj = (IObject)oObj;
				// take care of the objects that no longer need to draw
				if(oObj is IDrawable)
					if((nObj.eObjState & EObjectStates.Draw) != EObjectStates.Draw) { 
						if(oObj is IDrawableFont)
							_caDrawRefList[((IDrawableFont)oObj).sFontName].Remove(nObj.iId);
						else _caDrawRefList[((IDrawableTexture)oObj).sTexName].Remove(nObj.iId);

						// remove the clickable since you can't see it 
						_caClickable.Remove(nObj.iId);
					}

				// remove any objects no longer active
				if(oObj is IActiveBasic) { 
					if((nObj.eObjState & EObjectStates.Active) == EObjectStates.Active)
						naActive.Add((IActiveBasic)oObj);
					else _caActiveList.Remove(nObj.iId);
				}

				// if the object has no state left then blow it away
				if(nObj.eObjState == 0)
					iaRemList.Add(nObj.iId);
			}

			// remove all objects from the main processing list
			foreach(int iObj in iaRemList)
				_caMainObjectList.Remove(iObj);

			// process all the active objects
			foreach(IActiveBasic nActiveObj in naActive)
				nActiveObj.Process(cTime);
		}

		public void Draw(SpriteBatch cBatch)
		{
			// draw each object by their texture
			foreach(KeyValuePair<string, Dictionary<int, IDrawable>> tDrawList in _caDrawRefList)
				foreach(KeyValuePair<int, IDrawable> tObj in tDrawList.Value)
					tObj.Value.Draw(cBatch);
		}

		public void Clear()
		{
			_caActiveList.Clear();
			foreach(KeyValuePair<string, Dictionary<int, IDrawable>> tDrawList in _caDrawRefList)
				tDrawList.Value.Clear();
			_caDrawRefList.Clear();
			_caMainObjectList.Clear();
		}
	}
}
