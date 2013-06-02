using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sprite_Color_Swapper
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			string[]	saArgs = Environment.GetCommandLineArgs( );

			if( saArgs.Length < 2 )
			{
				Console.WriteLine( "The proper way to use this is to drag the base colored sprite onto the executable. The cmfs in the folder will be used to generate the new png files." );
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run( new Form1( saArgs[1] ));
		}
	}
}
