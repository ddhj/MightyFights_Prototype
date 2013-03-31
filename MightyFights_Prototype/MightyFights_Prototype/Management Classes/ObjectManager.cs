using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class ObjectManager
	{
		#region Singleton Implementation
		
		private static readonly ObjectManager _cInstance = new ObjectManager();
		public static ObjectManager cInstance	{ get { return _cInstance; }} 
		private ObjectManager() { }
		
		#endregion

		Dictionary<string, AnimationData>	_cAnimationDataList = new Dictionary<string,AnimationData>();
		Dictionary<string, Texture2D>		_cTextureList = new Dictionary<string,Texture2D>();

		public TrooperTemplate CreateTemplate(TemplateConfig cTemplateData)
		{
			TrooperTemplate cTemplate = new TrooperTemplate();
			DataStore.cInstance.cContent.Load<AnimationData>(cTemplateData.sTrooperType);
			return cTemplate;
		}
	}
}
