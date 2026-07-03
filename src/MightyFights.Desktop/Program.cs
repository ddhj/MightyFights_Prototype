using System;

namespace MightyFights_Prototype
{
	public static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			using (var game = new GameShell())
				game.Run();
		}
	}
}
