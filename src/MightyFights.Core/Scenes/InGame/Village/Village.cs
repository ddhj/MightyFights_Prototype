// system includes
using System;
using System.Text;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 Village mode (docs/my_vision.txt + docs/food.txt). The settlement/economy layer:
	//// found a village in a region, pick two vocations, then run a weekly turn loop -- food is
	//// produced as a % of the population's need, surplus sells at Market for coin, and each week your
	//// livestock may be raided. DEFEND a raid and it drops into a real field battle on the combat
	//// engine (VillageRaid, pushed on the scene stack, result polled back exactly like WarBattle ->
	//// WarClash); IGNORE it and that vocation's production ratchets down. Survive; if food or
	//// population hits zero, the village dies. Art backdrops are the only new content; all UI is
	//// spritefont + a 1x1 pixel, matching WarBattle.
	public class Village : IGameScene
	{
		enum EVillagePhase	{ Region, Vocations, Weekly, Predation, Resolving, GameOver }

		ESceneStates	_eState;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		SpriteFont		_cFont;
		Cursor			_cCursor;
		Texture2D		_cPixel;
		Random			_cRnd = new Random();

		// art backdrops
		Texture2D		_cMapBg,
						_cVocBg,
						_cVillageBg,
						_cSkyline;

		EVillagePhase	_ePhase = EVillagePhase.Region;
		Point			_tMouse;

		// region pick
		List<Region>	_caRegions = new List<Region>();
		Region			_cRegion;
		int				_iHoverRegion = -1;

		// vocation pick
		List<EVocation>		_caAvail = new List<EVocation>();
		List<EVocation>		_caChosen = new List<EVocation>();
		Rectangle[]			_caVocRects;
		Rectangle			_tFoundRect;
		int					_iHoverVoc = -1;

		// village / economy state
		string					_sVillageName = "";
		int						_iWeek = 1,
								_iPop = 100,
								_iCoin = 40;
		float					_fFood = 30;
		Dictionary<EVocation, int>	_ciCurPct = new Dictionary<EVocation, int>();		// current production %, degrades on IGNORE
		Dictionary<EVocation, int>	_ciPredChance = new Dictionary<EVocation, int>();	// current weekly raid chance, drops on DEFEND
		TemplateCfgMaster		_cDefenderCfg,
								_cRaiderCfg;

		// weekly-screen buttons
		Rectangle		_tNextWeekRect,
						_tForageRect,
						_tReplenishRect,
						_tConfirmYesRect,
						_tConfirmNoRect;
		bool			_bConfirmWeek,
						_bForaged;
		string			_sYield = "Found your village and end the week to see its yield.";

		// predation
		Queue<PredationEvent>	_cPending = new Queue<PredationEvent>();
		PredationEvent			_cCurEvent;
		Rectangle				_tDefendRect,
								_tIgnoreRect;
		VillageRaidResult		_cPendResult;
		string					_sLastRaid = "";

		// market flavor
		int				_iLastPrice = 2;
		bool			_bPriceUp = true;

		// end
		bool			_bThrived;
		const int		iWinWeek = 8;	// survive to here -> the village thrives

		#region IGameScene Members

		public ESceneStates eState	{ get { return _eState; } set { _eState = value; }}

		public bool Init()
		{
			try {
				DataStore	cData = DataStore.cInstance;

				_cGraphics = cData.cGraphics;
				_cFont = cData.cFont;
				_cBatch = new SpriteBatch(_cGraphics);

				_cPixel = new Texture2D(_cGraphics, 1, 1);
				_cPixel.SetData<Color>(new[] { Color.White });

				_cMapBg = cData.cContent.Load<Texture2D>(@"In Game\Village\region_map");
				_cVocBg = cData.cContent.Load<Texture2D>(@"In Game\Village\vocations_bg");
				_cVillageBg = cData.cContent.Load<Texture2D>(@"In Game\Village\village_bg");
				_cSkyline = cData.cContent.Load<Texture2D>(@"In Game\Village\skyline");

				// the muster and its predators are ordinary trooper templates (as WarBattle does):
				// cLSteward[0] is the player's; cRSteward[1] is a plain enemy trooper ([0] is "RCap",
				// which Trooper.cs renders 1.4x oversized).
				_cDefenderCfg = cData.cLSteward.cTemplates[0];
				_cRaiderCfg = cData.cRSteward.cTemplates[1];

				BuildRegions();

				// vocation list rows (left column of the vocations screen)
				_caVocRects = new Rectangle[VillageRules.cDefs.Count];
				for(int iCount = 0; iCount < _caVocRects.Length; ++iCount)
					_caVocRects[iCount] = new Rectangle(40, 120 + iCount * 34, 300, 30);
				_tFoundRect = new Rectangle(_cGraphics.Viewport.Width - 200, _cGraphics.Viewport.Height - 76, 180, 46);

				// weekly-screen buttons (right rail)
				int	iRailX = _cGraphics.Viewport.Width - 200;
				_tNextWeekRect  = new Rectangle(iRailX, _cGraphics.Viewport.Height - 76, 180, 46);
				_tForageRect    = new Rectangle(iRailX, _cGraphics.Viewport.Height - 130, 180, 44);
				_tReplenishRect = new Rectangle(iRailX, _cGraphics.Viewport.Height - 184, 180, 44);
				_tConfirmYesRect = new Rectangle(420, 320, 90, 44);
				_tConfirmNoRect  = new Rectangle(540, 320, 90, 44);

				// predation prompt buttons
				_tDefendRect = new Rectangle(320, 360, 160, 54);
				_tIgnoreRect = new Rectangle(544, 360, 160, 54);

				_cCursor = new Cursor();
				_cCursor.cTexRef = cData.cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.sTexName = @"Shared\arrow_cursor";
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2),
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
			} catch(Exception xEx) {
				System.Diagnostics.Debug.WriteLine(xEx.ToString());
				return false;
			}

			return true;
		}

		public void Update(GameTime cTime)
		{
			// a pushed VillageRaid freezes this scene; once it pops, this Update runs again and we
			// pick up the recorded result. Poll here rather than via any scene callback (as WarBattle).
			if(_ePhase == EVillagePhase.Resolving && _cPendResult != null && _cPendResult.bComplete)
				ResolveRaid();
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Color.Black);

			_cBatch.Begin(); {
				switch(_ePhase) {
					case EVillagePhase.Region:		DrawRegionPick();	break;
					case EVillagePhase.Vocations:	DrawVocationPick();	break;
					default:						DrawVillage();		break;	// Weekly / Predation / Resolving / GameOver share the town backdrop
				}

				if(_ePhase == EVillagePhase.Predation)	DrawPredation();
				else if(_ePhase == EVillagePhase.GameOver)	DrawGameOver();

				_cCursor.Draw(_cBatch);
			} _cBatch.End();
		}

		public void Unload()
		{
			if(_cBatch != null) _cBatch.Dispose();
			if(_cPixel != null) _cPixel.Dispose();
		}

		public void ToggleControls() {}

		public void RegisterHandlers()
		{
			InputSystem.MouseMove += new MouseEventHandler(InputSystem_MouseMove);
			InputSystem.MouseDown += new MouseEventHandler(InputSystem_MouseDown);
			InputSystem.KeyDown += new KeyEventHandler(InputSystem_KeyDown);
		}

		public void UnRegisterHandlers()
		{
			InputSystem.MouseMove -= InputSystem_MouseMove;
			InputSystem.MouseDown -= InputSystem_MouseDown;
			InputSystem.KeyDown -= InputSystem_KeyDown;
		}

		#endregion

	// setup

		void BuildRegions()
		{
			_caRegions.Clear();
			_caRegions.Add(new Region { sName = "Verdant Downs",   eTerrain = ETerrain.Pasture | ETerrain.Fields,   sBlurb = "Rolling pasture and open field.", tHotspot = new Rectangle(300, 330, 180, 38) });
			_caRegions.Add(new Region { sName = "The Boarwood",    eTerrain = ETerrain.Woods,                       sBlurb = "Deep woods, thick with game.",    tHotspot = new Rectangle(150, 200, 170, 38) });
			_caRegions.Add(new Region { sName = "Highstone Reach", eTerrain = ETerrain.Mountains | ETerrain.Woods,  sBlurb = "Crags and quarries.",             tHotspot = new Rectangle(520, 150, 190, 38) });
			_caRegions.Add(new Region { sName = "Saltmarsh Coast", eTerrain = ETerrain.Water,                       sBlurb = "Tidal flats and fishing water.",  tHotspot = new Rectangle(800, 340, 180, 38) });
			_caRegions.Add(new Region { sName = "Goldbarley Plain",eTerrain = ETerrain.Fields | ETerrain.Pasture,   sBlurb = "Wide grain country.",             tHotspot = new Rectangle(450, 96, 200, 38) });
		}

	// region -> vocations -> found

		void ChooseRegion(Region cRegion)
		{
			_cRegion = cRegion;
			_caAvail.Clear();
			_caChosen.Clear();

			foreach(EVocation eVoc in Enum.GetValues(typeof(EVocation)))
				if(VillageRules.AvailableIn(eVoc, cRegion.eTerrain))
					_caAvail.Add(eVoc);

			_ePhase = EVillagePhase.Vocations;
		}

		void ToggleVocation(EVocation eVoc)
		{
			if(_caChosen.Contains(eVoc))		_caChosen.Remove(eVoc);
			else if(_caChosen.Count < 2)		_caChosen.Add(eVoc);
		}

		void FoundVillage()
		{
			if(_caChosen.Count != 2)
				return;

			_sVillageName = _cRegion.sName.Split(' ')[_cRegion.sName.Split(' ').Length - 1] + "hold";

			_ciCurPct.Clear();
			_ciPredChance.Clear();
			foreach(EVocation eVoc in _caChosen) {
				VocationDef	cDef = VillageRules.Def(eVoc);
				if(cDef.IsFood) {
					_ciCurPct[eVoc] = cDef.iFoodPct;
					_ciPredChance[eVoc] = cDef.iPredationPct;
				}
			}

			_iWeek = 1; _iPop = 100; _iCoin = 40; _fFood = 30;
			_bForaged = false; _bConfirmWeek = false;
			_sYield = "A new week begins. Tend your holdings, then end the week.";
			_ePhase = EVillagePhase.Weekly;
		}

	// weekly turn

		int TotalProductionPct()
		{
			int	iSum = 0;
			foreach(KeyValuePair<EVocation, int> tKvp in _ciCurPct)
				iSum += tKvp.Value;
			return iSum;
		}

		void Forage()
		{
			if(_bForaged || _ePhase != EVillagePhase.Weekly)
				return;
			_bForaged = true;
			int	iGain = _cRnd.Next(10, 31);
			_fFood += iGain;
			_sYield = string.Format("Foragers returned with {0} food.", iGain);
		}

		void Replenish()
		{
			if(_ePhase != EVillagePhase.Weekly)
				return;

			int	iCost = 0;
			foreach(EVocation eVoc in _caChosen) {
				VocationDef	cDef = VillageRules.Def(eVoc);
				if(cDef.IsFood)
					iCost += (cDef.iFoodPct - _ciCurPct[eVoc]) / 4;
			}

			if(iCost <= 0 || _iCoin < iCost) {
				_sYield = iCost <= 0 ? "Your herds are already at full strength." : string.Format("Restocking costs {0} coin -- you have {1}.", iCost, _iCoin);
				return;
			}

			_iCoin -= iCost;
			foreach(EVocation eVoc in _caChosen)
				if(VillageRules.Def(eVoc).IsFood)
					_ciCurPct[eVoc] = VillageRules.Def(eVoc).iFoodPct;
			_sYield = string.Format("Restocked the herds for {0} coin.", iCost);
		}

		// NEXT WEEK confirmed: notify intrusions first (food.txt), then resolve the yield
		void BeginNextWeek()
		{
			_bConfirmWeek = false;
			_cPending.Clear();

			foreach(EVocation eVoc in _caChosen) {
				VocationDef	cDef = VillageRules.Def(eVoc);
				if(!cDef.IsFood || _ciCurPct[eVoc] <= 0)
					continue;

				if(_cRnd.Next(100) < _ciPredChance[eVoc]) {
					bool	bBig = _ciPredChance[eVoc] >= 40;
					_cPending.Enqueue(new PredationEvent {
						eVoc = eVoc,
						sRaider = bBig ? "a Hungry Giant" : "a pack of Trolls",
						iRaiders = bBig ? _cRnd.Next(3, 6) : _cRnd.Next(6, 11)
					});
				}
			}

			ProcessNextEvent();
		}

		// walk the predation queue one event at a time; when it empties, resolve the yield
		void ProcessNextEvent()
		{
			if(_cPending.Count == 0) {
				FinalizeWeek();
				return;
			}

			_cCurEvent = _cPending.Dequeue();
			_ePhase = EVillagePhase.Predation;
		}

		// DEFEND: push a real field battle, then poll its result in Update -> ResolveRaid
		void Defend()
		{
			int	iDefenders = Math.Max(8, Math.Min(30, _iPop / 6));

			VillageRaidPlan	cPlan = new VillageRaidPlan {
				cDefenderCfg = _cDefenderCfg,
				cRaiderCfg = _cRaiderCfg,
				iDefenders = iDefenders,
				iRaiders = _cCurEvent.iRaiders,
				sVocation = VillageRules.Def(_cCurEvent.eVoc).sName,
				sRaider = _cCurEvent.sRaider
			};

			_cPendResult = new VillageRaidResult();
			_ePhase = EVillagePhase.Resolving;

			VillageRaid	cRaid = new VillageRaid(cPlan, _cPendResult);
			if(cRaid.Init())
				DataStore.cInstance.cSceneMgr.AddScene(cRaid);
			else {
				// raid failed to init -- treat as an undefended loss rather than hang in Resolving
				_cPendResult = null;
				IgnoreOutcome(_cCurEvent);
				ProcessNextEvent();
			}
		}

		// control is back from the popped raid: per food.txt, putting up a fight PROTECTS production
		// regardless of who won; winning also cows the raiders (lower future chance). Losing costs the
		// fallen muster in villagers.
		void ResolveRaid()
		{
			bool	bWon = _cPendResult.iWinnerSide == 0;

			// production protected -- _ciCurPct[eVoc] untouched
			_ciPredChance[_cCurEvent.eVoc] = Math.Max(0, _ciPredChance[_cCurEvent.eVoc] - (bWon ? 15 : 5));
			_iPop = Math.Max(0, _iPop - _cPendResult.iDefendersLost);

			_sLastRaid = string.Format("{0} at the {1}: {2} ({3} lost defending).",
				_cCurEvent.sRaider, VillageRules.Def(_cCurEvent.eVoc).sName,
				bWon ? "REPELLED" : "OVERRUN", _cPendResult.iDefendersLost);

			_cPendResult = null;
			_cCurEvent = null;
			ProcessNextEvent();
		}

		// IGNORE: the raiders help themselves; that vocation's production is halved (food.txt)
		void IgnoreOutcome(PredationEvent cEvent)
		{
			int	iBefore = _ciCurPct[cEvent.eVoc];
			_ciCurPct[cEvent.eVoc] = iBefore / 2;
			_sLastRaid = string.Format("{0} ravaged the {1}: production {2}% -> {3}%.",
				cEvent.sRaider, VillageRules.Def(cEvent.eVoc).sName, iBefore, _ciCurPct[cEvent.eVoc]);
		}

		// all raids handled: run the food/market math for the week and advance
		void FinalizeWeek()
		{
			int		iNeed = _iPop;
			int		iPct = TotalProductionPct();
			float	fProduced = iNeed * (iPct / 100f);

			StringBuilder	sbYield = new StringBuilder();
			if(_sLastRaid.Length > 0) { sbYield.Append(_sLastRaid); sbYield.Append("  "); _sLastRaid = ""; }

			// consume the week's need out of what was produced plus stores
			_fFood += fProduced - iNeed;

			// famine: stores couldn't cover the need -> villagers starve
			if(_fFood < 0) {
				int	iStarved = (int)Math.Ceiling(-_fFood);
				_iPop = Math.Max(0, _iPop - iStarved);
				_fFood = 0;
				sbYield.Append(string.Format("FAMINE: {0} starved. ", iStarved));
			}

			// emigration: below 100% production, some villagers drift away
			if(iPct < 100 && _iPop > 0) {
				int	iLeave = (int)Math.Ceiling(_iPop * (100 - iPct) / 100f * 0.15f);
				_iPop = Math.Max(0, _iPop - iLeave);
				if(iLeave > 0) sbYield.Append(string.Format("{0} villagers left (short of food). ", iLeave));
			}

			// Market: sell everything above a two-week reserve
			float	fReserve = iNeed * 2;
			if(_fFood > fReserve) {
				int	iSell = (int)(_fFood - fReserve);
				_iLastPrice = _cRnd.Next(1, 4);
				_bPriceUp = _cRnd.Next(2) == 0;
				int	iEarned = iSell * _iLastPrice;
				_iCoin += iEarned;
				_fFood -= iSell;
				sbYield.Append(string.Format("Sold {0} food @ {1} = {2} coin. ", iSell, _iLastPrice, iEarned));
			}

			// non-food vocations trickle coin + flavor
			foreach(EVocation eVoc in _caChosen)
				if(!VillageRules.Def(eVoc).IsFood)
					_iCoin += _cRnd.Next(3, 9);

			// spoilage
			_fFood = (float)Math.Floor(_fFood * 0.9f);

			_iWeek++;
			_sYield = sbYield.Length > 0 ? sbYield.ToString() : "A quiet, prosperous week.";

			_bForaged = false;

			// end conditions (food.txt: pop or food at zero ends the game)
			if(_iPop <= 0 || _fFood <= 0) {
				_bThrived = false;
				_ePhase = EVillagePhase.GameOver;
			} else if(_iWeek > iWinWeek) {
				_bThrived = true;
				_ePhase = EVillagePhase.GameOver;
			} else	_ePhase = EVillagePhase.Weekly;
		}

	// input

		void InputSystem_MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			_tMouse = eMouseEvt.Location;
			_cCursor.Update(eMouseEvt.Location);

			_iHoverRegion = -1;
			if(_ePhase == EVillagePhase.Region)
				for(int iCount = 0; iCount < _caRegions.Count; ++iCount)
					if(_caRegions[iCount].tHotspot.Contains(_tMouse))
						_iHoverRegion = iCount;

			_iHoverVoc = -1;
			if(_ePhase == EVillagePhase.Vocations)
				for(int iCount = 0; iCount < _caAvail.Count; ++iCount)
					if(_caVocRects[iCount].Contains(_tMouse))
						_iHoverVoc = iCount;
		}

		void InputSystem_MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			if(eMouseEvt.Button != MouseButton.Left) {
				if(eMouseEvt.Button == MouseButton.Right && _ePhase == EVillagePhase.Vocations)
					_cRegion = null;	// (right-click on vocations does nothing structural; reserved)
				return;
			}

			switch(_ePhase) {
				case EVillagePhase.Region:		HandleRegionClick();	break;
				case EVillagePhase.Vocations:	HandleVocationClick();	break;
				case EVillagePhase.Weekly:		HandleWeeklyClick();	break;
				case EVillagePhase.Predation:	HandlePredationClick();	break;
				case EVillagePhase.GameOver:	BackToMenu();			break;
			}
		}

		void HandleRegionClick()
		{
			for(int iCount = 0; iCount < _caRegions.Count; ++iCount)
				if(_caRegions[iCount].tHotspot.Contains(_tMouse)) {
					ChooseRegion(_caRegions[iCount]);
					return;
				}
		}

		void HandleVocationClick()
		{
			for(int iCount = 0; iCount < _caAvail.Count; ++iCount)
				if(_caVocRects[iCount].Contains(_tMouse)) {
					ToggleVocation(_caAvail[iCount]);
					return;
				}

			if(_caChosen.Count == 2 && _tFoundRect.Contains(_tMouse))
				FoundVillage();
		}

		void HandleWeeklyClick()
		{
			if(_bConfirmWeek) {
				if(_tConfirmYesRect.Contains(_tMouse))		BeginNextWeek();
				else if(_tConfirmNoRect.Contains(_tMouse))	_bConfirmWeek = false;
				return;
			}

			if(_tNextWeekRect.Contains(_tMouse))		_bConfirmWeek = true;
			else if(_tForageRect.Contains(_tMouse))		Forage();
			else if(_tReplenishRect.Contains(_tMouse))	Replenish();
		}

		void HandlePredationClick()
		{
			if(_tDefendRect.Contains(_tMouse))
				Defend();
			else if(_tIgnoreRect.Contains(_tMouse)) {
				IgnoreOutcome(_cCurEvent);
				_cCurEvent = null;
				ProcessNextEvent();
			}
		}

		void InputSystem_KeyDown(object oSender, KeyEventArgs eKeyEvt)
		{
			// Escape bails out of the village layer (only reachable when this scene owns input -- a
			// pushed VillageRaid is on top during Resolving). Not mid-resolve.
			if(eKeyEvt.KeyCode == Keys.Escape && _ePhase != EVillagePhase.Resolving)
				BackToMenu();
		}

		void BackToMenu()
		{
			_eState = ESceneStates.Inactive;
			DataStore.cInstance.cSceneMgr.RemoveScene(this);
		}

	// draw helpers

		void DrawCover(Texture2D cTex)
		{
			float	fScale = Math.Max((float)_cGraphics.Viewport.Width / cTex.Width, (float)_cGraphics.Viewport.Height / cTex.Height);
			int		iW = (int)(cTex.Width * fScale),
					iH = (int)(cTex.Height * fScale);
			_cBatch.Draw(cTex, new Rectangle((_cGraphics.Viewport.Width - iW) / 2, (_cGraphics.Viewport.Height - iH) / 2, iW, iH), Color.White);
		}

		void Panel(Rectangle tRect, float fAlpha)
		{
			_cBatch.Draw(_cPixel, tRect, Color.Black * fAlpha);
		}

		void Button(Rectangle tRect, string sLabel, bool bEnabled, Color cBase)
		{
			bool	bHover = tRect.Contains(_tMouse);
			_cBatch.Draw(_cPixel, tRect, (bEnabled ? cBase : Color.DimGray) * (bHover && bEnabled ? .95f : .7f));
			Vector2	tSize = _cFont.MeasureString(sLabel);
			_cBatch.DrawString(_cFont, sLabel, new Vector2(tRect.X + (tRect.Width - tSize.X) / 2, tRect.Y + (tRect.Height - tSize.Y) / 2),
				bEnabled ? Color.White : Color.LightGray);
		}

	// draw: region pick

		void DrawRegionPick()
		{
			DrawCover(_cMapBg);
			Panel(new Rectangle(0, 0, _cGraphics.Viewport.Width, 40), .5f);
			_cBatch.DrawString(_cFont, "FOUND YOUR VILLAGE -- choose a region", new Vector2(16, 12), Color.White);

			for(int iCount = 0; iCount < _caRegions.Count; ++iCount) {
				Region	cReg = _caRegions[iCount];
				bool	bHover = iCount == _iHoverRegion;
				_cBatch.Draw(_cPixel, cReg.tHotspot, (bHover ? Color.SaddleBrown : Color.Black) * (bHover ? .9f : .6f));
				_cBatch.DrawString(_cFont, cReg.sName, new Vector2(cReg.tHotspot.X + 8, cReg.tHotspot.Y + 8), bHover ? Color.Gold : Color.White);
			}

			if(_iHoverRegion >= 0) {
				Region	cReg = _caRegions[_iHoverRegion];
				Panel(new Rectangle(0, _cGraphics.Viewport.Height - 52, _cGraphics.Viewport.Width, 52), .6f);
				_cBatch.DrawString(_cFont, cReg.sBlurb, new Vector2(16, _cGraphics.Viewport.Height - 44), Color.White);
				_cBatch.DrawString(_cFont, "Terrain: " + TerrainText(cReg.eTerrain), new Vector2(16, _cGraphics.Viewport.Height - 24), Color.LightGreen);
			}
		}

		static string TerrainText(ETerrain eTerrain)
		{
			if(eTerrain == ETerrain.None)
				return "open ground";
			List<string>	caParts = new List<string>();
			if((eTerrain & ETerrain.Pasture) != 0)		caParts.Add("pastureland");
			if((eTerrain & ETerrain.Fields) != 0)		caParts.Add("open fields");
			if((eTerrain & ETerrain.Woods) != 0)		caParts.Add("woods");
			if((eTerrain & ETerrain.Mountains) != 0)	caParts.Add("mountains");
			if((eTerrain & ETerrain.Water) != 0)		caParts.Add("water");
			return string.Join(", ", caParts);
		}

	// draw: vocation pick

		void DrawVocationPick()
		{
			DrawCover(_cVocBg);
			Panel(new Rectangle(0, 0, _cGraphics.Viewport.Width, 40), .5f);
			_cBatch.DrawString(_cFont, string.Format("{0} is known for... (pick 2)", _cRegion.sName), new Vector2(16, 12), Color.White);

			Panel(new Rectangle(24, 108, 330, _caAvail.Count * 34 + 12), .55f);
			for(int iCount = 0; iCount < _caAvail.Count; ++iCount) {
				EVocation	eVoc = _caAvail[iCount];
				VocationDef	cDef = VillageRules.Def(eVoc);
				bool		bChosen = _caChosen.Contains(eVoc);
				bool		bHover = iCount == _iHoverVoc;

				_cBatch.Draw(_cPixel, _caVocRects[iCount], (bChosen ? Color.DarkGreen : Color.Black) * (bHover ? .85f : .5f));
				string	sLine = cDef.IsFood ? string.Format("{0}  ({1}% food)", cDef.sName, cDef.iFoodPct) : cDef.sName;
				_cBatch.DrawString(_cFont, sLine, new Vector2(_caVocRects[iCount].X + 8, _caVocRects[iCount].Y + 7), bChosen ? Color.Gold : Color.White);
			}

			// detail panel on the right for the hovered vocation
			if(_iHoverVoc >= 0) {
				VocationDef	cDef = VillageRules.Def(_caAvail[_iHoverVoc]);
				Panel(new Rectangle(400, 120, 340, 150), .6f);
				_cBatch.DrawString(_cFont, cDef.sName, new Vector2(414, 130), Color.Gold);
				_cBatch.DrawString(_cFont, cDef.IsFood ? string.Format("Food: {0}% of need / week", cDef.iFoodPct) : "Food: none", new Vector2(414, 154), Color.White);
				_cBatch.DrawString(_cFont, cDef.IsFood ? string.Format("Raid chance: {0}%", cDef.iPredationPct) : "Raid chance: none", new Vector2(414, 176), cDef.iPredationPct > 0 ? Color.OrangeRed : Color.LightGray);
				_cBatch.DrawString(_cFont, "Market: " + cDef.sMarket, new Vector2(414, 198), Color.White);
				_cBatch.DrawString(_cFont, "Perk: " + cDef.sBonus, new Vector2(414, 220), Color.LightGreen);
			}

			_cBatch.DrawString(_cFont, string.Format("Chosen: {0} / 2", _caChosen.Count), new Vector2(24, _cGraphics.Viewport.Height - 30), Color.White);
			if(_caChosen.Count == 2)
				Button(_tFoundRect, "FOUND VILLAGE", true, Color.DarkGreen);
		}

	// draw: the village screen (weekly / predation / resolving / gameover all sit on this)

		void DrawVillage()
		{
			DrawCover(_cVillageBg);

			// a skyline band across the top as the title bar
			float	fBandScale = (float)_cGraphics.Viewport.Width / _cSkyline.Width;
			int		iBandH = (int)(72);
			_cBatch.Draw(_cSkyline, new Rectangle(0, 0, _cGraphics.Viewport.Width, iBandH), new Rectangle(0, 0, _cSkyline.Width, (int)(iBandH / fBandScale)), Color.White * .85f);
			Panel(new Rectangle(0, 0, _cGraphics.Viewport.Width, iBandH), .35f);
			_cBatch.DrawString(_cFont, string.Format("{0}   --   Week {1}", _sVillageName, _iWeek), new Vector2(16, 10), Color.White, 0, Vector2.Zero, 1.4f, SpriteEffects.None, 0);

			// stats rail (left)
			Panel(new Rectangle(12, 84, 250, 150), .55f);
			int	iPct = TotalProductionPct();
			_cBatch.DrawString(_cFont, string.Format("Population   {0}", _iPop), new Vector2(24, 94), Color.White);
			_cBatch.DrawString(_cFont, string.Format("Food store   {0}", (int)_fFood), new Vector2(24, 116), Color.White);
			_cBatch.DrawString(_cFont, string.Format("Coin         {0}", _iCoin), new Vector2(24, 138), Color.Gold);
			_cBatch.DrawString(_cFont, string.Format("Production   {0}%", iPct), new Vector2(24, 160), iPct >= 100 ? Color.LightGreen : Color.OrangeRed);
			_cBatch.DrawString(_cFont, iPct >= 100 ? string.Format("Food needs met (+{0}% to sell)", iPct - 100) : "SHORTFALL -- villagers will leave",
				new Vector2(24, 182), iPct >= 100 ? Color.LightGreen : Color.OrangeRed);
			_cBatch.DrawString(_cFont, string.Format("Market {0} {1}", _iLastPrice, _bPriceUp ? "^" : "v"), new Vector2(24, 204), _bPriceUp ? Color.LightGreen : Color.IndianRed);

			// vocations panel (left, lower)
			Panel(new Rectangle(12, 244, 250, 96), .55f);
			_cBatch.DrawString(_cFont, "Vocations", new Vector2(24, 250), Color.LightGray);
			int	iRow = 0;
			foreach(EVocation eVoc in _caChosen) {
				VocationDef	cDef = VillageRules.Def(eVoc);
				string	sLine = cDef.IsFood ? string.Format("{0}  {1}%", cDef.sName, _ciCurPct[eVoc]) : cDef.sName;
				_cBatch.DrawString(_cFont, sLine, new Vector2(24, 272 + iRow * 22), Color.White);
				iRow++;
			}

			// yield / status line
			Panel(new Rectangle(12, _cGraphics.Viewport.Height - 40, _cGraphics.Viewport.Width - 24, 32), .55f);
			_cBatch.DrawString(_cFont, _sYield, new Vector2(24, _cGraphics.Viewport.Height - 32), Color.White);

			// right-rail buttons only while actually taking the weekly turn
			if(_ePhase == EVillagePhase.Weekly && !_bConfirmWeek) {
				Button(_tReplenishRect, "REPLENISH HERDS", true, Color.SaddleBrown);
				Button(_tForageRect, _bForaged ? "FORAGED" : "FORAGE", !_bForaged, Color.DarkOliveGreen);
				Button(_tNextWeekRect, "NEXT WEEK", true, Color.DarkGreen);
			}

			if(_ePhase == EVillagePhase.Weekly && _bConfirmWeek) {
				Panel(new Rectangle(0, 0, _cGraphics.Viewport.Width, _cGraphics.Viewport.Height), .55f);
				_cBatch.DrawString(_cFont, "End the week?", new Vector2(455, 280), Color.White, 0, Vector2.Zero, 1.4f, SpriteEffects.None, 0);
				Button(_tConfirmYesRect, "YES", true, Color.DarkGreen);
				Button(_tConfirmNoRect, "NO", true, Color.DarkRed);
			}

			if(_ePhase == EVillagePhase.Resolving)
				_cBatch.DrawString(_cFont, "The muster marches to the fields...", new Vector2(400, 300), Color.Gold);
		}

		void DrawPredation()
		{
			Panel(new Rectangle(0, 0, _cGraphics.Viewport.Width, _cGraphics.Viewport.Height), .6f);
			VocationDef	cDef = VillageRules.Def(_cCurEvent.eVoc);

			_cBatch.DrawString(_cFont, "HOSTILE INTRUSION", new Vector2(400, 230), Color.OrangeRed, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
			_cBatch.DrawString(_cFont, string.Format("{0} is approaching your {1}.", _cCurEvent.sRaider, cDef.sName), new Vector2(330, 280), Color.White);
			_cBatch.DrawString(_cFont, "Defend and you protect production even in defeat; ignore it and the herd is ravaged.", new Vector2(230, 306), Color.LightGray);

			Button(_tDefendRect, "DEFEND", true, Color.DarkGreen);
			Button(_tIgnoreRect, "IGNORE", true, Color.DarkRed);
		}

		void DrawGameOver()
		{
			Panel(new Rectangle(0, 0, _cGraphics.Viewport.Width, _cGraphics.Viewport.Height), .72f);
			_cBatch.DrawString(_cFont, _bThrived ? string.Format("{0} THRIVES", _sVillageName) : string.Format("{0} IS LOST", _sVillageName),
				new Vector2(360, 250), _bThrived ? Color.Gold : Color.OrangeRed, 0, Vector2.Zero, 1.6f, SpriteEffects.None, 0);
			_cBatch.DrawString(_cFont, _bThrived ? string.Format("You held out {0} weeks. Population {1}, coin {2}.", _iWeek - 1, _iPop, _iCoin)
				: "Your people scattered to the wind.", new Vector2(340, 300), Color.White);
			_cBatch.DrawString(_cFont, "Click to return to the mode menu", new Vector2(370, 330), Color.LightGray);
		}
	}
}
