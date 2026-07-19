// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	//// ddhj: 2026 War Battle mode (docs/WAR_BATTLE_DESIGN.md). The strategic layer: divisions
	//// maneuver on a grid, and when a player division reaches an enemy one you commit a stance and
	//// the two armies clash for real on the combat engine (WarClash, pushed onto the scene stack).
	//// Destroy every enemy division to win. Spritefont + a 1x1 pixel; no new art. This scene owns
	//// no combat -- it hands each fight to WarClash and reads back the survivor count.
	public class WarBattle : IGameScene
	{
		enum EWarPhase	{ Maneuver, StanceSelect, Resolving, GameOver }

		ESceneStates	_eState;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		SpriteFont		_cFont;
		Cursor			_cCursor;
		Texture2D		_cPixel;

		List<Division>	_caDivs = new List<Division>();
		Division		_cSelected;
		EWarPhase		_ePhase = EWarPhase.Maneuver;

		// pending clash bookkeeping
		Division		_cPendPlayer,
						_cPendEnemy;
		WarClashResult	_cPendResult;
		bool			_bPlayerWon;

		Point			_tMouse;
		int				_iHoverCol = -1,
						_iHoverRow = -1;

		// stance-select buttons
		EWarStance[]	_eaStances = new EWarStance[] { EWarStance.Charge, EWarStance.Bow, EWarStance.Magic };
		Rectangle[]		_caStanceRects;

		// strategic grid geometry
		const int	iGridCols = 8,
					iGridRows = 5,
					iCellW = 90,
					iCellH = 80,
					iOriginX = 152,
					iOriginY = 120;

		const float	fAdvantageMult = 1.35f;	// the stance winner fields this much of a formation

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

				BuildArmies();

				// three stance buttons, centered, shown only during StanceSelect
				_caStanceRects = new Rectangle[_eaStances.Length];
				for(int iCount = 0; iCount < _eaStances.Length; ++iCount)
					_caStanceRects[iCount] = new Rectangle(340 + iCount * 130, 300, 110, 60);

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
			// a pushed WarClash freezes this scene; once it pops, this Update runs again and we pick
			// up the recorded result. Poll here rather than via any scene callback.
			if(_ePhase == EWarPhase.Resolving && _cPendResult != null && _cPendResult.bComplete)
				ResolveClash();
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Color.Black);

			_cBatch.Begin(); {
				DrawGrid();
				DrawDivisions();
				DrawHud();

				if(_ePhase == EWarPhase.StanceSelect)
					DrawStanceSelect();
				else if(_ePhase == EWarPhase.GameOver)
					DrawGameOver();

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

		void BuildArmies()
		{
			DataStore	cData = DataStore.cInstance;
			TemplateCfgMaster	cPlayerCfg = cData.cLSteward.cTemplates[0];
			//// cRSteward.cTemplates[0] is "RCap" -- Trooper.cs gives any "Cap"-named template a
			//// 1.4x render scale, which would make every enemy trooper oversized. Use [1] ("RT1"),
			//// a plain trooper, so both sides' formations render at the same normal scale.
			TemplateCfgMaster	cEnemyCfg = cData.cRSteward.cTemplates[1];

			_caDivs.Clear();

			// player divisions on the left flank
			_caDivs.Add(new Division { iSide = 0, iCol = 0, iRow = 1, iTroops = 45, eStance = EWarStance.Charge, sLabel = "1st Foot",  cCfg = cPlayerCfg, cColor = Color.CornflowerBlue });
			_caDivs.Add(new Division { iSide = 0, iCol = 0, iRow = 2, iTroops = 40, eStance = EWarStance.Bow,    sLabel = "2nd Foot",  cCfg = cPlayerCfg, cColor = Color.CornflowerBlue });
			_caDivs.Add(new Division { iSide = 0, iCol = 1, iRow = 3, iTroops = 35, eStance = EWarStance.Magic,  sLabel = "3rd Foot",  cCfg = cPlayerCfg, cColor = Color.CornflowerBlue });

			// enemy divisions on the right flank
			_caDivs.Add(new Division { iSide = 1, iCol = 7, iRow = 1, iTroops = 45, eStance = EWarStance.Bow,    sLabel = "Van",   cCfg = cEnemyCfg, cColor = Color.IndianRed });
			_caDivs.Add(new Division { iSide = 1, iCol = 7, iRow = 2, iTroops = 40, eStance = EWarStance.Magic,  sLabel = "Line",  cCfg = cEnemyCfg, cColor = Color.IndianRed });
			_caDivs.Add(new Division { iSide = 1, iCol = 6, iRow = 3, iTroops = 35, eStance = EWarStance.Charge, sLabel = "Rear",  cCfg = cEnemyCfg, cColor = Color.IndianRed });
		}

	// strategic turn logic

		// player moved a division: resolve contact, or let the enemy step and resolve again
		void EndPlayerAction()
		{
			if(TryTriggerClash())
				return;

			EnemyTurn();
			TryTriggerClash();
		}

		void EnemyTurn()
		{
			foreach(Division cEnemy in _caDivs) {
				if(cEnemy.iSide != 1)
					continue;

				Division	cTarget = NearestFoe(cEnemy);
				if(cTarget == null)
					continue;

				// already adjacent? hold position; the clash resolves in TryTriggerClash
				if(Chebyshev(cEnemy, cTarget) <= 1)
					continue;

				StepToward(cEnemy, cTarget);
			}
		}

		// scan every player/enemy pair; the first adjacency (or shared cell) starts a clash
		bool TryTriggerClash()
		{
			foreach(Division cPlayer in _caDivs) {
				if(cPlayer.iSide != 0)
					continue;

				foreach(Division cEnemy in _caDivs) {
					if(cEnemy.iSide != 1 || Chebyshev(cPlayer, cEnemy) > 1)
						continue;

					_cPendPlayer = cPlayer;
					_cPendEnemy = cEnemy;
					_cSelected = null;
					_ePhase = EWarPhase.StanceSelect;
					return true;
				}
			}

			return false;
		}

		// player committed a stance: bake the matchup advantage into spawn counts and push the clash
		void StartClash(EWarStance ePlayerStance)
		{
			int	iAdv = WarRules.Beats(ePlayerStance, _cPendEnemy.eStance);	// +1 player, -1 enemy, 0 tie

			WarClashPlan	cPlan = new WarClashPlan {
				cPlayerCfg = _cPendPlayer.cCfg,
				cEnemyCfg = _cPendEnemy.cCfg,
				sPlayerLabel = _cPendPlayer.sLabel,
				sEnemyLabel = _cPendEnemy.sLabel,
				ePlayerStance = ePlayerStance,
				eEnemyStance = _cPendEnemy.eStance,
				iPlayerSpawn = iAdv > 0 ? (int)(_cPendPlayer.iTroops * fAdvantageMult) : _cPendPlayer.iTroops,
				iEnemySpawn = iAdv < 0 ? (int)(_cPendEnemy.iTroops * fAdvantageMult) : _cPendEnemy.iTroops,
				iAdvantageSide = iAdv > 0 ? 0 : iAdv < 0 ? 1 : -1
			};

			_cPendResult = new WarClashResult();
			_ePhase = EWarPhase.Resolving;

			WarClash	cClash = new WarClash(cPlan, _cPendResult);
			if(cClash.Init())
				DataStore.cInstance.cSceneMgr.AddScene(cClash);
			else {
				// clash failed to initialize -- abandon it rather than hang in Resolving
				_cPendResult = null;
				_ePhase = EWarPhase.Maneuver;
			}
		}

		// control is back from the popped clash: apply the outcome to the strategic map
		void ResolveClash()
		{
			int	iSurvivors = Math.Max(1, _cPendResult.iSurvivors);

			if(_cPendResult.iWinnerSide == 0) {
				_caDivs.Remove(_cPendEnemy);
				_cPendPlayer.iTroops = iSurvivors;
			} else {
				_caDivs.Remove(_cPendPlayer);
				_cPendEnemy.iTroops = iSurvivors;
			}

			_cPendPlayer = _cPendEnemy = null;
			_cPendResult = null;

			// win/lose: whoever has no divisions left
			bool	bEnemyLeft = false,
					bPlayerLeft = false;
			foreach(Division cDiv in _caDivs) {
				if(cDiv.iSide == 0) bPlayerLeft = true;
				else				bEnemyLeft = true;
			}

			if(!bEnemyLeft || !bPlayerLeft) {
				_bPlayerWon = !bEnemyLeft;
				_ePhase = EWarPhase.GameOver;
			} else	_ePhase = EWarPhase.Maneuver;
		}

	// grid helpers

		Division DivisionAt(int iCol, int iRow)
		{
			foreach(Division cDiv in _caDivs)
				if(cDiv.iCol == iCol && cDiv.iRow == iRow)
					return cDiv;
			return null;
		}

		static int Chebyshev(Division cA, Division cB)
		{
			return Math.Max(Math.Abs(cA.iCol - cB.iCol), Math.Abs(cA.iRow - cB.iRow));
		}

		// step one cell (8-directional) toward the target, into an empty cell that reduces distance
		void StepToward(Division cMover, Division cTarget)
		{
			int	iStepCol = cMover.iCol + Math.Sign(cTarget.iCol - cMover.iCol),
				iStepRow = cMover.iRow + Math.Sign(cTarget.iRow - cMover.iRow);

			// prefer the full diagonal/orthogonal step; fall back to axis-only if it's blocked
			if(TryMove(cMover, iStepCol, iStepRow))			return;
			if(TryMove(cMover, iStepCol, cMover.iRow))		return;
			if(TryMove(cMover, cMover.iCol, iStepRow))		return;
		}

		bool TryMove(Division cMover, int iCol, int iRow)
		{
			if(iCol < 0 || iRow < 0 || iCol >= iGridCols || iRow >= iGridRows)
				return false;
			if(iCol == cMover.iCol && iRow == cMover.iRow)
				return false;
			if(DivisionAt(iCol, iRow) != null)		// no stacking
				return false;

			cMover.iCol = iCol;
			cMover.iRow = iRow;
			return true;
		}

		Division NearestFoe(Division cFrom)
		{
			Division	cBest = null;
			int			iBest = int.MaxValue;
			int			iWantSide = cFrom.iSide == 0 ? 1 : 0;

			foreach(Division cDiv in _caDivs) {
				if(cDiv.iSide != iWantSide)
					continue;

				int	iDist = Chebyshev(cFrom, cDiv);
				if(iDist < iBest) {
					iBest = iDist;
					cBest = cDiv;
				}
			}

			return cBest;
		}

		Rectangle CellRect(int iCol, int iRow)
		{
			return new Rectangle(iOriginX + iCol * iCellW, iOriginY + iRow * iCellH, iCellW, iCellH);
		}

		bool CellFromPoint(Point tPt, out int iCol, out int iRow)
		{
			iCol = (tPt.X - iOriginX) / iCellW;
			iRow = (tPt.Y - iOriginY) / iCellH;
			return tPt.X >= iOriginX && tPt.Y >= iOriginY && iCol < iGridCols && iRow < iGridRows;
		}

	// input

		void InputSystem_MouseMove(object oSender, MouseEventArgs eMouseEvt)
		{
			_tMouse = eMouseEvt.Location;
			_cCursor.Update(eMouseEvt.Location);

			if(!CellFromPoint(_tMouse, out _iHoverCol, out _iHoverRow)) {
				_iHoverCol = _iHoverRow = -1;
			}
		}

		void InputSystem_MouseDown(object oSender, MouseEventArgs eMouseEvt)
		{
			switch(_ePhase) {
				case EWarPhase.Maneuver:	HandleManeuverClick(eMouseEvt);	break;
				case EWarPhase.StanceSelect:	HandleStanceClick(eMouseEvt);	break;
				case EWarPhase.GameOver:	BackToMenu();	break;
			}
		}

		void HandleManeuverClick(MouseEventArgs eMouseEvt)
		{
			if(eMouseEvt.Button == MouseButton.Right) {
				_cSelected = null;
				return;
			}

			int	iCol, iRow;
			if(!CellFromPoint(eMouseEvt.Location, out iCol, out iRow))
				return;

			Division	cHit = DivisionAt(iCol, iRow);

			// clicking one of your divisions selects it
			if(cHit != null && cHit.iSide == 0) {
				_cSelected = cHit;
				return;
			}

			// with a division selected, click an in-range empty cell to advance it (one cell,
			// 8-directional). Moving ends the player's action -> enemy steps -> contact resolves.
			if(_cSelected != null && cHit == null
				&& Math.Max(Math.Abs(iCol - _cSelected.iCol), Math.Abs(iRow - _cSelected.iRow)) == 1) {
				if(TryMove(_cSelected, iCol, iRow))
					EndPlayerAction();
			}
		}

		void InputSystem_KeyDown(object oSender, KeyEventArgs eKeyEvt)
		{
			// Escape bails out of the strategic layer (only reachable when this scene has input --
			// during a clash WarClash is on top of the stack and owns Escape). Not while Resolving.
			if(eKeyEvt.KeyCode == Keys.Escape && _ePhase != EWarPhase.Resolving)
				BackToMenu();
		}

		void HandleStanceClick(MouseEventArgs eMouseEvt)
		{
			if(eMouseEvt.Button != MouseButton.Left)
				return;

			for(int iCount = 0; iCount < _caStanceRects.Length; ++iCount)
				if(_caStanceRects[iCount].Contains(eMouseEvt.Location)) {
					StartClash(_eaStances[iCount]);
					return;
				}
		}

		void BackToMenu()
		{
			_eState = ESceneStates.Inactive;
			DataStore.cInstance.cSceneMgr.RemoveScene(this);
		}

	// draw

		void DrawGrid()
		{
			// grid backdrop
			_cBatch.Draw(_cPixel, new Rectangle(iOriginX, iOriginY, iGridCols * iCellW, iGridRows * iCellH), Color.DarkOliveGreen * .35f);

			// cell lines
			for(int iCol = 0; iCol <= iGridCols; ++iCol)
				_cBatch.Draw(_cPixel, new Rectangle(iOriginX + iCol * iCellW, iOriginY, 1, iGridRows * iCellH), Color.Gray * .5f);
			for(int iRow = 0; iRow <= iGridRows; ++iRow)
				_cBatch.Draw(_cPixel, new Rectangle(iOriginX, iOriginY + iRow * iCellH, iGridCols * iCellW, 1), Color.Gray * .5f);

			// hover highlight (only useful while maneuvering)
			if(_ePhase == EWarPhase.Maneuver && _iHoverCol >= 0)
				_cBatch.Draw(_cPixel, CellRect(_iHoverCol, _iHoverRow), Color.White * .12f);

			// selected division's reachable cells
			if(_ePhase == EWarPhase.Maneuver && _cSelected != null)
				for(int iCol = _cSelected.iCol - 1; iCol <= _cSelected.iCol + 1; ++iCol)
					for(int iRow = _cSelected.iRow - 1; iRow <= _cSelected.iRow + 1; ++iRow)
						if(iCol >= 0 && iRow >= 0 && iCol < iGridCols && iRow < iGridRows
							&& !(iCol == _cSelected.iCol && iRow == _cSelected.iRow) && DivisionAt(iCol, iRow) == null)
							_cBatch.Draw(_cPixel, CellRect(iCol, iRow), Color.LightGreen * .18f);
		}

		void DrawDivisions()
		{
			foreach(Division cDiv in _caDivs) {
				Rectangle	tCell = CellRect(cDiv.iCol, cDiv.iRow);
				Rectangle	tToken = new Rectangle(tCell.X + 8, tCell.Y + 8, tCell.Width - 16, tCell.Height - 16);

				bool	bSel = cDiv == _cSelected;
				_cBatch.Draw(_cPixel, tToken, cDiv.cColor * (bSel ? .95f : .7f));

				// selection outline
				if(bSel) {
					_cBatch.Draw(_cPixel, new Rectangle(tToken.X, tToken.Y, tToken.Width, 2), Color.Gold);
					_cBatch.Draw(_cPixel, new Rectangle(tToken.X, tToken.Bottom - 2, tToken.Width, 2), Color.Gold);
					_cBatch.Draw(_cPixel, new Rectangle(tToken.X, tToken.Y, 2, tToken.Height), Color.Gold);
					_cBatch.Draw(_cPixel, new Rectangle(tToken.Right - 2, tToken.Y, 2, tToken.Height), Color.Gold);
				}

				_cBatch.DrawString(_cFont, cDiv.sLabel, new Vector2(tToken.X + 4, tToken.Y + 4), Color.White);
				_cBatch.DrawString(_cFont, string.Format("{0} [{1}]", cDiv.iTroops, WarRules.Name(cDiv.eStance)[0]),
					new Vector2(tToken.X + 4, tToken.Y + 26), Color.White);
			}
		}

		void DrawHud()
		{
			_cBatch.DrawString(_cFont, "WAR BATTLE", new Vector2(iOriginX, 60), Color.LightGray);

			string	sHint;
			switch(_ePhase) {
				case EWarPhase.Maneuver:
					sHint = _cSelected == null ? "Click a blue division to select it"
						: "Click a highlighted cell to advance   (right-click: deselect)";
				break;
				case EWarPhase.StanceSelect:	sHint = "Choose your stance for the clash";	break;
				case EWarPhase.Resolving:		sHint = "The armies clash...";	break;
				default:						sHint = "";	break;
			}
			_cBatch.DrawString(_cFont, sHint, new Vector2(iOriginX, 88), Color.Gold);

			_cBatch.DrawString(_cFont, "Charge > Bow > Magic > Charge", new Vector2(iOriginX + 430, 60), Color.DarkGray);
		}

		void DrawStanceSelect()
		{
			_cBatch.Draw(_cPixel, new Rectangle(0, 0, _cGraphics.Viewport.Width, _cGraphics.Viewport.Height), Color.Black * .55f);

			_cBatch.DrawString(_cFont, string.Format("{0}  vs  {1}", _cPendPlayer.sLabel, _cPendEnemy.sLabel),
				new Vector2(360, 220), Color.White);
			_cBatch.DrawString(_cFont, string.Format("Enemy stance: {0}", WarRules.Name(_cPendEnemy.eStance)),
				new Vector2(360, 250), Color.IndianRed);

			for(int iCount = 0; iCount < _caStanceRects.Length; ++iCount) {
				Rectangle	tRect = _caStanceRects[iCount];
				bool		bHover = tRect.Contains(_tMouse);
				bool		bWins = WarRules.Beats(_eaStances[iCount], _cPendEnemy.eStance) > 0;

				_cBatch.Draw(_cPixel, tRect, (bWins ? Color.DarkGreen : Color.DimGray) * (bHover ? .95f : .7f));
				_cBatch.DrawString(_cFont, WarRules.Name(_eaStances[iCount]), new Vector2(tRect.X + 20, tRect.Y + 10), Color.White);
				_cBatch.DrawString(_cFont, "beats " + WarRules.Beats(_eaStances[iCount]), new Vector2(tRect.X + 8, tRect.Y + 34), Color.LightGray);
			}
		}

		void DrawGameOver()
		{
			_cBatch.Draw(_cPixel, new Rectangle(0, 0, _cGraphics.Viewport.Width, _cGraphics.Viewport.Height), Color.Black * .7f);
			_cBatch.DrawString(_cFont, _bPlayerWon ? "THE ENEMY ARMY IS BROKEN -- VICTORY" : "YOUR ARMY IS SHATTERED -- DEFEAT",
				new Vector2(330, 250), _bPlayerWon ? Color.Gold : Color.OrangeRed);
			_cBatch.DrawString(_cFont, "Click to return to the mode menu", new Vector2(360, 290), Color.LightGray);
		}
	}
}
