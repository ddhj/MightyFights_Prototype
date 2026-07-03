// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// project includes
using ProjectMercury.Emitters;

namespace ProjectMercury.Renderers
{
	//// PORT (Phase 3 T3.2 / Phase 4 T4.2): Mercury's SpriteBatchRenderer. Owns its own
	//// SpriteBatch (game code never passes one in -- confirmed by grep, DrawParticles
	//// (SpriteBatch) never forwards its parameter to the particle system, see
	//// docs/PORT_NOTES.md Phase 0 T0.4). Draws per-Emitter (one Begin/End pair each) rather
	//// than batching the whole effect together, since different emitters in the same effect
	//// can use different BlendMode strings ("Alpha" vs "Add") -- simplest correct approach at
	//// this game's particle scale (budgets ~500 total per the port plan).
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
			if (_cSpriteBatch == null)
				return;

			foreach (Emitter cEmitter in cEffect)
			{
				if (cEmitter.ParticleTexture == null || cEmitter.LiveParticles.Count == 0)
					continue;

				BlendState cBlendState = string.Equals(cEmitter.BlendMode, "Add", StringComparison.OrdinalIgnoreCase)
					? BlendState.Additive
					: BlendState.AlphaBlend;

				Texture2D cTexture = cEmitter.ParticleTexture;
				Vector2 tOrigin = new Vector2(cTexture.Width / 2f, cTexture.Height / 2f);

				_cSpriteBatch.Begin(SpriteSortMode.Deferred, cBlendState);
				foreach (Particle cParticle in cEmitter.LiveParticles)
				{
					Color cColor = new Color(cParticle.Colour.X, cParticle.Colour.Y, cParticle.Colour.Z)
						* MathHelper.Clamp(cParticle.Opacity, 0f, 1f);

					_cSpriteBatch.Draw(cTexture, cParticle.Position, null, cColor, cParticle.Rotation,
						tOrigin, cParticle.Scale / Math.Max(cTexture.Width, cTexture.Height), SpriteEffects.None, 0f);
				}
				_cSpriteBatch.End();
			}
		}
	}
}
