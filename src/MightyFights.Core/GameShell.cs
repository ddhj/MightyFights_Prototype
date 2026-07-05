using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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

			//// ddhj: 2026 -- content/Config/GameSettings.json, editable without recompiling.
			//// Applied here (not the ctor) since DataStore.cInstance.cContent -- needed to resolve
			//// the config's path -- isn't wired up until this point; ApplyChanges runs before the
			//// window is first presented, so there's no visible windowed-then-fullscreen flash.
			GameSettings	cSettings = GameSettings.Load();
			graphics.IsFullScreen = cSettings.bFullscreen;
			graphics.ApplyChanges();

			//// ddhj: 2026 -- load the saved campaign if one exists (kaiju hunts bank survivor
			//// exp into the roster and save; a fresh boot must not wipe it), else start new.
			//// LoadData is a no-op when no save file is present.
			DataStore.cInstance.LoadData();
			if(DataStore.cInstance.cLSteward == null)
				DataStore.cInstance.InitNew();

////right side
			DataStore.cInstance.cRSteward = new Steward();
			// create the captain data
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsable")));
			DataStore.cInstance.cRSteward.cTemplates[0].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 };
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
			DataStore.cInstance.bPlayMusic = true;		//// ddhj: was false in debug boot; enabled in Phase 6 to verify Song/MP3 playback (V6.1). Toggle back if you want silent dev runs.
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
		bool _bF2WasDown;

		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			// PORT (T1.4): guarded -- GamePad support is not guaranteed on the future
			// KNI BlazorGL web head; Desktop/DesktopGL keeps this behavior unchanged.
#if !BLAZORGL
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();
#endif

			//// ddhj: kaiju -- F2 toggles the tCenter/bDir debug draw (DataStore.bDebugCenters).
			bool bF2Down = Keyboard.GetState().IsKeyDown(Keys.F2);
			if(bF2Down && !_bF2WasDown)
				DataStore.cInstance.bDebugCenters = !DataStore.cInstance.bDebugCenters;
			_bF2WasDown = bF2Down;

			// PORT (Phase 6): pump the higher-level input events. Replaces the old Win32 WndProc
			// hook (which fired these during message dispatch); now polled once per frame before
			// the scene components update. See InputManager.cs / PORT_NOTES.md Phase 6.
			InputSystem.Update(gameTime);

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
