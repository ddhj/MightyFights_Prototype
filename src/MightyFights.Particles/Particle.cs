// 3rd party includes
using Microsoft.Xna.Framework;

namespace ProjectMercury
{
	//// PORT (Phase 4, T4.2): live per-particle simulation state. Not part of Mercury's public
	//// API (no game code touches individual particles), purely an implementation detail of the
	//// shim's Emitter/Modifier simulation loop.
	public struct Particle
	{
		public Vector2 Position;
		public Vector2 Velocity;
		public float Rotation;
		public float RotationRate;
		public Vector3 Colour;
		public float Opacity;
		public float Scale;

		public float Age;   // seconds since spawn
		public float Term;  // this particle's lifetime, seconds (copied from Emitter.Term at spawn)

		public float NormalizedAge => Term <= 0f ? 1f : Age / Term;
		public bool IsExpired => Age >= Term;
	}
}
