using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class MainMenu : IGameScene
	{
		ESceneStates	_eState;
		Texture2D		_cTrooperTex;
		AnimationData	_cTroopers;
		SpriteBatch		_cBatch;
		GraphicsDevice	_cGraphics;
		ClickableSprite	_cCursor,
						_cP1CapGuy,
 						_cP1T1Guy,
						_cP1T2Guy,
						_cP2CapGuy,
						_cP2T1Guy,
						_cP2T2Guy,
						_cEditedGuy,
						_cP1CaptainCard,
 						_cP1Template1,
						_cP1Template2,
						_cP2CaptainCard,
						_cP2Template1,
						_cP2Template2,
						_cToBattle;
		TemplateConfig	_cLCap,
						_cLT1, 
						_cLT2, 						
						_cRCap, 
						_cRT1, 
						_cRT2,
						_cEditedTemplate;
		bool			_bProcessPress = true,
						_bInTemplate = false;
		NumericUpDown	_cP1CapCount, 
						_cP1T1Count,
						_cP1T2Count,
						_cP2CapCount,
						_cP2T1Count,
						_cP2T2Count;
		int				_iLCount, 
						_iRCount;
		Song			_cMusic;

		Dictionary<string, bool>	 _cTakenColors = new Dictionary<string,bool>();
		
		#region IGameScene Members

		public ESceneStates eState		{ get { return _eState; } set { _eState = value; }}

		public void Update(GameTime cTime)
		{
			MouseState	cState = Mouse.GetState();
			Microsoft.Xna.Framework.Point		tPoint = new Microsoft.Xna.Framework.Point(cState.X, cState.Y);

			// set the position of the cursor
			_cCursor.tPos = new Vector2(cState.X, cState.Y);

			if(cState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { 
				if(_bProcessPress) { 
					if(((IClickable)_cToBattle).ContainsPoint(tPoint)) { 
						IGameScene nBattleGround = new BattleGround_Basic();
						// set the counts for the stewards
						_cLCap.iCount = (int)_cP1CapCount.Value;
						_cLT1.iCount = (int)_cP1T1Count.Value;
						_cLT2.iCount = (int)_cP1T2Count.Value;
						_cRCap.iCount = (int)_cP2CapCount.Value;
						_cRT1.iCount = (int)_cP2T1Count.Value;
						_cRT2.iCount = (int)_cP2T2Count.Value;
						if(nBattleGround.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nBattleGround);
						}
					} else if(((IClickable)_cP1CaptainCard).ContainsPoint(tPoint)) { 
						_cEditedTemplate = _cLCap;
						_cEditedGuy = _cP1CapGuy;
						_cTakenColors.Remove(_cEditedTemplate.sColor);
						IGameScene nTemplate = new Template(_cLCap, _cTakenColors);
						if(nTemplate.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nTemplate);
						}

						_bInTemplate = true;
					} else if(((IClickable)_cP1Template1).ContainsPoint(tPoint)) { 
						_cEditedTemplate = _cLT1;
						_cEditedGuy = _cP1T1Guy;
						_cTakenColors.Remove(_cEditedTemplate.sColor);
						IGameScene nTemplate = new Template(_cLT1, _cTakenColors);
						if(nTemplate.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nTemplate);
						}

						_bInTemplate = true;
					} else if(((IClickable)_cP1Template2).ContainsPoint(tPoint)) { 
						_cEditedTemplate = _cLT2;
						_cEditedGuy = _cP1T2Guy;
						_cTakenColors.Remove(_cEditedTemplate.sColor);
						IGameScene nTemplate = new Template(_cLT2, _cTakenColors);
						if(nTemplate.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nTemplate);
						}

						_bInTemplate = true;
					} else if(((IClickable)_cP2CaptainCard).ContainsPoint(tPoint)) { 
						_cEditedTemplate = _cRCap;
						_cEditedGuy = _cP2CapGuy;
						_cTakenColors.Remove(_cEditedTemplate.sColor);
						IGameScene nTemplate = new Template(_cRCap, _cTakenColors);
						if(nTemplate.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nTemplate);
						}
						_bInTemplate = true;
					} else if(((IClickable)_cP2Template1).ContainsPoint(tPoint)) { 
						_cEditedTemplate = _cRT1;
						_cEditedGuy = _cP2T1Guy;
						_cTakenColors.Remove(_cEditedTemplate.sColor);
						IGameScene nTemplate = new Template(_cRT1, _cTakenColors);
						if(nTemplate.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nTemplate);
						}
						_bInTemplate = true;
					} else if(((IClickable)_cP2Template2).ContainsPoint(tPoint)) { 
						_cEditedTemplate = _cRT2;
						_cEditedGuy = _cP2T2Guy;
						_cTakenColors.Remove(_cEditedTemplate.sColor);
						IGameScene nTemplate = new Template(_cRT2, _cTakenColors);
						if(nTemplate.Init()) { 
							DataStore.cInstance.cSceneMgr.AddScene(nTemplate);
						}
						_bInTemplate = true;
					}

					_bProcessPress = false;
				} 
			} else _bProcessPress = true;	
		}

		public void Draw(GameTime cTime)
		{
			_cGraphics.Clear(Microsoft.Xna.Framework.Color.Black);

			_cBatch.Begin(); { 
				// draw the cards and the to battle items
				_cBatch.Draw(_cP1CaptainCard.cTexRef, _cP1CaptainCard.tPos, _cP1CaptainCard.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
				_cBatch.Draw(_cP1Template1.cTexRef, _cP1Template1.tPos, _cP1Template1.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
				_cBatch.Draw(_cP1Template2.cTexRef, _cP1Template2.tPos, _cP1Template2.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
				_cBatch.Draw(_cP2CaptainCard.cTexRef, _cP2CaptainCard.tPos, _cP2CaptainCard.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
				_cBatch.Draw(_cP2Template1.cTexRef, _cP2Template1.tPos, _cP2Template1.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
				_cBatch.Draw(_cP2Template2.cTexRef, _cP2Template2.tPos, _cP2Template2.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
				_cBatch.Draw(_cToBattle.cTexRef, _cToBattle.tPos, _cToBattle.cFrame.tRect, Microsoft.Xna.Framework.Color.White);

				// the troopers and their colors and all that will have to pay attention to the rotation and all of that 
				// troopers and or anything off a spritesheet might need their own draw method
				_cBatch.Draw(_cTrooperTex, _cP1CapGuy.tPos, _cP1CapGuy.cFrame.tRect, Microsoft.Xna.Framework.Color.White, 
					_cP1CapGuy.cFrame.bRot ? -(float)Math.PI/2 : 0, _cP1CapGuy.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
				_cBatch.Draw(_cTrooperTex, _cP1T1Guy.tPos, _cP1T1Guy.cFrame.tRect, Microsoft.Xna.Framework.Color.White, 
					_cP1T1Guy.cFrame.bRot ? -(float)Math.PI/2 : 0, _cP1T1Guy.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
				_cBatch.Draw(_cTrooperTex, _cP1T2Guy.tPos, _cP1T2Guy.cFrame.tRect, Microsoft.Xna.Framework.Color.White, 
					_cP1T2Guy.cFrame.bRot ? -(float)Math.PI/2 : 0, _cP1T2Guy.cFrame.tTopLeft, 1, SpriteEffects.None, 0);

				_cBatch.Draw(_cTrooperTex, _cP2CapGuy.tPos, _cP2CapGuy.cFrame.tRect, Microsoft.Xna.Framework.Color.White, 
					_cP2CapGuy.cFrame.bRot ? -(float)Math.PI/2 : 0, _cP2CapGuy.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
				_cBatch.Draw(_cTrooperTex, _cP2T1Guy.tPos, _cP2T1Guy.cFrame.tRect, Microsoft.Xna.Framework.Color.White, 
					_cP2T1Guy.cFrame.bRot ? -(float)Math.PI/2 : 0, _cP2T1Guy.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
				_cBatch.Draw(_cTrooperTex, _cP2T2Guy.tPos, _cP2T2Guy.cFrame.tRect, Microsoft.Xna.Framework.Color.White, 
					_cP2T2Guy.cFrame.bRot ? -(float)Math.PI/2 : 0, _cP2T2Guy.cFrame.tTopLeft, 1, SpriteEffects.None, 0);
			
				// draw cursor
				_cBatch.Draw(_cCursor.cTexRef, _cCursor.tPos, _cCursor.cFrame.tRect, Microsoft.Xna.Framework.Color.White);
			} _cBatch.End();
		}

		public bool Init()
		{
			try { 
				ContentManager cContent = DataStore.cInstance.cContent;
				Random	cRand = DataStore.cInstance.cRand;

				_cGraphics = DataStore.cInstance.cGraphics;

				// since the placement of the card and the battle are relative to eachother and the veiwport 
				// just grab the texture first
				Texture2D	cCard1 = cContent.Load<Texture2D>(@"Out Game\Main Menu\green_card"), 
							cCard2 = cContent.Load<Texture2D>(@"Out Game\Main Menu\red_card"), 
							cBattle = cContent.Load<Texture2D>(@"Out Game\Main Menu\to_battle!-1");

				_cLCap = DataStore.cInstance.cLSteward.cTemplates[0];
				_cLT1 = DataStore.cInstance.cLSteward.cTemplates[1]; 
				_cLT2 = DataStore.cInstance.cLSteward.cTemplates[2]; 

				_cRCap = DataStore.cInstance.cRSteward.cTemplates[0];
				_cRT1 = DataStore.cInstance.cRSteward.cTemplates[1]; 
				_cRT2 = DataStore.cInstance.cRSteward.cTemplates[2]; 

				// build the inital taken colors
				_cTakenColors.Add(_cLCap.sColor, true);
				_cTakenColors.Add(_cLT1.sColor, true);
				_cTakenColors.Add(_cLT2.sColor, true);
				_cTakenColors.Add(_cRCap.sColor, true);
				_cTakenColors.Add(_cRT1.sColor, true);
				_cTakenColors.Add(_cRT2.sColor, true);

				_cMusic = ObjectCreationManager.cInstance.CreateMusic( "Final_Fantasy_4_Submission_OC_ReMix" );

				_cCursor = new ClickableSprite();
				_cCursor.cTexRef = cContent.Load<Texture2D>(@"Shared\arrow_cursor");
				_cCursor.tPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				_cCursor.cFrame = new Frame(_cCursor.cTexRef.Bounds, new Vector2(_cCursor.cTexRef.Bounds.Width / 2, _cCursor.cTexRef.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(_cCursor.cTexRef.Bounds.Width, _cCursor.cTexRef.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);

//// left cards
				_cP1CaptainCard = new ClickableSprite();
				_cP1CaptainCard.cTexRef = cCard1;
				_cP1CaptainCard.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP1CaptainCard.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - cBattle.Bounds.Width, 200);
				_cP1Template1 = new ClickableSprite();
				_cP1Template1.cTexRef = cCard1;
				_cP1Template1.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP1Template1.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - cBattle.Bounds.Width, 300);
				_cP1Template2 = new ClickableSprite();
				_cP1Template2.cTexRef = cCard1;
				_cP1Template2.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP1Template2.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - cBattle.Bounds.Width, 400);

//// right cards
				_cP2CaptainCard = new ClickableSprite();
				_cP2CaptainCard.cTexRef = cCard1;
				_cP2CaptainCard.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP2CaptainCard.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + cBattle.Bounds.Width - cCard2.Bounds.Width, 200);
				_cP2Template1 = new ClickableSprite();
				_cP2Template1.cTexRef = cCard1;
				_cP2Template1.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP2Template1.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + cBattle.Bounds.Width - cCard2.Bounds.Width, 300);
				_cP2Template2 = new ClickableSprite();
				_cP2Template2.cTexRef = cCard1;
				_cP2Template2.cFrame = new Frame(cCard1.Bounds, new Vector2(cCard1.Bounds.Width / 2, cCard1.Bounds.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cCard1.Bounds.Width, cCard1.Bounds.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cP2Template2.tPos = new Vector2(_cGraphics.Viewport.Width / 2 + cBattle.Bounds.Width - cCard2.Bounds.Width, 400);

				_cToBattle = new ClickableSprite();
				_cToBattle.cTexRef = cBattle;
				_cToBattle.cFrame = new Frame(cBattle.Bounds, new Vector2(cBattle.Bounds.Width / 2, cBattle.Height / 2), 
					new Vector2(0, 0), new Vector2(0, 0), new Vector2(cBattle.Width, cBattle.Height), new Vector2(0, 0), new Vector2(0, 0), null, false, false);
				_cToBattle.tPos = new Vector2(_cGraphics.Viewport.Width / 2 - (cBattle.Bounds.Width / 2), 
					300 + cCard1.Bounds.Height / 2 - (cBattle.Bounds.Height / 2));

				_cTrooperTex = cContent.Load<Texture2D>(@"Sprite Data\Troopers\Halberd\Front Facing\front");
				_cTroopers = cContent.Load<AnimationData>(@"Sprite Data\Troopers\Halberd\Front Facing\frontarray");

//// guys in the cards
				_cP1CapGuy = new ClickableSprite();
				_cP1CapGuy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][_cLCap.sColor.Substring(_cLCap.sColor.LastIndexOf("\\") + 1)].iStartIndex];
				_cP1CapGuy.cTexRef = _cTrooperTex;
				_cP1CapGuy.tPos = new Vector2(_cP1CaptainCard.tPos.X - _cP1CapGuy.cTexRef.Width / 4 + 5, _cP1CaptainCard.tPos.Y - 10);

				_cP1T1Guy = new ClickableSprite();
				_cP1T1Guy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][_cLT1.sColor.Substring(_cLT1.sColor.LastIndexOf("\\") + 1)].iStartIndex];
				_cP1T1Guy.cTexRef = _cTrooperTex;
				_cP1T1Guy.tPos = new Vector2(_cP1Template1.tPos.X - _cP1T1Guy.cTexRef.Width / 4 + 5, _cP1Template1.tPos.Y - 10);
				_cP1T2Guy = new ClickableSprite();
				_cP1T2Guy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][_cLT2.sColor.Substring(_cLT2.sColor.LastIndexOf("\\") + 1)].iStartIndex];
				_cP1T2Guy.cTexRef = _cTrooperTex;
				_cP1T2Guy.tPos = new Vector2(_cP1Template2.tPos.X - _cP1T2Guy.cTexRef.Width / 4 + 5, _cP1Template2.tPos.Y - 10);

				_cP2CapGuy = new ClickableSprite();
				_cP2CapGuy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][_cRCap.sColor.Substring(_cRCap.sColor.LastIndexOf("\\") + 1)].iStartIndex];
				_cP2CapGuy.cTexRef = _cTrooperTex;
				_cP2CapGuy.tPos = new Vector2(_cP2CaptainCard.tPos.X - _cP2CapGuy.cTexRef.Width / 4 + 5, _cP2CaptainCard.tPos.Y - 10);
				_cP2T1Guy = new ClickableSprite();
				_cP2T1Guy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][_cRT1.sColor.Substring(_cRT1.sColor.LastIndexOf("\\") + 1)].iStartIndex];
				_cP2T1Guy.cTexRef = _cTrooperTex;
				_cP2T1Guy.tPos = new Vector2(_cP2Template1.tPos.X - _cP2T1Guy.cTexRef.Width / 4 + 5, _cP2Template1.tPos.Y - 10);
				_cP2T2Guy = new ClickableSprite();
				_cP2T2Guy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][_cRT2.sColor.Substring(_cRT2.sColor.LastIndexOf("\\") + 1)].iStartIndex];
				_cP2T2Guy.cTexRef = _cTrooperTex;
				_cP2T2Guy.tPos = new Vector2(_cP2Template2.tPos.X - _cP2T2Guy.cTexRef.Width / 4 + 5, _cP2Template2.tPos.Y - 10);
				
				_cP1CapCount = new NumericUpDown();
				_cP1CapCount.Size = new System.Drawing.Size(58, 20);
				_cP1CapCount.Location = new System.Drawing.Point((int)_cP1CaptainCard.tPos.X - 65, (int)_cP1CaptainCard.tPos.Y + 20);
				_cP1CapCount.Minimum = 1;
				_cP1CapCount.Maximum = 10;
				_cP1CapCount.Value = 10;//cRand.Next(100);
				_cP1CapCount.ValueChanged += new EventHandler(Count_ValueChanged);
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cP1CapCount);
				_cP1T1Count = new NumericUpDown();
				_cP1T1Count.Size = new System.Drawing.Size(58, 20);
				_cP1T1Count.Location = new System.Drawing.Point((int)_cP1Template1.tPos.X - 65, (int)_cP1Template1.tPos.Y + 20);
				_cP1T1Count.Minimum = 1;
				_cP1T1Count.Maximum = 100;
				_cP1T1Count.Value = 45;//cRand.Next(100);
				_cP1T1Count.ValueChanged += new EventHandler(Count_ValueChanged);
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cP1T1Count);
				_cP1T2Count = new NumericUpDown();
				_cP1T2Count.Size = new System.Drawing.Size(58, 20);
				_cP1T2Count.Location = new System.Drawing.Point((int)_cP1Template2.tPos.X - 65, (int)_cP1Template2.tPos.Y + 20);
				_cP1T2Count.Minimum = 1;
				_cP1T2Count.Maximum = 100;
				_cP1T2Count.Value = 45;//cRand.Next(100);
				_cP1T2Count.ValueChanged += new EventHandler(Count_ValueChanged);
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cP1T2Count);
				
				_cP2CapCount = new NumericUpDown();
				_cP2CapCount.Size = new System.Drawing.Size(58, 20);
				_cP2CapCount.Location = new System.Drawing.Point((int)_cP2CaptainCard.tPos.X + 50, (int)_cP2CaptainCard.tPos.Y + 20);
				_cP2CapCount.Minimum = 1;
				_cP2CapCount.Maximum = 10;
				_cP2CapCount.Value = 10;//cRand.Next(100);
				_cP2CapCount.ValueChanged += new EventHandler(Count_ValueChanged);
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cP2CapCount);
				_cP2T1Count = new NumericUpDown();
				_cP2T1Count.Size = new System.Drawing.Size(58, 20);
				_cP2T1Count.Location = new System.Drawing.Point((int)_cP2Template1.tPos.X + 50, (int)_cP2Template1.tPos.Y + 20);
				_cP2T1Count.Minimum = 1;
				_cP2T1Count.Maximum = 100;
				_cP2T1Count.Value = 45;//cRand.Next(100);
				_cP2T1Count.ValueChanged += new EventHandler(Count_ValueChanged);
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cP2T1Count);
				_cP2T2Count = new NumericUpDown();
				_cP2T2Count.Size = new System.Drawing.Size(58, 20);
				_cP2T2Count.Location = new System.Drawing.Point((int)_cP2Template2.tPos.X + 50, (int)_cP2Template2.tPos.Y + 20);
				_cP2T2Count.Minimum = 1;
				_cP2T2Count.Maximum = 100;
				_cP2T2Count.Value = 45;//cRand.Next(100);
				_cP2T2Count.ValueChanged += new EventHandler(Count_ValueChanged);
				Control.FromHandle(DataStore.cInstance.cGame.Window.Handle).Controls.Add(_cP2T2Count);

				_cBatch = new SpriteBatch(DataStore.cInstance.cGraphics);

				MediaPlayer.Play( _cMusic );
				return true;
			} catch(Exception xEx) {
				System.Windows.Forms.MessageBox.Show(xEx.ToString());
				return false;
			}
		}

		bool CheckLeftCount()
		{
			return _cP1CapCount.Value + _cP1T1Count.Value + _cP1T2Count.Value <= 100;
		}

		bool CheckRightCount()
		{
			return _cP2CapCount.Value + _cP2T1Count.Value + _cP2T2Count.Value <= 100;
		}

		void Count_ValueChanged(object sender, EventArgs e)
		{
			bool	bNotValid;
			if(sender == _cP1CapCount || sender == _cP1T1Count || sender == _cP1T2Count) 
				bNotValid = !CheckLeftCount();
			else bNotValid = !CheckRightCount();
				
			if(bNotValid) { 
				System.Windows.Forms.MessageBox.Show("There can only be 100 per army");
				--((NumericUpDown)sender).Value;
			}
		}

		public void Unload()
		{
			_cP1CaptainCard.Dispose();
			_cP2CaptainCard.Dispose();
			_cToBattle.Dispose();
			_cTrooperTex.Dispose();
			_cTroopers.Dispose();
			MediaPlayer.Stop( );
		}

		public void ToggleControls()
		{
			_cP1CapCount.Visible = _cP1T1Count.Visible = _cP1T2Count.Visible = _cP2CapCount.Visible = 
				_cP2T1Count.Visible = _cP2T2Count.Visible = _eState == ESceneStates.Active;
 
			if(_eState == ESceneStates.Active) { 
				// check to see if we were in a template
				if(_bInTemplate) { 
					// set the front guy with the new color from the template
					//// ddhj: since zombie is not really a color we have to change it to blood zombie for the guys lookup
					string sColor = _cEditedTemplate.sColor.Substring(_cEditedTemplate.sColor.LastIndexOf("\\") + 1);
					_cEditedGuy.cFrame = _cTroopers.caFrameData[_cTroopers.cReferenceList["Main"]["Sub"][sColor].iStartIndex];

					// rebuild the taken colors
					_cTakenColors.Clear();
					_cTakenColors.Add(_cLCap.sColor, true);
					_cTakenColors.Add(_cLT1.sColor, true);
					_cTakenColors.Add(_cLT2.sColor, true);
					_cTakenColors.Add(_cRCap.sColor, true);
					_cTakenColors.Add(_cRT1.sColor, true);
					_cTakenColors.Add(_cRT2.sColor, true);
				}
			}
		}

		#endregion
	}
}
