// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMercury.Emitters
{
	//// PORT (Phase 3, T3.2): Mercury's Emitter base class. Confirmed by grep (port plan T0.4 /
	//// docs/PORT_NOTES.md "T0.4 -- Mercury API surface") that game code only ever touches
	//// Term and ParticleTextureAssetName -- everything else (Budget, ReleaseQuantity/Speed/
	//// Colour/Opacity/Scale, Modifiers, BlendMode, TriggerOffset, etc.) is XML-authored tuning
	//// data, never read or written from C#. Phase 4 (Effect Loading & Simulation) adds the
	//// concrete subtypes (CircleEmitter/ConeEmitter/LineEmitter/RectEmitter -- see the emitter
	//// inventory in PORT_NOTES.md Phase 0) and the actual simulation data/logic; this class is
	//// deliberately minimal for now, just enough for the 17 Mercury-touching files to compile
	//// unchanged against it (Phase 3's gate).
	public class Emitter
	{
		public string Name { get; set; }
		public string ParticleTextureAssetName { get; set; }
		public float Term { get; set; }

		// Populated by ParticleEffect.LoadContent() resolving ParticleTextureAssetName. Internal
		// because no game code reads it directly today; the Phase 4 SpriteBatchRenderer will.
		internal Texture2D ParticleTexture { get; set; }

		// Whether this emitter is actively releasing particles (XML "Enabled" field). Not
		// currently read from C#, but needed by Phase 4's simulation loop -- declared now so
		// Phase 4 doesn't need to touch this class's public surface.
		public bool Enabled { get; set; } = true;

		public Emitter()
		{
		}

		//// PORT note for Phase 4: once Modifiers/reference-typed tuning data are added here,
		//// revisit ParticleEffect.DeepCopy()'s per-emitter clone -- it currently relies on
		//// MemberwiseClone, which is only correct while every field on this class is a value
		//// type or immutable (string).
		internal virtual Emitter ShallowClone()
		{
			return (Emitter)this.MemberwiseClone();
		}
	}
}
