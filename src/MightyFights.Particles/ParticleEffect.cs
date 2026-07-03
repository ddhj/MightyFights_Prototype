// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// project includes
using ProjectMercury.Emitters;

namespace ProjectMercury
{
	//// PORT (Phase 3, T3.2): Mercury's ParticleEffect. Confirmed by grep to behave as a
	//// List<Emitter> subclass -- TerminatingParticleEffect's constructor
	//// (Support Classes/Particle System Objects.cs) does "this.AddRange(cEffect)" and copies
	//// Author/Capacity/Controllers/Description/Name, all via plain get/set. Capacity is
	//// List<Emitter>'s own inherited property, not redeclared here. See docs/PORT_NOTES.md
	//// Phase 0 T0.4 for the full grep-verified surface and Phase 3's corrections to plan §3.
	public class ParticleEffect : List<Emitter>
	{
		public string Author { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }

		//// Real Mercury exposes a Controllers collection (global effect-level behaviors).
		//// Confirmed by grep: game code only ever copies this through
		//// (TerminatingParticleEffect's constructor), never reads/writes individual entries --
		//// so it's modeled as an opaque object here rather than guessing its element type.
		public object Controllers { get; set; }

		public ParticleEffect()
		{
		}

		public virtual void Initialise()
		{
		}

		//// Resolves each Emitter's ParticleTextureAssetName through the given ContentManager.
		//// DataManager.CreateParticleSystem/CreateTerminatingParticleSystem prefix the raw
		//// asset name with "Particles\" before this runs -- see DataManager.cs.
		public virtual void LoadContent(ContentManager cContent)
		{
			foreach (Emitter cEmitter in this)
				if (!string.IsNullOrEmpty(cEmitter.ParticleTextureAssetName))
					cEmitter.ParticleTexture = cContent.Load<Texture2D>(cEmitter.ParticleTextureAssetName);
		}

		public virtual void Update(float fElapsedSeconds)
		{
			// Phase 4: advance each emitter's live particles. No particle state is simulated
			// yet at this phase -- see Emitters/Emitter.cs.
		}

		//// Triggers a one-shot release of particles at the given world position. Confirmed 4
		//// call sites (BloodSpray, Buff Sparkle, HealingCircle, HealerRecharge -- see
		//// docs/PORT_NOTES.md). Phase 4 implements the actual spawn; Phase 3 just needs the
		//// call site to resolve.
		public virtual void Trigger(Vector2 tPosition)
		{
		}

		//// Stops any further particle release (confirmed 2 call sites in BObjectManager.cs /
		//// CObjectManager.cs, both inside a Clear()-adjacent teardown path). Existing
		//// TerminatingParticleEffect semantics (Term-based auto-termination, see Particle
		//// System Objects.cs) are independent of this and must stay exact once Phase 4 lands --
		//// see docs/PORT_NOTES.md Phase 0 T0.4 "Confirmed" section.
		public virtual void Terminate()
		{
			foreach (Emitter cEmitter in this)
				cEmitter.Enabled = false;
		}

		//// Confirmed used by DataManager.CreateParticleSystem/CreateTerminatingParticleSystem
		//// to hand out an independent copy per battle/instance from a single cached template.
		public virtual ParticleEffect DeepCopy()
		{
			ParticleEffect cCopy = new ParticleEffect();
			cCopy.Author = this.Author;
			cCopy.Description = this.Description;
			cCopy.Name = this.Name;
			cCopy.Controllers = this.Controllers;

			foreach (Emitter cEmitter in this)
				cCopy.Add(cEmitter.ShallowClone());

			return cCopy;
		}
	}
}
