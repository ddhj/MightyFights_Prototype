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
	}
}
