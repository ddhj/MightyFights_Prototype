using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

			// the battle heuristics are also going to be set here or in some other area based on template config data
			cTemplate.cAiData = new AiBattleData();

			return cTemplate;
		}
	}
}
