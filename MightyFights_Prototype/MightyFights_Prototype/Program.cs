using System;

namespace MightyFights_Prototype
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GameShell game = new GameShell())
            {
                game.Run();
            }
        }
    }
#endif
}

