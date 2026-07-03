// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace ProjectMercury.Emitters
{
	//// PORT (Phase 4, T4.2): fields ground-truthed against the XML (Width, Height, Rotation,
	//// Frame -- see docs/PORT_NOTES.md Phase 4). Frame = emit from the rectangle's border only
	//// vs a filled area (same idea as CircleEmitter.Ring). Rotation orients the rectangle
	//// around the emitter's origin. Best-effort per T4.4.
	public class RectEmitter : Emitter
	{
		public float Width { get; set; }
		public float Height { get; set; }
		public float Rotation { get; set; }
		public bool Frame { get; set; }

		static readonly Random s_cRand = new Random();

		protected override Particle SpawnParticle(Vector2 tOrigin)
		{
			float fX, fY;
			if (Frame)
			{
				// pick a point on one of the 4 edges, uniformly by perimeter
				float fPerimeter = 2f * (Width + Height);
				float fPick = (float)s_cRand.NextDouble() * fPerimeter;
				if (fPick < Width) { fX = fPick - Width / 2f; fY = -Height / 2f; }
				else if (fPick < Width + Height) { fX = Width / 2f; fY = (fPick - Width) - Height / 2f; }
				else if (fPick < 2 * Width + Height) { fX = (Width / 2f) - (fPick - Width - Height); fY = Height / 2f; }
				else { fX = -Width / 2f; fY = (Height / 2f) - (fPick - 2 * Width - Height); }
			}
			else
			{
				fX = ((float)s_cRand.NextDouble() - 0.5f) * Width;
				fY = ((float)s_cRand.NextDouble() - 0.5f) * Height;
			}

			float fCos = (float)Math.Cos(Rotation), fSin = (float)Math.Sin(Rotation);
			Vector2 tOffset = new Vector2(fX * fCos - fY * fSin, fX * fSin + fY * fCos);

			return NewParticle(tOrigin + tOffset, ReleaseImpulse);
		}
	}
}
