using System;
using System.IO;
using System.Text.Json;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 -- lets the artist retune Kaiju Hunt's three active combatants (Kaiju,
	//// Halberdier, Peasant) from a plain JSON file instead of recompiling. Lives at
	//// content/Config/CombatTuning.json, copied alongside the game by the existing
	//// content/**/*.json build glob (same mechanism AnimationData JSONs already use -- see
	//// MightyFights.Desktop.csproj), so it round-trips through a distributed build too.
	public class CombatTuning
	{
		public Stats	Kaiju			{ get; set; }
		public Stats	Halberdier		{ get; set; }
		public Stats	Peasant			{ get; set; }
		public float	fKaijuScale		{ get; set; }

		static readonly JsonSerializerOptions _cJsonOpts = new JsonSerializerOptions { WriteIndented = true };

		/// <summary> the shipped balance as of 2026-07 -- also what gets written out as the
		/// starter file the first time nobody has one to edit yet </summary>
		public static CombatTuning Defaults()
		{
			return new CombatTuning {
				Kaiju = new Stats { iAtkSpeed = 25, iMovement = 5, fHp = 6000, iMaxHp = 6000, iPower = 55, fCrit = .15f, iHealPoint = 0, iFleePoint = 0, iArmorClass = 3 },
				Halberdier = new Stats { iAtkSpeed = 0, iMovement = 0, fHp = 100, iMaxHp = 100, iPower = 5, fCrit = .05f, iHealPoint = 40, iFleePoint = 5, iArmorClass = 3 },
				Peasant = new Stats { iAtkSpeed = 0, iMovement = 5, fHp = 60, iMaxHp = 60, iPower = 3, fCrit = .03f, iHealPoint = 20, iFleePoint = 10, iArmorClass = 1 },
				fKaijuScale = 2.5f,
			};
		}

		/// <summary> loads content/Config/CombatTuning.json (or the shipped Content/Config copy
		/// next to a distributed build); writes the defaults out as a starter file the first time
		/// it's missing, so the artist has real field names/values to edit rather than a blank
		/// page. Falls back to Defaults() on any read/parse error so a bad edit can't crash a
		/// hunt -- just logs and keeps the last-known-good numbers. </summary>
		public static CombatTuning Load()
		{
			string	sPath = DataManager.ResolveContentPath(@"Config\CombatTuning", ".json");

			try {
				if(!File.Exists(sPath)) {
					Directory.CreateDirectory(Path.GetDirectoryName(sPath));
					File.WriteAllText(sPath, JsonSerializer.Serialize(Defaults(), _cJsonOpts));
				}

				return JsonSerializer.Deserialize<CombatTuning>(File.ReadAllText(sPath), _cJsonOpts);
			} catch(Exception xEx) {
				System.Diagnostics.Debug.WriteLine("CombatTuning.Load failed, using defaults: " + xEx.ToString());
				return Defaults();
			}
		}
	}
}
