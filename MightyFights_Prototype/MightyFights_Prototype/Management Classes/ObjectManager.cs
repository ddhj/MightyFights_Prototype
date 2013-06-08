// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Renderers;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype
{
	public class ObjectCreationManager
	{
		#region Singleton Implementation
		
		private static readonly ObjectCreationManager _cInstance = new ObjectCreationManager();
		public static ObjectCreationManager cInstance	{ get { return _cInstance; }} 
		private ObjectCreationManager() {}
		
		#endregion

		Dictionary<string, AnimationData>	_cAnimationDataList = new Dictionary<string,AnimationData>();
		Dictionary<string, Texture2D>		_cTextureList = new Dictionary<string,Texture2D>();
		Dictionary<string, ParticleEffect>	_cParticles = new Dictionary<string,ParticleEffect>( );
		Dictionary<string, SoundEffect>		_cSfx = new Dictionary<string,SoundEffect>( );
		Dictionary<string, Song>			_cSongs = new Dictionary<string,Song>( );
		ContentManager						_cContent;
		int									_iCurObjId = 0;

		public int iCurObjId	{ get { return ++_iCurObjId; }}

		public bool Init()
		{
			_cContent = DataStore.cInstance.cContent; 
			return true;
		}

		public TrooperTemplate CreateTemplate(TemplateConfig cTemplateData)
		{
			TrooperTemplate	cTemplate = new TrooperTemplate();
			AnimationData	cAnimData;
			Texture2D		cTexData;
			int				iMod;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(cTemplateData.sTrooperType, out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(cTemplateData.sTrooperType, cAnimData = _cContent.Load<AnimationData>(cTemplateData.sTrooperType));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(cTemplateData.sColor, out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(cTemplateData.sColor, cTexData = _cContent.Load<Texture2D>(cTemplateData.sColor));

			// build in the systems for the trooper
			cTemplate.cAnimProcessorRef = new AnimationProcessor(cAnimData);
			cTemplate.cTextureRef = cTexData;
			cTemplate.sTexName = cTemplateData.sColor;

			////ddhj modifications to the attack speed increment based on the template data. not sure if this 
			// is where we are going to want this to go 
			foreach(KeyValuePair<string, ActionData> tAction in cAnimData.cReferenceList["Attack"]["Basic"]) { 
				iMod = (int)(-tAction.Value.iIncrement * ((float)cTemplateData.cStats.iAtkSpeed / 100));
				cTemplate.cAnimProcessorRef.cActionIncrement.Add(tAction.Key, iMod);
			}

			// set the stats for the 
			cTemplate.cStats = new Stats(cTemplateData.cStats);

			// set the action manager, likely this will have some stuff from the template too
			cTemplate.cActionMgr = new TrooperActMgr(cTemplate.cAnimProcessorRef);

			// the battle heuristics are also going to be set here or in some other area based on template config data
			cTemplate.cAiData = new AiBattleData();

			//// ddhj: stat dialog ... this is going a little far
			cTemplate.sTemplateName = cTemplateData.sTemplateName;

			return cTemplate;
		}

		public Priest CreatePriest(Vector2 tPos, Team cTeam, int iMaxHp, int iMaxSlots, float fHealRate, float fRegenRate, BattlegroundData cBtlGndData) 
		{
			Priest			cPriest;
			AnimationData	cAnimData;
			Texture2D		cTexData;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(@"Sprite Data\Chaplain\ChaplainArray", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(@"Sprite Data\Chaplain\ChaplainArray", cAnimData = _cContent.Load<AnimationData>(@"Sprite Data\Chaplain\ChaplainArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"Sprite Data\Chaplain\Chaplain", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"Sprite Data\Chaplain\Chaplain", cTexData = _cContent.Load<Texture2D>(@"Sprite Data\Chaplain\Chaplain"));

			cPriest = new Priest(iMaxHp, iMaxSlots, fHealRate, fRegenRate, tPos, cTeam, cAnimData, cBtlGndData);
			cPriest.cTexRef = cTexData;
			cPriest.sTexName = @"Sprite Data\Chaplain\Chaplain";

			return cPriest;
		}

		public BasicBuff CreateBuff(ICombatant nCombatant, BattlegroundData cBgData)
		{
			AnimationData		cAnimData;
			Random				cRand = DataStore.cInstance.cRand;
			Texture2D			cTexData;
			BasicBuff			cBasicBuff;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(@"In Game\Buffs\DropsArray", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(@"In Game\Buffs\DropsArray", cAnimData = _cContent.Load<AnimationData>(@"In Game\Buffs\DropsArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"In Game\Buffs\Drops", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"In Game\Buffs\Drops", cTexData = _cContent.Load<Texture2D>(@"In Game\Buffs\Drops"));

			// buff data
			cBasicBuff = new BasicBuff(cAnimData, (EBuffEffects)cRand.Next((int)EBuffEffects.MaxBuffs), 3, cBgData);
			cBasicBuff.cTexRef = cTexData;
			cBasicBuff.sTexName = @"In Game\Buffs\Drops";

			return cBasicBuff;
		}

		public void CreateBuffRadiusObj(BuffClickEvent cClickObj) 
		{
			AnimationData		cAnimData;
			Random				cRand = DataStore.cInstance.cRand;
			Texture2D			cTexData;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(@"In Game\Buffs\GemsArray", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(@"In Game\Buffs\GemsArray", cAnimData = _cContent.Load<AnimationData>(@"In Game\Buffs\GemsArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"In Game\Buffs\Gems", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"In Game\Buffs\Gems", cTexData = _cContent.Load<Texture2D>(@"In Game\Buffs\Gems"));

			cClickObj.cTexRef = cTexData;
			cClickObj.sTexName = @"In Game\Buffs\Gems";
			cClickObj.cAnimData = cAnimData;
		}

		public BuffGem CreateBuffGem(EBuffEffects eType) 
		{
			BuffGem				cNewGem;
			AnimationData		cAnimData;
			Random				cRand = DataStore.cInstance.cRand;
			Texture2D			cTexData;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(@"In Game\Buffs\GemsArray", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(@"In Game\Buffs\GemsArray", cAnimData = _cContent.Load<AnimationData>(@"In Game\Buffs\GemsArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"In Game\Buffs\Gems", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"In Game\Buffs\Gems", cTexData = _cContent.Load<Texture2D>(@"In Game\Buffs\Gems"));

			cNewGem = new BuffGem(cAnimData, eType);
			cNewGem.cTexRef = cTexData;
			cNewGem.sTexName = @"In Game\Buffs\Gems";

			return cNewGem;
		}

		public BuffAnimal CreateAnimalEffect(EBuffEffects eType) 
		{
			BuffAnimal	cNewAnimal;
			string		sAnimalTex = string.Format(@"In Game\Buffs\Animals\{0}", Enum.GetName(typeof(EBuffEffects), eType).Split('_')[0]);
			Texture2D	cTexRef;

			if(!_cTextureList.TryGetValue(sAnimalTex, out cTexRef))
				// add the texture to the reference list 
				_cTextureList.Add(sAnimalTex, cTexRef = _cContent.Load<Texture2D>(sAnimalTex));
				
			cNewAnimal = new BuffAnimal(cTexRef);
			cNewAnimal.sTexName = sAnimalTex;
			cNewAnimal.tPos = new Vector2(DataStore.cInstance.cGraphics.Viewport.Width / 2 - cTexRef.Bounds.Width / 2, 
				DataStore.cInstance.cGraphics.Viewport.Height / 2 - cTexRef.Bounds.Height / 2);

			return cNewAnimal;
		}

		public BasicSprite CreateHammerIcon()
		{
			BasicSprite		cHammer = new BasicSprite();
			Texture2D		cTexData;
			AnimationData	cAnimData;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(@"In Game\Buffs\DropsArray", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(@"In Game\Buffs\DropsArray", cAnimData = _cContent.Load<AnimationData>(@"In Game\Buffs\DropsArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"In Game\Buffs\Drops", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"In Game\Buffs\Drops", cTexData = _cContent.Load<Texture2D>(@"In Game\Buffs\Drops"));

			cHammer.cTexRef = cTexData;
			cHammer.sTexName = @"In Game\Buffs\Drops";
			cHammer.cFrame = cAnimData.GetFrame(cAnimData.GetActionData("Main", "Sub", "hammer"), 0);
			cHammer.tPos = new Vector2(930, 30);

			return cHammer;
		}

		public Drop CreateDrop(string sDrop, BattlegroundData cBgData)
		{
			AnimationData		cAnimData;
			Random				cRand = DataStore.cInstance.cRand;
			Texture2D			cTexData;
			Drop				cBasicBuff;

			// check to see if we are already referencing this animation 
			if(!_cAnimationDataList.TryGetValue(@"In Game\Buffs\DropsArray", out cAnimData))
				// add it to the reference list
				_cAnimationDataList.Add(@"In Game\Buffs\DropsArray", cAnimData = _cContent.Load<AnimationData>(@"In Game\Buffs\DropsArray"));

			// check to see if we are already refencing this texture 
			if(!_cTextureList.TryGetValue(@"In Game\Buffs\Drops", out cTexData))
				// add the texture to the reference list 
				_cTextureList.Add(@"In Game\Buffs\Drops", cTexData = _cContent.Load<Texture2D>(@"In Game\Buffs\Drops"));

			// buff data
			cBasicBuff = new Drop(cAnimData, sDrop, 3, cBgData);
			cBasicBuff.cTexRef = cTexData;
			cBasicBuff.sTexName = @"In Game\Buffs\Drops";

			return cBasicBuff;
		}

		public ParticleEffect CreateParticleSystem( string sEffect )
		{
			ParticleEffect	cEffect;

			if( _cParticles.TryGetValue( sEffect, out cEffect ))
				cEffect = cEffect.DeepCopy( );
			else	{
				cEffect = _cContent.Load<ParticleEffect>( string.Format( @"Particles\{0}", sEffect ));
				// push the defined texture name into the proper directory
				foreach( Emitter cEmitter in cEffect )
					cEmitter.ParticleTextureAssetName = string.Format( @"Particles\{0}", cEmitter.ParticleTextureAssetName );
				cEffect.LoadContent( _cContent );
				_cParticles.Add( sEffect, cEffect );
			}
			cEffect.Initialise( );

			return cEffect;
		}

		public TerminatingParticleEffect CreateTerminatingParticleSystem( string sEffect )
		{
			ParticleEffect		cEffect;
			
			if( _cParticles.TryGetValue( sEffect, out cEffect ))
				cEffect = new TerminatingParticleEffect( cEffect.DeepCopy( ));
			else	{
				cEffect = new TerminatingParticleEffect( _cContent.Load<ParticleEffect>( string.Format( @"Particles\{0}", sEffect )));
				// push the defined texture name into the proper directory
				foreach( Emitter cEmitter in cEffect )
					cEmitter.ParticleTextureAssetName = string.Format( @"Particles\{0}", cEmitter.ParticleTextureAssetName );
			//	cEffect.LoadContent( _cContent );
				_cParticles.Add( sEffect, cEffect );
			}
			cEffect.LoadContent( _cContent );
			cEffect.Initialise( );

			return (TerminatingParticleEffect)cEffect;
		}

		public Song CreateMusic( string sSong )
		{
			Song		cSong;

			if( !_cSongs.TryGetValue( sSong, out cSong ))
			{
				cSong = _cContent.Load<Song>( string.Format( @"Music\{0}", sSong ));
				_cSongs.Add( sSong, cSong );
			}

			return cSong;
		}

		public SoundEffectInstance CreateSfx( string sSfx )
		{
			SoundEffect	cSfx;

			if( !_cSfx.TryGetValue( sSfx, out cSfx ))
			{
				cSfx = _cContent.Load<SoundEffect>( string.Format( @"SFX\{0}", sSfx ));
				_cSfx.Add( sSfx, cSfx );
			}

			return cSfx.CreateInstance( );
		}
	}

	public class ObjectManager
	{
		Dictionary<string, Dictionary<int, IDrawable>>		_cDrawRefList = new Dictionary<string,Dictionary<int,IDrawable>>();
		TerminatingParticleEffectManager	_cParticleSystemMgr;
		Dictionary<int, IActiveBasic>		_cActiveList = new Dictionary<int,IActiveBasic>();
		Dictionary<int, object>				_cMainObjectList = new Dictionary<int,object>();
		Dictionary<int, IClickable>			_cClickable = new Dictionary<int,IClickable>();

		bool		_bProcessClick = true;

		public BattlegroundData cParentData		{ get; set; }

		public void AddObject(object oData)
		{
			IObject nObj = (IObject)oData;
			Dictionary<int, IDrawable> caDrawList = null;

			// add the object to the mater list 
			_cMainObjectList.Add(nObj.iId, oData);

			// check if the object being added has a draw component
			if(oData is IDrawable) { 
				// check to see if the drawable component is a texture
				if(oData is IDrawableTexture) { 
					if(!_cDrawRefList.TryGetValue(((IDrawableTexture)oData).sTexName, out caDrawList))
						_cDrawRefList.Add(((IDrawableTexture)oData).sTexName, caDrawList = new Dictionary<int, IDrawable>());
				// the object is a drawable font
				} else if(oData is IDrawableFont) { 
					if(!_cDrawRefList.TryGetValue(((IDrawableFont)oData).sFontName, out caDrawList))
						_cDrawRefList.Add(((IDrawableFont)oData).sFontName, caDrawList = new Dictionary<int, IDrawable>());
				}

				// check to see if the draw has data 
				if(caDrawList != null) 
					caDrawList.Add(nObj.iId, (IDrawable)oData);
			}

			// check to see if the object has an active component 
			if(oData is IActiveBasic) 
				_cActiveList.Add(nObj.iId, (IActiveBasic)oData);
		}

		public void AddClickObject(object oData, DProcessClick dlProcess)
		{
			// add the object to the process and draw lists if possible
			AddObject(oData);

			// this object should be a clickable object, otherwize why the hell is AddClickObject getting called
			if(oData is IClickable) { 
				((IClickable)oData).dlProcessClick = dlProcess;
				_cClickable.Add(((IObject)oData).iId, (IClickable)oData);
			}
		}

		public void CreateParticleManager( )
		{
			Renderer	cRenderer = new SpriteBatchRenderer( );

			cRenderer.GraphicsDeviceService = DataStore.cInstance.cGfxMgr;
			cRenderer.LoadContent( DataStore.cInstance.cContent );
			_cParticleSystemMgr = new TerminatingParticleEffectManager( cRenderer );
		}

		public void AddParticleEffect( ParticleEffect cEffect )
		{
			_cParticleSystemMgr.Add( cEffect );
		}

		public void Process(GameTime cTime)
		{
			List<int>				iaRemList = new List<int>();
			List<IActiveBasic>		naActive = new List<IActiveBasic>();
			IObject					nObj;
			MouseState				cMouseState = Mouse.GetState();
			Point					tPoint = new Point(cMouseState.X, cMouseState.Y);
			List<IClickable>		naClickObjList = new List<IClickable>();

			// preprocess lists
			foreach(object oObj in _cMainObjectList.Values) { 
				nObj = (IObject)oObj;
				// take care of the objects that no longer need to draw
				if(oObj is IDrawable)
					if((nObj.eObjState & EObjectStates.Draw) != EObjectStates.Draw) { 
						if(oObj is IDrawableFont)
							_cDrawRefList[((IDrawableFont)oObj).sFontName].Remove(nObj.iId);
						else _cDrawRefList[((IDrawableTexture)oObj).sTexName].Remove(nObj.iId);

						// remove the clickable since you can't see it 
						_cClickable.Remove(nObj.iId);
					}

				// remove any objects no longer active
				if(oObj is IActiveBasic) { 
					if((nObj.eObjState & EObjectStates.Active) == EObjectStates.Active)
						naActive.Add((IActiveBasic)oObj);
					else _cActiveList.Remove(nObj.iId);
				}

				// if the object has no state left then blow it away
				if(nObj.eObjState == 0)
					iaRemList.Add(nObj.iId);
			}

			// check the mouse button click 
			foreach(KeyValuePair<int, IClickable> tClickObj in _cClickable)
				naClickObjList.Add(tClickObj.Value);

			if(cMouseState.LeftButton == ButtonState.Pressed) { 
				if(_bProcessClick == true) { 
					foreach(IClickable nClickObj in naClickObjList)
						if(nClickObj.ContainsPoint(tPoint))
							if(nClickObj.dlProcessClick != null)
								nClickObj.dlProcessClick(this.cParentData, nClickObj);
						
					_bProcessClick = false;
				}
			} else _bProcessClick = true;

			// remove all objects from the main processing list
			foreach(int iObj in iaRemList)
				_cMainObjectList.Remove(iObj);

			// process all the active objects
			foreach(IActiveBasic nActiveObj in naActive)
				nActiveObj.Process(cTime);

			_cParticleSystemMgr.Update( cTime );
		}

		public void Draw(SpriteBatch cBatch)
		{
			// draw each object by their texture
			foreach(Dictionary<int, IDrawable> cDrawList in _cDrawRefList.Values)
				foreach(IDrawable nObj in cDrawList.Values)
					nObj.Draw(cBatch);
		}

		public void DrawParticles( SpriteBatch cBatch )
		{
			// draw each object by their texture
			if( _cParticleSystemMgr.Count > 0 )
				_cParticleSystemMgr.Draw( );
		}

		public void Clear()
		{
			_cActiveList.Clear();
			foreach(Dictionary<int, IDrawable> cDrawList in _cDrawRefList.Values)
				cDrawList.Clear();
			_cDrawRefList.Clear();
			_cMainObjectList.Clear();
			_cParticleSystemMgr.Clear( );

			// clear the effect managers
			foreach( ParticleEffect cEffect in _cParticleSystemMgr )
				cEffect.Terminate( );
			_cParticleSystemMgr.Clear( );
		}
	}
}
