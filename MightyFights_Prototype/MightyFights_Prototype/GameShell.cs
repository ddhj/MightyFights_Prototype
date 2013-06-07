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
*/
			//// ddhj: the prototype steward gets three templates
////left side
			DataStore.cInstance.cLSteward = new Steward();
			// create the captain data
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fcrimson")));
			DataStore.cInstance.cLSteward.cTemplates[0].cStats = new Stats { iAtkSpeed = 14, iMovement = 14, fHp = 150, iMaxHp = 150, iPower = 20 };
			DataStore.cInstance.cLSteward.cTemplates[0].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[0].iTopLevel = 4;
			// add the basic guys (these start at level 1)
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fazure")));
			DataStore.cInstance.cLSteward.cTemplates[1].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5 };
			DataStore.cInstance.cLSteward.cTemplates[1].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[1].iTopLevel = 0;
			DataStore.cInstance.cLSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fbrown")));
			DataStore.cInstance.cLSteward.cTemplates[2].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5 };
			DataStore.cInstance.cLSteward.cTemplates[2].iBottomLevel = DataStore.cInstance.cLSteward.cTemplates[2].iTopLevel = 0;

////right side
			DataStore.cInstance.cRSteward = new Steward();
			// create the captain data
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsable")));
			DataStore.cInstance.cRSteward.cTemplates[0].cStats = new Stats { iAtkSpeed = 14, iMovement = 14, fHp = 150, iMaxHp = 150, iPower = 20 };
			DataStore.cInstance.cRSteward.cTemplates[0].iBottomLevel = DataStore.cInstance.cRSteward.cTemplates[0].iTopLevel = 4;
			// add the basic guys (these start at level 1)
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fsteel")));
			DataStore.cInstance.cRSteward.cTemplates[1].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5 };
			DataStore.cInstance.cRSteward.cTemplates[1].iBottomLevel = DataStore.cInstance.cRSteward.cTemplates[1].iTopLevel = 0;
			DataStore.cInstance.cRSteward.cTemplates.Add(new TemplateCfgMaster(new TemplateConfig(@"Sprite Data\Troopers\Halberd\HalberdArray", @"Sprite Data\Troopers\Halberd\Textures\fstorm")));
			DataStore.cInstance.cRSteward.cTemplates[2].cStats = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5 };
			DataStore.cInstance.cRSteward.cTemplates[2].iBottomLevel = DataStore.cInstance.cRSteward.cTemplates[2].iTopLevel = 0;

			// init the object manager
			ObjectCreationManager.cInstance.Init();

			// set the scene manager
			SceneManager	cManager = new SceneManager(this);

			// add the main menu as the initial drawing object
			IGameScene		nScene = new MainMenu();
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
