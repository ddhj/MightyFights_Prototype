// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using ProjectMercury;

// project includes
using MightyFights_Support;

namespace MightyFights_Prototype	{
	public partial class Trooper : Combatant, IDrawableTexture, IAnimate, IActive<Trooper>	{
	// Data
		Texture2D	_cTexRef;
		Stats		_cInitialStats;
		//// ddhj: kaiju -- protected so oversized combatant subclasses can widen the swarm
		//// capacity and render scale (captains already used _fScale via the "Cap" name check).
		//// This is the "rated strength" knob (owner-directed): how many troopers can
		//// meaningfully engage this target at once, a design/gameplay decision independent of
		//// its pure visual size -- attack-slot GEOMETRY (below) generates that many rendezvous
		//// points, evenly spread around the target, so raising this actually creates more room
		//// rather than crowding more attackers onto a fixed handful of points.
		protected int	_iAvailablePositions = 6;
		//// ddhj: kaiju -- protected so a subclass's own attack heuristic (Kaiju.Attack_Kaiju)
		//// can drive the same attack-range/animation/attacker-bookkeeping plumbing the base
		//// Attack_Basic uses, instead of duplicating Trooper's private state.
		protected int	_iAttackingPos;
		float		_fZorder;
		protected float	_fScale = 1.25f;
		//// ddhj: kaiju -- which of the (dynamically sized, _iAvailablePositions-wide) radial
		//// attack slots are currently occupied. Lazily (re)sized in EnsureSlotArray because a
		//// subclass constructor (e.g. Kaiju's) only overwrites _iAvailablePositions AFTER the
		//// base Trooper constructor has already run.
		bool[]		_baSlotTaken;
		protected bool	_bAttacking;

		ParticleEffect				_cBloodSpray;
		ActionManager<Trooper>		_cActionMgr;
		protected AnimationProcessor	_cAnimProc;
		BattlegroundData			_cBattleDataRef = null;

		SoundEffectInstance			_cSfxCrit,
									_cSfxParry;

		Dictionary<EBuffEffects, BuffActionData>	_cBuffList = new Dictionary<EBuffEffects,BuffActionData>();
		//// ddhj: kaiju -- key is now a plain radial slot index (0.._iAvailablePositions-1),
		//// not the old fixed 6-value ETrooperAttackPos bitmask enum. See Trooper_Combatant.cs.
		Dictionary<int, Combatant>	_cAttackers = new Dictionary<int, Combatant>();

		////ddhj template stuff... not sure if it should go on trooper proper
		float	_fRunMovementSpeed,
				_fWalkMovementSpeed;
		long	_lFleeRunStop = 2000,
				_lFleeCurTime;

	// Properties
		public float fZRange			{ get; set; }
		public bool bActive				{ get; set; }
		public bool bDir				{ get; set; }
		public int iWeaponRngSq			{ get; set; }
		public Vector2 tAttackPos		{ get; set; }
		public float fZorder			{ get { return _fZorder; }}
		public override float fCombatantScale	{ get { return _fScale; }}


		public Texture2D cTexRef		{ get { return _cTexRef; } set { _cTexRef = value; }}
		public Frame cFrame				{ get { return _cAnimProc.cCurFrame; } set{}}
		public string sTexName			{ get; set; }
		public override Vector2 tPos	{ get { return _tPos; } set { _tPos = value; UpdateRefPoints(); }}

		//// ddhj: this is just for the stats dialog, longer term we are probably going to need the template id
		// so the trooper can dump their exp into it
		public string sTemplateName		{ get; set; }

		public override bool bAvailablePos		{ get { return _cAttackers.Count < _iAvailablePositions; }}

		//public Dictionary<EBuffEffects, BuffActionData> cBuffList		{ get { return _cBuffList; }}
		public AnimationProcessor cAnimationProcessor	{ get { return _cAnimProc; } set { _cAnimProc = value; }}
		public ActionManager<Trooper>	cActionManager	{ get { return _cActionMgr; } set { _cActionMgr = value; }}

	// Constructor
		public Trooper(int iId, Team cTeam, TrooperTemplate cTemplate)
		{
			_iId = iId;
			_cTeam = cTeam;

			_cAnimProc = cTemplate.cAnimProcessorRef;
			_cTexRef = cTemplate.cTextureRef;
			_cActionMgr = cTemplate.cActionMgr;
			_cActionMgr.cData = this;
			_cInitialStats = new Stats(cTemplate.cStats);
			_cStats = cTemplate.cStats;
			cAiData = cTemplate.cAiData;
			sTexName = cTemplate.sTexName;
			
			//// ddhj stats dialog 
			this.sTemplateName = cTemplate.sTemplateName;
			//// this is just a test of scale captains
			if(this.sTemplateName.Contains("Cap"))
				_fScale = 1.4f;
			_cExpData = new ExperienceData( );

			_cActionMgr.AddPermAction(new Action(this.TrooperUpkeep, null, null));

			////ddhj: this is the initial area for the template config, this will probably change over time
			CalcMovementSpeed();

			// use the template data to set up the heuristics... 
			//// there is a problem here since the object manager should have set this up but the heuristics are methods on an instance of trooper,
			//// considered static methods but I am not sure I like that solution
			cAiData.cHeurisitics[EBattleHeuristics.Attack] = Attack_Basic;
			cAiData.cHeurisitics[EBattleHeuristics.ChooseOpponent] = ChooseOpponent;
			cAiData.cHeurisitics[EBattleHeuristics.Flee] = Flee;
			cAiData.cHeurisitics[EBattleHeuristics.Idle] = Idle;
			cAiData.cHeurisitics[EBattleHeuristics.Pant] = Pant;
			cAiData.cHeurisitics[EBattleHeuristics.Persue] = Persue;
	
			// this is going to come from somewhere
			this.iWeaponRange = 10;
			this.iWeaponRngSq = this.iWeaponRange * this.iWeaponRange;

			// set its states
			_eObjState = EObjectStates.Active | EObjectStates.Draw;

			// get the id from the object manager proper
			this.iId = DataManager.cInstance.iCurObjId;

			_cSfxCrit = DataManager.cInstance.CreateSfx( "Decapitation-SoundBible.com-800292304" );
			_cSfxParry = DataManager.cInstance.CreateSfx( "Swords_Collide-Sound_Explorer-2015600826" );
			_cSfxCrit.Volume = _cSfxParry.Volume = .2f;
		}
		
		void CalcMovementSpeed()
		{
			////ddhj: this is the initial area for the template config, this will probably change over time
			_fRunMovementSpeed = 2.5f * (1.0f + _cStats.iMovement / 100f);
			_fWalkMovementSpeed = (1.0f + _cStats.iMovement / 100f);
		}

		void CalcAtackSpeeds()
		{
			int iMod;
			
			// since we are a trooper and we know that we have basic attacks we are going to walk the list of them 
			// and modify their values by our attack speed
			foreach(KeyValuePair<string, ActionData> tAction in _cAnimProc.cAnimData.cReferenceList["Attack"]["Basic"]) { 
				iMod = (int)(-tAction.Value.iIncrement * ((float)_cStats.iAtkSpeed / 100));
				_cAnimProc.cActionIncrement[tAction.Key] = iMod;
			}
		}

		void CalibrateStats()
		{
			// if there are buffs to process
			IBattleStats nStats = new Stats(_cInitialStats);

			foreach(KeyValuePair<EBuffEffects, BuffActionData> tBuff in _cBuffList)
				tBuff.Value.dlBuffEffect(nStats);

			_cStats.SetStats(nStats);

			CalcMovementSpeed();
			CalcAtackSpeeds();
		}
	}
}
