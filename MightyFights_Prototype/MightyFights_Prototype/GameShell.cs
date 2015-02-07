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

			DataStore.cInstance.LoadData();
			//DataStore.cInstance.InitNew();

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
