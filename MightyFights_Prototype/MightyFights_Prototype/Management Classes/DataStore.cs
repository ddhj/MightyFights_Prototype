using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Linq;

namespace MightyFights_Prototype
{
	public sealed class DataStore
	{
		#region Singleton Implementation
		
		private static readonly DataStore _cInstance = new DataStore();
		public static DataStore cInstance	{ get { return _cInstance; }} 
		private DataStore() { }
		
		#endregion

		public SceneManager		cSceneMgr	{ get; set; }
		public ContentManager	cContent	{ get; set; }
		public GraphicsDevice	cGraphics	{ get; set; }
		public BattlegroundData	cBattleData	{ get; set; }
		public GameTime			cTime		{ get; set; }
	}
}
