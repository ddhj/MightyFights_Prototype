using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class GameShell : Microsoft.Xna.Framework.Game
	{
		public GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public GameShell()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferHeight = 576;
			graphics.PreferredBackBufferWidth = 1024;
			graphics.IsFullScreen = false;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// init the data store
			DataStore.cInstance.cContent = this.Content;
			DataStore.cInstance.cGraphics = this.GraphicsDevice;
			DataStore.cInstance.cGfxMgr = this.graphics;
			DataStore.cInstance.cGame = this;
			DataStore.cInstance.cRand = new Random();

/* some ref for the below hard code of the initial templates
		static int[]	_iaAtkSpeedStats = new int[] { 0, 4, 10, 14, 18, 19, 20, 21, 22, 25, 30, 40, 50, 70 };
		// these are % increases to the base movement speed
		static int[]	_iaMoveSpeedStats = new int[] { 0, 4, 10, 14, 18, 19, 20, 21, 22, 25, 30, 40, 50, 70 };

		// these are just raw stats
		static int[]	_iaHitPointStats = new int[] { 100, 110, 120, 150, 200, 400, 600, 800, 1200, 1600, 2000, 2200, 2400, 3000 };
		static int[]	_iaAtkPowerStats = new int[] { 5, 10, 15, 20, 30, 35, 40, 50, 70, 90, 200, 300, 500, 800 };
		static float[]	_fCrit = new float[] { .05f, .06f, .07f, .1f, .11f, .12f, .13f, .18f, .2f, .25f, .30f, .50f	};
 		static int[]	_iaAC = new int[] { 3, 4, 5, 10, 15, 20, 25, 40, 45, 50, 100, 150, 200, 400 };
 */
			//// ddhj: the prototype steward gets three templates
////left side
			DataStore.cInstance.cLSteward = new Steward();
			// create the captain data
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fcrimson")));
			DataStore.cInstance.cLSteward.cTemplates[0].cStats = new Stats { iAtkSpeed = 20, iMovement = 20, fHp = 600, iMaxHp = 600, iPower = 40, fCrit = .13f, iHealPoint = 240, iFleePoint = 30, iArmorClass = 25 };
			DataStore.cInstance.cLSteward.cTemplates[0].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[0].iTopLevel = 6;
			DataStore.cInstance.cLSteward.cTemplates[0].sTemplateName = "LCap";
			DataStore.cInstance.cLSteward.cTemplates[0].iCount = 15;
			// add the basic guys (these start at level 1)
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fazure")));
			DataStore.cInstance.cLSteward.cTemplates[1].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[1].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[1].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[1].sTemplateName = "LT1";
			DataStore.cInstance.cLSteward.cTemplates[1].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[2].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[2].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[2].sTemplateName = "LT2";
			DataStore.cInstance.cLSteward.cTemplates[2].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[3].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[3].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[3].sTemplateName = "LT3";
			DataStore.cInstance.cLSteward.cTemplates[3].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[4].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[4].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[4].sTemplateName = "LT4";
			DataStore.cInstance.cLSteward.cTemplates[4].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[5].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[5].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[5].sTemplateName = "LT5";
			DataStore.cInstance.cLSteward.cTemplates[5].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[6].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[6].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[6].sTemplateName = "LT6";
			DataStore.cInstance.cLSteward.cTemplates[6].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[7].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[7].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[7].sTemplateName = "LT7";
			DataStore.cInstance.cLSteward.cTemplates[7].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[8].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[8].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[8].sTemplateName = "LT8";
			DataStore.cInstance.cLSteward.cTemplates[8].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[9].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[9].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[9].sTemplateName = "LT9";
			DataStore.cInstance.cLSteward.cTemplates[9].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[10].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[10].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[10].sTemplateName = "LT10";
			DataStore.cInstance.cLSteward.cTemplates[10].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[11].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[11].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[11].sTemplateName = "LT11";
			DataStore.cInstance.cLSteward.cTemplates[11].iCount = 15;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[12].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cLSteward.cTemplates[12].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates[12].sTemplateName = "LT12";
			DataStore.cInstance.cLSteward.cTemplates[12].iCount = 15;

////right side
			DataStore.cInstance.cRSteward = new Steward();
			// create the captain data
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsable")));
			DataStore.cInstance.cRSteward.cTemplates[0].cStats = new Stats { iAtkSpeed = 20, iMovement = 20, fHp = 600, iMaxHp = 600, iPower = 40, fCrit = .12f, iHealPoint = 240, iFleePoint = 30, iArmorClass = 25 };
			DataStore.cInstance.cRSteward.cTemplates[0].iBottomLevel = DataStore.cInstance.cRSteward.cTemplates[0].iTopLevel = 6;
			DataStore.cInstance.cRSteward.cTemplates[0].sTemplateName = "RCap";
			// add the basic guys (these start at level 1)
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsteel")));
			DataStore.cInstance.cRSteward.cTemplates[1].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cRSteward.cTemplates[1].iBottomLevel = DataStore.cInstance.cRSteward.cTemplates[1].iTopLevel = 0;
			DataStore.cInstance.cRSteward.cTemplates[1].sTemplateName = "RT1";
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fstorm")));
			DataStore.cInstance.cRSteward.cTemplates[2].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
			DataStore.cInstance.cRSteward.cTemplates[2].iBottomLevel = DataStore.cInstance.cRSteward.cTemplates[2].iTopLevel = 0;
			DataStore.cInstance.cRSteward.cTemplates[2].sTemplateName = "RT2";

			// global toggles 
			DataStore.cInstance.bPlayMusic = false;
			DataStore.cInstance.bDamageNumbers = true;
			DataStore.cInstance.bHealSpots = false;
			DataStore.cInstance.bLifeBars = false;
			DataStore.cInstance.bShowBg = true;
			DataStore.cInstance.bSlowMo = false;
			DataStore.cInstance.bZoneDisplay = false;

			// init the object manager
			DataManager.cInstance.Init();

			// set the scene manager
			SceneManager	cManager = new SceneManager(this);

			// set the music in the game to be repeted if it runs long
			MediaPlayer.IsRepeating = true;

			// set the input system to the window
			InputSystem.Initialize(Window);

			// add the main menu as the initial drawing object
			IGameScene		nScene = new TitleScreen();
			if(nScene.Init()) { 
				cManager.AddScene(nScene);
				Components.Add(cManager);
			}

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			
			// load the sprite font into the data store
			DataStore.cInstance.cFont = Content.Load<SpriteFont>(@"Shared\DebugFont");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here

			base.Draw(gameTime);
		}
	}
}
