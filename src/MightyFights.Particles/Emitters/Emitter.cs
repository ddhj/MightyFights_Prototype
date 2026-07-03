// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// project includes
using ProjectMercury.Modifiers;

namespace ProjectMercury.Emitters
{
	//// PORT (Phase 3 T3.2 / Phase 4 T4.1-T4.2): Mercury's Emitter. Phase 3 defined the members
	//// game code touches directly (Name, ParticleTextureAssetName, Term); Phase 4 adds the full
	//// XML-authored tuning data and the actual simulation (spawn/age/modify/cull), ground-truthed
	//// against the corrected emitter/field inventory in docs/PORT_NOTES.md Phase 4. Used
	//// directly (no Type attribute in the XML) 9 times across the 15 files -- a plain,
	//// shapeless point emitter is a real, common case, not a fallback -- so this class is
	//// concrete/instantiable, not abstract.
	public class Emitter
	{
		public string Name { get; set; }
		public string ParticleTextureAssetName { get; set; }
		public float Term { get; set; }
		public bool Enabled { get; set; } = true;

		public int Budget { get; set; } = 100;
		public int ReleaseQuantity { get; set; } = 1;
		public float MinimumTriggerPeriod { get; set; }
		public string BlendMode { get; set; } = "Alpha";
		public Vector2 TriggerOffset { get; set; }
		public Vector2 ReleaseImpulse { get; set; }

		public FloatRange ReleaseSpeed { get; set; }
		public ColourRange ReleaseColour { get; set; } = new ColourRange { Value = Vector3.One };
		public FloatRange ReleaseOpacity { get; set; } = new FloatRange { Value = 1f };
		public FloatRange ReleaseScale { get; set; } = new FloatRange { Value = 1f };
		public FloatRange ReleaseRotation { get; set; }

		public List<Modifier> Modifiers { get; set; } = new List<Modifier>();

		// Populated by ParticleEffect.LoadContent() resolving ParticleTextureAssetName. Internal
		// because no game code reads it directly; the SpriteBatchRenderer does.
		internal Texture2D ParticleTexture { get; set; }

		List<Particle> _cLiveParticles = new List<Particle>();
		static readonly Random s_cRand = new Random();

		public Emitter()
		{
		}

		//// Releases ReleaseQuantity new particles at tPosition (+TriggerOffset), respecting
		//// Budget. Real Mercury supports continuous per-frame emission gated by
		//// MinimumTriggerPeriod; every confirmed call site in this game (BloodSpray, Buff
		//// Sparkle, HealingCircle, HealerRecharge -- see docs/PORT_NOTES.md) fires Trigger()
		//// once per event rather than holding it down, so a single burst-release per call is
		//// sufficient and matches actual usage (T4.4: best-effort, not pixel-exact).
		public void Trigger(Vector2 tPosition)
		{
			if (!Enabled)
				return;

			Vector2 tOrigin = tPosition + TriggerOffset;
			int iSpawnCount = Math.Min(ReleaseQuantity, Math.Max(0, Budget - _cLiveParticles.Count));

			for (int i = 0; i < iSpawnCount; ++i)
				_cLiveParticles.Add(SpawnParticle(tOrigin));
		}

		//// Shape-specific spawn position/velocity. Base Emitter (no XML Type attribute) has no
		//// shape at all: particles release from tOrigin itself, direction driven purely by
		//// ReleaseImpulse/ReleaseRotation -- confirmed correct by field inventory (base-Emitter
		//// XML entries never include Radius/Direction/Length/Width, only the common release
		//// fields).
		protected virtual Particle SpawnParticle(Vector2 tOrigin)
		{
			return NewParticle(tOrigin, ReleaseImpulse);
		}

		protected Particle NewParticle(Vector2 tPosition, Vector2 tBaseVelocity)
		{
			float fSpeed = ReleaseSpeed.Sample(s_cRand);
			float fAngle = (float)(s_cRand.NextDouble() * MathHelper.TwoPi);
			Vector2 tVelocity = tBaseVelocity + new Vector2((float)Math.Cos(fAngle), (float)Math.Sin(fAngle)) * fSpeed;

			return new Particle
			{
				Position = tPosition,
				Velocity = tVelocity,
				Rotation = ReleaseRotation.Sample(s_cRand),
				RotationRate = 0f,
				Colour = ReleaseColour.Sample(s_cRand),
				Opacity = ReleaseOpacity.Sample(s_cRand),
				Scale = ReleaseScale.Sample(s_cRand),
				Age = 0f,
				Term = Term,
			};
		}

		public void Update(float fElapsedSeconds)
		{
			for (int i = _cLiveParticles.Count - 1; i >= 0; --i)
			{
				Particle cParticle = _cLiveParticles[i];

				cParticle.Age += fElapsedSeconds;
				if (cParticle.IsExpired)
				{
					_cLiveParticles.RemoveAt(i);
					continue;
				}

				cParticle.Position += cParticle.Velocity * fElapsedSeconds;

				foreach (Modifier cModifier in Modifiers)
					cModifier.Process(fElapsedSeconds, ref cParticle);

				_cLiveParticles[i] = cParticle;
			}
		}

		internal IReadOnlyList<Particle> LiveParticles => _cLiveParticles;

		//// Public on purpose (unlike LiveParticles) -- useful for a debug HUD or the kind of
		//// "no unbounded particle growth" soak-test monitoring the port plan's T4.2 gate calls
		//// for, without exposing per-particle simulation internals.
		public int LiveParticleCount => _cLiveParticles.Count;

		//// PORT note for Phase 4 (was a Phase 3 stub, now real): clones tuning data plus a
		//// fresh (empty) live-particle buffer -- DeepCopy hands out independent per-battle
		//// instances of a cached effect template, so simulation state must not be shared.
		internal virtual Emitter ShallowClone()
		{
			Emitter cClone = (Emitter)this.MemberwiseClone();
			cClone._cLiveParticles = new List<Particle>();
			cClone.Modifiers = new List<Modifier>(this.Modifiers);
			return cClone;
		}
	}

	//// Value/Variation pair, matches the <Value>/<Variation> XML sub-elements used by
	//// ReleaseSpeed/ReleaseOpacity/ReleaseScale/ReleaseRotation.
	public struct FloatRange
	{
		public float Value;
		public float Variation;

		public float Sample(Random cRand)
		{
			return Value + ((float)cRand.NextDouble() * 2f - 1f) * Variation;
		}
	}

	//// Same Value/Variation shape as FloatRange, but per-channel (RGB), for ReleaseColour.
	public struct ColourRange
	{
		public Vector3 Value;
		public Vector3 Variation;

		public Vector3 Sample(Random cRand)
		{
			return new Vector3(
				Value.X + ((float)cRand.NextDouble() * 2f - 1f) * Variation.X,
				Value.Y + ((float)cRand.NextDouble() * 2f - 1f) * Variation.Y,
				Value.Z + ((float)cRand.NextDouble() * 2f - 1f) * Variation.Z);
		}
	}
}
