// system includes
using System;

// 3rd party includes
using Microsoft.Xna.Framework;

namespace ProjectMercury.Modifiers
{
	//// PORT (Phase 4, T4.1/T4.2): base class for the 16 modifier types actually present across
	//// the 15 particle XML files (inventoried by parsing, not guessed -- see
	//// docs/PORT_NOTES.md Phase 4 for the corrected count; the original Phase 0 inventory only
	//// found 12 types because it missed the XML-namespace-alias form some files use).
	//// Implementations are best-effort per the port plan's T4.4 ("roughly right look," not
	//// pixel-exact) -- exact real-Mercury math was never available to verify against (binary
	//// engine, upstream dead). Each modifier's field set IS ground-truthed against the XML,
	//// though; only the per-frame math is a plausible reconstruction.
	public abstract class Modifier
	{
		public abstract void Process(float fElapsedSeconds, ref Particle cParticle);
	}

	public class ColourModifier : Modifier
	{
		public Vector3 InitialColour { get; set; }
		public Vector3 UltimateColour { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			cParticle.Colour = Vector3.Lerp(InitialColour, UltimateColour, cParticle.NormalizedAge);
		}
	}

	public class ColourInterpolatorModifier : Modifier
	{
		public Vector3 InitialColour { get; set; }
		public Vector3 MiddleColour { get; set; }
		public Vector3 FinalColour { get; set; }
		public float MiddlePosition { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float t = cParticle.NormalizedAge;
			cParticle.Colour = t <= MiddlePosition
				? Vector3.Lerp(InitialColour, MiddleColour, MiddlePosition <= 0f ? 1f : t / MiddlePosition)
				: Vector3.Lerp(MiddleColour, FinalColour, MiddlePosition >= 1f ? 1f : (t - MiddlePosition) / (1f - MiddlePosition));
		}
	}

	public class DampingModifier : Modifier
	{
		public float DampingCoefficient { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float fScale = MathHelper.Clamp(1f - DampingCoefficient * fElapsedSeconds, 0f, 1f);
			cParticle.Velocity *= fScale;
		}
	}

	public class HueShiftModifier : Modifier
	{
		public float HueShift { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			// Approximate hue rotation via a simple RGB rotation matrix around the grey axis --
			// "roughly right look" per T4.4, not a colourimetrically exact HSV round-trip.
			float fAngle = HueShift * cParticle.NormalizedAge;
			float fCos = (float)Math.Cos(fAngle), fSin = (float)Math.Sin(fAngle);
			Vector3 c = cParticle.Colour;
			cParticle.Colour = new Vector3(
				c.X * fCos - c.Y * fSin,
				c.X * fSin + c.Y * fCos,
				c.Z);
		}
	}

	public class LinearGravityModifier : Modifier
	{
		public Vector2 Gravity { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			cParticle.Velocity += Gravity * fElapsedSeconds;
		}
	}

	public class OpacityFastFadeModifier : Modifier
	{
		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float t = cParticle.NormalizedAge;
			cParticle.Opacity = MathHelper.Clamp(1f - t * t, 0f, 1f);
		}
	}

	public class OpacityInterpolatorModifier : Modifier
	{
		public float InitialOpacity { get; set; }
		public float MiddleOpacity { get; set; }
		public float FinalOpacity { get; set; }
		public float MiddlePosition { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float t = cParticle.NormalizedAge;
			cParticle.Opacity = t <= MiddlePosition
				? MathHelper.Lerp(InitialOpacity, MiddleOpacity, MiddlePosition <= 0f ? 1f : t / MiddlePosition)
				: MathHelper.Lerp(MiddleOpacity, FinalOpacity, MiddlePosition >= 1f ? 1f : (t - MiddlePosition) / (1f - MiddlePosition));
		}
	}

	public class OpacityModifier : Modifier
	{
		public float Initial { get; set; }
		public float Ultimate { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			cParticle.Opacity = MathHelper.Lerp(Initial, Ultimate, cParticle.NormalizedAge);
		}
	}

	public class OpacityOscillator : Modifier
	{
		public float Frequency { get; set; }
		public float MinimumOpacity { get; set; }
		public float MaximumOpacity { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float fWave = ((float)Math.Sin(cParticle.Age * Frequency * MathHelper.TwoPi) + 1f) * 0.5f;
			cParticle.Opacity = MathHelper.Lerp(MinimumOpacity, MaximumOpacity, fWave);
		}
	}

	public class RotationModifier : Modifier
	{
		public float RotationRate { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			cParticle.Rotation += RotationRate * fElapsedSeconds;
		}
	}

	public class RotationRateModifier : Modifier
	{
		public float InitialRate { get; set; }
		public float FinalRate { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float fRate = MathHelper.Lerp(InitialRate, FinalRate, cParticle.NormalizedAge);
			cParticle.Rotation += fRate * fElapsedSeconds;
		}
	}

	public class ScaleInterpolatorModifier : Modifier
	{
		public float InitialScale { get; set; }
		public float MiddleScale { get; set; }
		public float FinalScale { get; set; }
		public float MiddlePosition { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float t = cParticle.NormalizedAge;
			cParticle.Scale = t <= MiddlePosition
				? MathHelper.Lerp(InitialScale, MiddleScale, MiddlePosition <= 0f ? 1f : t / MiddlePosition)
				: MathHelper.Lerp(MiddleScale, FinalScale, MiddlePosition >= 1f ? 1f : (t - MiddlePosition) / (1f - MiddlePosition));
		}
	}

	public class ScaleMergeModifier : Modifier
	{
		public float MergeScale { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			// "Merge" toward a common scale over time, exponential approach so repeated calls
			// (varying frame time) stay stable rather than compounding a fixed-step lerp.
			cParticle.Scale += (MergeScale - cParticle.Scale) * MathHelper.Clamp(fElapsedSeconds, 0f, 1f);
		}
	}

	public class ScaleModifier : Modifier
	{
		public float InitialScale { get; set; }
		public float UltimateScale { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			cParticle.Scale = MathHelper.Lerp(InitialScale, UltimateScale, cParticle.NormalizedAge);
		}
	}

	public class TrajectoryRotationModifier : Modifier
	{
		public float RotationOffset { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			if (cParticle.Velocity.LengthSquared() > 0.0001f)
				cParticle.Rotation = (float)Math.Atan2(cParticle.Velocity.Y, cParticle.Velocity.X) + RotationOffset;
		}
	}

	public class VelocityClampModifier : Modifier
	{
		public float MaximumVelocity { get; set; }

		public override void Process(float fElapsedSeconds, ref Particle cParticle)
		{
			float fSpeed = cParticle.Velocity.Length();
			if (fSpeed > MaximumVelocity && fSpeed > 0.0001f)
				cParticle.Velocity *= MaximumVelocity / fSpeed;
		}
	}
}
