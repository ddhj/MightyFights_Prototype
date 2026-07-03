// system includes
using System;
using System.Collections.Generic;

// 3rd party includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMercury.Renderers
{
	//// PORT (Phase 3, T3.2): Mercury's Renderer base class. Field type confirmed by grep --
	//// game code declares "Renderer cRenderer = new SpriteBatchRenderer();" (see
	//// BObjectManager.cs / CObjectManager.cs), so SpriteBatchRenderer : Renderer.
	//// GraphicsDeviceService is set from DataStore.cGfxMgr (a GraphicsDeviceManager) -- typed
	//// directly as GraphicsDeviceManager here rather than the real Mercury's
	//// IGraphicsDeviceService, since that's the only concrete type ever assigned to it.
	public abstract class Renderer
	{
		public GraphicsDeviceManager GraphicsDeviceService { get; set; }

		public virtual void LoadContent(ContentManager cContent)
		{
		}

		//// PORT note for Phase 4: no game code calls RenderEffect directly (confirmed by grep,
		//// see docs/PORT_NOTES.md Phase 0 T0.4 correction #2) -- only
		//// ParticleEffectManager.Draw() does, internally. That means this method's exact
		//// signature is this shim's own internal contract, not something that has to match
		//// upstream Mercury's overload. Phase 4 implements the actual per-particle draw loop
		//// here; Phase 3 leaves it a no-op so the shim compiles and links end-to-end first.
		internal virtual void RenderEffect(ParticleEffect cEffect)
		{
		}
	}
}
