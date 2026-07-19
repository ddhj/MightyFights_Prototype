// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 Village mode (docs/my_vision.txt + docs/food.txt). Shared data model for the
	//// settlement/economy layer: a region is founded, two vocations are chosen, then a weekly turn
	//// loop runs the food economy. When livestock is raided (predation), the player can DEFEND,
	//// which pushes a real field battle on the combat engine (VillageRaid) exactly like WarClash --
	//// or IGNORE and let production ratchet down. Nothing here touches the shared combat engine;
	//// these are mode-local types passed across the scene-stack push.

	//// where a vocation can be practised. A region carries a set of these; a vocation is available
	//// only if its required terrain is None or is present in the region.
	[Flags]
	public enum ETerrain
	{
		None		= 0,
		Pasture		= 1,
		Fields		= 2,
		Woods		= 4,
		Mountains	= 8,
		Water		= 16
	}

	public enum EVocation
	{
		MarketHogs,
		CattlePastures,
		GoatsSheep,
		Poultry,
		Fishing,
		FruitOrchards,
		HerbGardening,
		HorseBreeding,
		BarleyFields,
		GrapeVineyards,
		Forestry,
		StoneMasonry
	}

	//// static description of one vocation. iFoodPct is the % of the village's weekly food need this
	//// vocation produces on its own (per food.txt: Hogs 300%, Cattle 200%, ... Orchards 25%);
	//// non-food vocations are 0 and just trickle coin + flavor. iPredationPct is the weekly chance a
	//// giant/troll raids it (food.txt). eNeeds is the terrain it requires to be selectable.
	public class VocationDef
	{
		public string	sName;
		public int		iFoodPct;
		public int		iPredationPct;
		public ETerrain	eNeeds;
		public string	sMarket;	// what it sells at Market (flavor)
		public string	sBonus;		// its pre/post-battle or narrative perk (flavor)

		public bool IsFood	{ get { return iFoodPct > 0; }}
	}

	public static class VillageRules
	{
		//// the table straight out of my_vision.txt / food.txt. Order matches EVocation.
		public static readonly Dictionary<EVocation, VocationDef> cDefs = new Dictionary<EVocation, VocationDef> {
			{ EVocation.MarketHogs,     new VocationDef { sName = "Market Hogs",     iFoodPct = 300, iPredationPct = 60, eNeeds = ETerrain.None,      sMarket = "Cuts",      sBonus = "+Large Beast raids" }},
			{ EVocation.CattlePastures, new VocationDef { sName = "Cattle Pastures", iFoodPct = 200, iPredationPct = 40, eNeeds = ETerrain.Pasture,   sMarket = "Butter",    sBonus = "Yeomen cheaper" }},
			{ EVocation.GoatsSheep,     new VocationDef { sName = "Goats & Sheep",   iFoodPct = 150, iPredationPct = 30, eNeeds = ETerrain.None,      sMarket = "Wool",      sBonus = "+Large Beast raids" }},
			{ EVocation.Poultry,        new VocationDef { sName = "Poultry",         iFoodPct = 75,  iPredationPct = 15, eNeeds = ETerrain.None,      sMarket = "Feathers",  sBonus = "Archers cheaper" }},
			{ EVocation.Fishing,        new VocationDef { sName = "Fishing",         iFoodPct = 50,  iPredationPct = 0,  eNeeds = ETerrain.Water,     sMarket = "Oil",       sBonus = "Boating cheaper" }},
			{ EVocation.FruitOrchards,  new VocationDef { sName = "Fruit Orchards",  iFoodPct = 25,  iPredationPct = 0,  eNeeds = ETerrain.None,      sMarket = "Cider",     sBonus = "Hog roughage" }},
			{ EVocation.HerbGardening,  new VocationDef { sName = "Herb Gardening",  iFoodPct = 0,   iPredationPct = 0,  eNeeds = ETerrain.None,      sMarket = "Herbs",     sBonus = "Mages cheaper" }},
			{ EVocation.HorseBreeding,  new VocationDef { sName = "Horse Breeding",  iFoodPct = 0,   iPredationPct = 0,  eNeeds = ETerrain.Pasture,   sMarket = "Steeds",    sBonus = "Knights cheaper" }},
			{ EVocation.BarleyFields,   new VocationDef { sName = "Barley Fields",   iFoodPct = 0,   iPredationPct = 0,  eNeeds = ETerrain.Fields,    sMarket = "Ale",       sBonus = "Sells feed" }},
			{ EVocation.GrapeVineyards, new VocationDef { sName = "Grape Vineyards", iFoodPct = 0,   iPredationPct = 0,  eNeeds = ETerrain.Fields,    sMarket = "Wine",      sBonus = "Sells hog feed" }},
			{ EVocation.Forestry,       new VocationDef { sName = "Forestry",        iFoodPct = 0,   iPredationPct = 0,  eNeeds = ETerrain.Woods,     sMarket = "Lumber",    sBonus = "Axe bonus; +monsters" }},
			{ EVocation.StoneMasonry,   new VocationDef { sName = "Stone Masonry",   iFoodPct = 0,   iPredationPct = 0,  eNeeds = ETerrain.Mountains, sMarket = "Gemstones", sBonus = "Build castles/walls" }},
		};

		public static VocationDef Def(EVocation eVoc)	{ return cDefs[eVoc]; }

		public static bool AvailableIn(EVocation eVoc, ETerrain eRegion)
		{
			ETerrain	eNeeds = cDefs[eVoc].eNeeds;
			return eNeeds == ETerrain.None || (eRegion & eNeeds) != 0;
		}
	}

	//// a foundable place on the world map (my_vision.txt "THE MAP"). Screen-space hotspot for the
	//// region-pick step; its terrain gates which vocations the village can practise.
	public class Region
	{
		public string		sName;
		public ETerrain		eTerrain;
		public string		sBlurb;
		public Rectangle	tHotspot;	// clickable marker on the region_map backdrop
	}

	//// the notice that a vocation's livestock is under attack this week (food.txt). DEFEND pushes a
	//// VillageRaid; IGNORE ratchets the vocation's production down.
	public class PredationEvent
	{
		public EVocation	eVoc;
		public string		sRaider;	// "a pack of Trolls", "a Hungry Giant"
		public int			iRaiders;	// how many monster troopers show up if defended
	}

	//// strategic -> tactical: immutable inputs to one defence battle. Defenders are the village's
	//// muster; raiders are the predators. Mirrors WarClashPlan.
	public class VillageRaidPlan
	{
		public TemplateCfgMaster	cDefenderCfg,
									cRaiderCfg;
		public int					iDefenders,
									iRaiders;
		public string				sVocation,	// what's being defended, for the HUD
									sRaider;
	}

	//// tactical -> strategic: written by VillageRaid.OnBattleOver before teardown, polled by Village
	//// once control returns up the stack. Mirrors WarClashResult.
	public class VillageRaidResult
	{
		public bool	bComplete;
		public int	iWinnerSide;		// 0 = village, 1 = raiders
		public int	iDefendersLost;		// muster casualties -> villagers lost
	}
}
