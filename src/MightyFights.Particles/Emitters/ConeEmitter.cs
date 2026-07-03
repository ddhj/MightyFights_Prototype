// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	//// PORT (Phase 4, T4.2): fields ground-truthed against the XML (Direction, ConeAngle --
	//// see docs/PORT_NOTES.md Phase 4). Spawns at the emitter's origin (no shape offset);
	//// initial velocity direction is randomized within +/-ConeAngle/2 of Direction. Best-effort
	//// per T4.4.
	public class ConeEmitter : Emitter
	{
		public float Direction { get; set; }
		public float ConeAngle { get; set; }

		static readonly Random s_cRand = new Random();

		protected override Particle SpawnParticle(Vector2 tOrigin)
		{
			float fAngle = Direction + ((float)s_cRand.NextDouble() - 0.5f) * ConeAngle;
			float fSpeed = ReleaseSpeed.Sample(s_cRand);
			Vector2 tVelocity = ReleaseImpulse + new Vector2((float)Math.Cos(fAngle), (float)Math.Sin(fAngle)) * fSpeed;

			Particle cParticle = NewParticle(tOrigin, Vector2.Zero);
			cParticle.Velocity = tVelocity;
			return cParticle;
		}
	}
}
