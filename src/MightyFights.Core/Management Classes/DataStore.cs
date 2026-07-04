using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Linq;

using Microsoft.Xna.Framework.Media;

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
		//// ddhj: kaiju -- toggle with F2 (GameShell.Update). Draws each combatant's real
		//// tCenter (a dot) and bDir facing (a short line) so the direction-flip/ref-point bug
		//// can be diagnosed visually instead of guessed at from code. Owner: use this during
		//// playtest to pin down exactly what "worse" looks like.
		public bool				bDebugCenters	{ get; set; }
		public bool				bShowBg			{ get; set; }
		public Song				cBgm			{ get; set; }

		// the steward for the battle
		public Steward			cLSteward		{ get; set; }
		public Steward			cRSteward		{ get; set; }

		//// ddhj: Phase 5 -- pluggable save backing store. Lazily defaults to the desktop filesystem;
		//// the web head can inject a localStorage implementation before any save/load occurs.
		private ISaveStorage	_cSaveStorage;
		public ISaveStorage		cSaveStorage
		{
			get { return _cSaveStorage ?? (_cSaveStorage = new FileSaveStorage()); }
			set { _cSaveStorage = value; }
		}

		private const string	sSaveKey = "SaveData.json";

		public void SaveData()
		{
			cSaveStorage.Write(sSaveKey, SaveSerializer.ToJson(cLSteward));
		}

		public void LoadData()
		{
			if(cSaveStorage.Exists(sSaveKey)) {
				cLSteward = SaveSerializer.FromJson(cSaveStorage.Read(sSaveKey));
				cLSteward.HydrateCompanyRefLists();
			}
		}

		public void InitNew()
		{
			//// ddhj: 2026 -- artist-tunable starting stats (content/Config/CombatTuning.json,
			//// see CombatTuning.cs) for the footman and Peasant baselines. Only applies here, at
			//// first-ever campaign creation: past this point cLSteward.cTemplates[*].cStats is
			//// live player progression (Template-editor leveling), so it must never be
			//// re-clobbered from the config on a later boot.
			CombatTuning	cTuning = CombatTuning.Load();

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
			DataStore.cInstance.cLSteward.cTemplates[0].cStats = cTuning.Halberdier;
			DataStore.cInstance.cLSteward.cTemplates[0].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[0].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[0].sTemplateName = "Initial Template";
			DataStore.cInstance.cLSteward.cTemplates[0].iCount = 20;
			DataStore.cInstance.cLSteward.cTemplates[0].iBLeftPos = DataStore.cInstance.cLSteward.cTemplates[0].iTLeftPos = 466;
			DataStore.cInstance.cLSteward.cTemplates[0].iId = 0;

			//// ddhj: 2026 -- Peasant's persisted roster slot (docs/DESIGN_DIRECTION.md kaiju
			//// leveling task). Previously only existed as a disposable per-hunt config in
			//// KaijuHunt.SetupBattle with no bank target; giving it a real Steward slot lets it
			//// bank exp and level like the Halberdier. Stats come from the same tuning config.
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Peasant\PeasantArray", @"Sprite Data\Troopers\Peasant\Peasant")));
			DataStore.cInstance.cLSteward.cTemplates[1].cStats = cTuning.Peasant;
			DataStore.cInstance.cLSteward.cTemplates[1].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[1].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[1].sTemplateName = "Peasant";
			DataStore.cInstance.cLSteward.cTemplates[1].iCount = 20;
			DataStore.cInstance.cLSteward.cTemplates[1].iBLeftPos = DataStore.cInstance.cLSteward.cTemplates[1].iTLeftPos = 466;
			DataStore.cInstance.cLSteward.cTemplates[1].iId = 1;

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
