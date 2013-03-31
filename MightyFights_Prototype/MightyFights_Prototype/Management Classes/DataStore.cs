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

		ContentManager	_cContent;
		GraphicsDevice	_cGraphics;
		
		public SceneManager	cSceneMgr	{get; set;}
		public ContentManager	cContent	{ get { return _cContent; } set { _cContent = value; }}
		public GraphicsDevice	cGraphics	{ get { return _cGraphics; } set { _cGraphics = value; }}
	}
}
