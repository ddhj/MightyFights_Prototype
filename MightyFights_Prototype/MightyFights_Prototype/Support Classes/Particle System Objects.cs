// system includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 3rd party includes
using Microsoft.Xna.Framework;

using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Renderers;

namespace MightyFights_Prototype	{

	public class TerminatingParticleEffect : ParticleEffect	{
	// Data
		long	_lTimeToTerminate = 0,
				_lCurTime;

	// Properties
		public bool bTimeElapsed	{ get { return _lCurTime > _lTimeToTerminate; }}

	// Constructor
		public TerminatingParticleEffect( ParticleEffect cEffect )
		{
			this.AddRange( cEffect );
			this.Author = cEffect.Author;
			this.Capacity = cEffect.Capacity;
			this.Controllers = cEffect.Controllers;
			this.Description = cEffect.Description;
			this.Name = cEffect.Name;
		}
	// functions
		public new void Initialise( )
		{
			long	lEmitter;
			base.Initialise( );

			foreach( Emitter cEmitter in this )
			{
				lEmitter = (long)( cEmitter.Term * 1000 );
				if( lEmitter > _lTimeToTerminate )
					_lTimeToTerminate = lEmitter;
			}
		}

		public override void Update( float fDeltaSeconds )
		{
			base.Update( fDeltaSeconds );

			_lCurTime += (long)( fDeltaSeconds * 1000 );
		}
	}

	public class TerminatingParticleEffectManager : ParticleEffectManager	{

		public TerminatingParticleEffectManager( Renderer cRenderer ) : base( cRenderer ) {}
		public void Update( GameTime cTime )
		{
			base.Update((float)cTime.ElapsedGameTime.TotalSeconds, false );

			for( int iCount = 0; iCount < this.Count; ++iCount )
				if( this[iCount] is TerminatingParticleEffect )
					if(((TerminatingParticleEffect)this[iCount] ).bTimeElapsed )
						this.RemoveAt( iCount-- );
		}
	}
}
