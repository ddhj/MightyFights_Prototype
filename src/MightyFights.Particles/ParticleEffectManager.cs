// system includes
using System;
using System.Collections.Generic;

// project includes
using ProjectMercury.Renderers;

namespace ProjectMercury
{
	//// PORT (Phase 3, T3.2): Mercury's ParticleEffectManager. Not in the port plan's original
	//// §3 API sketch at all -- found by grep during Phase 0 (docs/PORT_NOTES.md T0.4
	//// correction #1). Game code (TerminatingParticleEffectManager, in
	//// Support Classes/Particle System Objects.cs) uses it as a List<ParticleEffect>: enumerable,
	//// indexer + RemoveAt, Count, Clear -- all inherited here, not redeclared.
	public class ParticleEffectManager : List<ParticleEffect>
	{
		protected Renderer cRenderer;

		public ParticleEffectManager(Renderer cRenderer)
		{
			this.cRenderer = cRenderer;
		}

		//// Confirmed 2-arg call site: TerminatingParticleEffectManager.Update(GameTime) calls
		//// base.Update((float)cTime.ElapsedGameTime.TotalSeconds, false). The bool parameter's
		//// semantics were never pinned down against real Mercury source (see
		//// docs/PORT_NOTES.md Phase 0 T0.4, still an open item) -- named generically here;
		//// revisit if Phase 4's simulation needs it to mean something specific (e.g. suppress
		//// trigger-driven emission during a paused/dialog state).
		public virtual void Update(float fElapsedSeconds, bool bTriggerEvents)
		{
			foreach (ParticleEffect cEffect in this)
				cEffect.Update(fElapsedSeconds);
		}

		//// Confirmed call site is always parameterless -- no SpriteBatch/Matrix is ever passed
		//// by game code (docs/PORT_NOTES.md Phase 0 T0.4 correction #2). The renderer owns its
		//// own draw state.
		public virtual void Draw()
		{
			foreach (ParticleEffect cEffect in this)
				cRenderer.RenderEffect(cEffect);
		}
	}
}
