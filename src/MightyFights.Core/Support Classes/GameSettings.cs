using System;
using System.IO;
using System.Text.Json;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 -- general app settings the owner can flip without recompiling, starting with
	//// fullscreen vs windowed. Lives at content/Config/GameSettings.json, same self-creating/
	//// copied-alongside-the-build convention as CombatTuning.cs.
	public class GameSettings
	{
		public bool	bFullscreen	{ get; set; }

		static readonly JsonSerializerOptions _cJsonOpts = new JsonSerializerOptions { WriteIndented = true };

		public static GameSettings Defaults()
		{
			return new GameSettings { bFullscreen = false };
		}

		public static GameSettings Load()
		{
			string	sPath = DataManager.ResolveContentPath(@"Config\GameSettings", ".json");

			try {
				if(!File.Exists(sPath)) {
					Directory.CreateDirectory(Path.GetDirectoryName(sPath));
					File.WriteAllText(sPath, JsonSerializer.Serialize(Defaults(), _cJsonOpts));
				}

				return JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(sPath), _cJsonOpts);
			} catch(Exception xEx) {
				System.Diagnostics.Debug.WriteLine("GameSettings.Load failed, using defaults: " + xEx.ToString());
				return Defaults();
			}
		}
	}
}
