// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMercury.Renderers
{
	//// PORT (Phase 3, T3.2): Mercury's SpriteBatchRenderer. Owns its own SpriteBatch
	//// (game code never passes one in -- confirmed by grep, DrawParticles(SpriteBatch) never
	//// forwards its parameter to the particle system, see docs/PORT_NOTES.md Phase 0 T0.4).
	//// Phase 4 fills in the actual per-particle draw loop inside RenderEffect.
	public class SpriteBatchRenderer : Renderer
	{
		SpriteBatch _cSpriteBatch;

		public override void LoadContent(ContentManager cContent)
		{
			base.LoadContent(cContent);

			if (GraphicsDeviceService != null)
				_cSpriteBatch = new SpriteBatch(GraphicsDeviceService.GraphicsDevice);
		}

		internal override void RenderEffect(ParticleEffect cEffect)
		{
			// Phase 4: iterate cEffect's live particles and draw each via _cSpriteBatch, using
			// the emitter's ParticleTextureAssetName-resolved texture. No live particle state
			// exists yet at this phase (Emitter doesn't simulate/spawn until Phase 4), so this
			// is intentionally a no-op for now -- the shim needs to compile and link end-to-end
			// before simulation is real.
		}
	}
}
