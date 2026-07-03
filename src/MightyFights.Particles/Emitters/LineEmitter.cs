// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	//// PORT (Phase 4, T4.2): fields ground-truthed against the XML (Length, Angle, Rectilinear,
	//// EmitBothWays -- see docs/PORT_NOTES.md Phase 4). Spawn point is a random position along
	//// a line segment of the given Length, oriented at Angle, centered on the emitter's origin.
	//// Rectilinear = initial velocity is perpendicular to the line rather than a random
	//// direction; EmitBothWays = that perpendicular velocity alternates to either side of the
	//// line rather than always the same side. Best-effort per T4.4.
	public class LineEmitter : Emitter
	{
		public float Length { get; set; }
		public float Angle { get; set; }
		public bool Rectilinear { get; set; }
		public bool EmitBothWays { get; set; }

		static readonly Random s_cRand = new Random();

		protected override Particle SpawnParticle(Vector2 tOrigin)
		{
			float fT = ((float)s_cRand.NextDouble() - 0.5f) * Length;
			Vector2 tAlong = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
			Vector2 tSpawnPos = tOrigin + tAlong * fT;

			Vector2 tBaseVelocity = ReleaseImpulse;
			if (Rectilinear)
			{
				Vector2 tPerp = new Vector2(-tAlong.Y, tAlong.X);
				if (EmitBothWays && s_cRand.Next(2) == 0)
					tPerp = -tPerp;
				tBaseVelocity += tPerp * ReleaseSpeed.Sample(s_cRand);
				Particle cParticle = NewParticle(tSpawnPos, Vector2.Zero);
				cParticle.Velocity = tBaseVelocity;
				return cParticle;
			}

			return NewParticle(tSpawnPos, tBaseVelocity);
		}
	}
}
