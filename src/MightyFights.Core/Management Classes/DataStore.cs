using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Linq;

using Microsoft.Xna.Framework.Media;
using fastJSON;

namespace MightyFights_Prototype
{
	public sealed class DataStore
	{
		#region Singleton Implementation
		
		private static readonly DataStore _cInstance = new DataStore();
		public static DataStore cInstance	{ get { return _cInstance; }} 
		private DataStore() { }
		
		#endregion

		public Game			cGame		{ get; set; }
		public GameTime		tTime		{ get; set; }
		public SpriteFont	cFont		{ get; set; }
		public Random		cRand		{ get; set; }
		public SceneManager		cSceneMgr	{ get; set; }
		public ContentManager	cContent	{ get; set; }
		public GraphicsDevice	cGraphics	{ get; set; }
		public BattlegroundData	cBattleData	{ get; set; }
		public GraphicsDeviceManager cGfxMgr		{ get; set; }

		public Dictionary<string, TemplateConfig>	cTemplates		{ get; set; }
		public Dictionary<string, TemplateConfig>	cCaptains		{ get; set; }
		//public Dictionary<string 

		// the template config from the main menu, this will be replaced with realness at some point
		public TemplateConfig	cLeftConfig		{ get; set; }
		public TemplateConfig	cRightConfig	{ get; set; }

		//// ddhj: debug data
		public Texture2D		cBorder		{ get; set; }

		//// ddhj: battle ground toggles that are processed in non battleground areas
		public bool				bLifeBars		{ get; set; }
		public bool				bDamageNumbers	{ get; set; }
		public bool				bSlowMo			{ get; set; }
		public bool				bHealSpots		{ get; set; }
		public bool				bPlayMusic		{ get; set; }
		public bool				bZoneDisplay	{ get; set; }
		public bool				bShowBg			{ get; set; }
		public Song				cBgm			{ get; set; }

		// the steward for the battle 
		public Steward			cLSteward		{ get; set; }
		public Steward			cRSteward		{ get; set; }

		public void SaveData()
		{
            IsolatedStorageFile cIsoFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            System.IO.FileStream cFile = cIsoFile.OpenFile("SaveData.waf", System.IO.FileMode.OpenOrCreate);
            using(System.IO.StreamWriter cReader = new System.IO.StreamWriter(cFile)) {
				JSONParameters cParam = new JSONParameters();
				cParam.UsingGlobalTypes = false;
				cParam.UseExtensions = false;
                cReader.Write(fastJSON.JSON.Instance.ToJSON(DataStore.cInstance.cLSteward, cParam));
            }
		}

		public void LoadData()
		{
			IsolatedStorageFile cIsoFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
			if(cIsoFile.FileExists("SaveData.waf")) {
				System.IO.FileStream cFile = cIsoFile.OpenFile("SaveData.waf", System.IO.FileMode.Open);
				using(System.IO.StreamReader cReader = new System.IO.StreamReader(cFile)) {
					cLSteward = fastJSON.JSON.Instance.ToObject<Steward>(cReader.ReadLine());
				}
				
				cLSteward.HydrateCompanyRefLists();
			}
		}

		public void InitNew()
		{
			DataStore.cInstance.cLSteward = new Steward();
			// make the first company
			// create the captain data
			DataStore.cInstance.cLSteward.cCaptains.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fcrimson")));
			DataStore.cInstance.cLSteward.cCaptains[0].cStats = new Stats { iAtkSpeed = 20, iMovement = 20, fHp = 600, iMaxHp = 600, iPower = 40, fCrit = .13f, iHealPoint = 240, iFleePoint = 30, iArmorClass = 25 };
			DataStore.cInstance.cLSteward.cCaptains[0].iBottomLevel = DataStore.cInstance.cLSteward.cCaptains[0].iTopLevel = 6;
			DataStore.cInstance.cLSteward.cCaptains[0].sTemplateName = "Phillip";
			DataStore.cInstance.cLSteward.cCaptains[0].iCount = 1;
			
			// add the basic guys (these start at level 1)
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fazure")));
			DataStore.cInstance.cLSteward.cTemplates[0].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[0].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[0].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[0].sTemplateName = "Initial Template";
			DataStore.cInstance.cLSteward.cTemplates[0].iCount = 20;
			DataStore.cInstance.cLSteward.cTemplates[0].iBLeftPos = DataStore.cInstance.cLSteward.cTemplates[0].iTLeftPos = 466;
			DataStore.cInstance.cLSteward.cTemplates[0].iId = 0;

//// the initial company with refereneces back to the template
			Company cCompany = new Company();
			cCompany.sName = "1st Company";
			cCompany.caTemplates.Add(new SelectedTemplate(0, 10, 10));
			cCompany.iMaxSize = 10;
			cCompany.sName = "Initial Company";
			cCompany.sIconName = "In Game\\Camp\\Company Dialog\\axe";
			DataStore.cInstance.cLSteward.caCompanies.Add(cCompany);
			DataStore.cInstance.cLSteward.hCompaniesByName.Add(cCompany.sName, cCompany);
			DataStore.cInstance.cLSteward.hCompaniesByIconName.Add(cCompany.sIconName, cCompany);
		}
	}
}
