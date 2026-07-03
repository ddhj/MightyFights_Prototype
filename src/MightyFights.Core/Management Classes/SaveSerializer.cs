using System.Text.Json;

namespace MightyFights_Prototype
{
	// Central System.Text.Json configuration for save data (Phase 5 replacement for fastJSON).
	// Only the player Steward graph is persisted, and it is all plain POCOs (strings/ints/floats,
	// plus string- and enum-keyed dictionaries) with no XNA types, so no custom converters are
	// needed. Two things in the graph must be kept out of the JSON to round-trip correctly, both
	// handled with [JsonIgnore] at the type:
	//   - Steward.hCompaniesByName / hCompaniesByIconName: derived lookup dictionaries rebuilt by
	//     HydrateCompanyRefLists() after load; serializing them would double-populate and throw.
	//   - Company.caTemplates: a get-only computed copy of a private backing dictionary.
	// See PORT_NOTES.md Phase 5 for the full R2 disposition.
	internal static class SaveSerializer
	{
		static readonly JsonSerializerOptions _cOptions = new JsonSerializerOptions
		{
			IncludeFields	= false,
			WriteIndented	= false,
		};

		public static string ToJson(Steward cSteward)
		{
			return JsonSerializer.Serialize(cSteward, _cOptions);
		}

		public static Steward FromJson(string sJson)
		{
			return JsonSerializer.Deserialize<Steward>(sJson, _cOptions);
		}
	}
}
