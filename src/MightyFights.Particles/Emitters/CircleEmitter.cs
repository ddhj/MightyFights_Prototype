// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	//// PORT (Phase 4, T4.2): fields ground-truthed against the XML (Radius, Ring, Radiate --
	//// see docs/PORT_NOTES.md Phase 4). Ring = emit from the circle's perimeter only vs a
	//// filled disc; Radiate = initial velocity points outward from the emitter's origin rather
	//// than a random direction. Best-effort per T4.4.
	public class CircleEmitter : Emitter
	{
		public float Radius { get; set; }
		public bool Ring { get; set; }
		public bool Radiate { get; set; }

		static readonly Random s_cRand = new Random();

		protected override Particle SpawnParticle(Vector2 tOrigin)
		{
			float fAngle = (float)(s_cRand.NextDouble() * MathHelper.TwoPi);
			float fDist = Ring ? Radius : Radius * (float)Math.Sqrt(s_cRand.NextDouble());
			Vector2 tOffset = new Vector2((float)Math.Cos(fAngle), (float)Math.Sin(fAngle)) * fDist;

			Vector2 tBaseVelocity = ReleaseImpulse;
			if (Radiate && tOffset.LengthSquared() > 0.0001f)
			{
				Vector2 tOutward = Vector2.Normalize(tOffset);
				tBaseVelocity += tOutward * ReleaseSpeed.Sample(s_cRand);
				Particle cParticle = NewParticle(tOrigin + tOffset, Vector2.Zero);
				cParticle.Velocity = tBaseVelocity;
				return cParticle;
			}

			return NewParticle(tOrigin + tOffset, tBaseVelocity);
		}
	}
}
