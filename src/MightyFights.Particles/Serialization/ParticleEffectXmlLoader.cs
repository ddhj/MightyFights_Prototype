// system includes
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

// 3rd party includes
using Microsoft.Xna.Framework;

// project includes
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;

namespace ProjectMercury.Serialization
{
	//// PORT (Phase 4, T4.1): parser for the 15 Content/Particles/*.xml files (the plan's
	//// preferred strategy -- direct parse into shim types, no intermediate JSON conversion
	//// artifact). XNA's IntermediateSerializer format declares polymorphic Item types two
	//// different ways across these files -- fully qualified
	//// (Type="ProjectMercury.Emitters.CircleEmitter") or via an xmlns-declared alias
	//// (xmlns:Emitters="ProjectMercury.Emitters", Type="Emitters:CircleEmitter") -- and some
	//// Items omit the Type attribute entirely, meaning the base Emitter/Modifier class. All
	//// three forms are ground-truthed against the actual files, not assumed -- see
	//// docs/PORT_NOTES.md Phase 4 for the corrected inventory (the original Phase 0 grep only
	//// caught the fully-qualified form and missed 22 of 35 emitters / a third of the modifier
	//// types as a result).
	public static class ParticleEffectXmlLoader
	{
		public static ParticleEffect Load(string sXmlPath)
		{
			XDocument cDoc = XDocument.Load(sXmlPath);
			XElement cRoot = cDoc.Root;

			Dictionary<string, string> cNsAliases = cRoot.Attributes()
				.Where(a => a.IsNamespaceDeclaration)
				.ToDictionary(a => a.Name.LocalName, a => a.Value);

			XElement cAsset = cRoot.Element("Asset");
			ParticleEffect cEffect = new ParticleEffect
			{
				Name = (string)cAsset.Element("Name"),
				Author = (string)cAsset.Element("Author"),
				Description = (string)cAsset.Element("Description"),
			};

			foreach (XElement cItem in cAsset.Elements("Item"))
				cEffect.Add(ParseEmitter(cItem, cNsAliases));

			return cEffect;
		}

		//// Only the trailing class name is used to pick the C# type -- the alias's resolved
		//// namespace URI isn't cross-checked against "ProjectMercury.Emitters"/"...Modifiers",
		//// since every file in this game consistently declares those two aliases the same way
		//// and the class name alone is unambiguous within this fixed, small set of 15 files.
		static string ResolveSimpleClassName(string sTypeAttr)
		{
			if (string.IsNullOrEmpty(sTypeAttr))
				return null;

			string sAfterAlias = sTypeAttr.Contains(':') ? sTypeAttr.Split(':')[1] : sTypeAttr;
			int iLastDot = sAfterAlias.LastIndexOf('.');
			return iLastDot >= 0 ? sAfterAlias.Substring(iLastDot + 1) : sAfterAlias;
		}

		static Emitter ParseEmitter(XElement cItem, Dictionary<string, string> cNsAliases)
		{
			string sClassName = ResolveSimpleClassName((string)cItem.Attribute("Type"));

			Emitter cEmitter = sClassName switch
			{
				"CircleEmitter" => new CircleEmitter
				{
					Radius = FloatOf(cItem, "Radius"),
					Ring = BoolOf(cItem, "Ring"),
					Radiate = BoolOf(cItem, "Radiate"),
				},
				"ConeEmitter" => new ConeEmitter
				{
					Direction = FloatOf(cItem, "Direction"),
					ConeAngle = FloatOf(cItem, "ConeAngle"),
				},
				"LineEmitter" => new LineEmitter
				{
					Length = FloatOf(cItem, "Length"),
					Angle = FloatOf(cItem, "Angle"),
					Rectilinear = BoolOf(cItem, "Rectilinear"),
					EmitBothWays = BoolOf(cItem, "EmitBothWays"),
				},
				"RectEmitter" => new RectEmitter
				{
					Width = FloatOf(cItem, "Width"),
					Height = FloatOf(cItem, "Height"),
					Rotation = FloatOf(cItem, "Rotation"),
					Frame = BoolOf(cItem, "Frame"),
				},
				_ => new Emitter(),
			};

			cEmitter.Name = (string)cItem.Element("Name");
			cEmitter.ParticleTextureAssetName = (string)cItem.Element("ParticleTextureAssetName");
			cEmitter.Term = FloatOf(cItem, "Term");
			cEmitter.Enabled = BoolOf(cItem, "Enabled", true);
			cEmitter.Budget = IntOf(cItem, "Budget", 100);
			cEmitter.ReleaseQuantity = IntOf(cItem, "ReleaseQuantity", 1);
			cEmitter.MinimumTriggerPeriod = FloatOf(cItem, "MinimumTriggerPeriod");
			cEmitter.BlendMode = (string)cItem.Element("BlendMode") ?? "Alpha";
			cEmitter.TriggerOffset = Vector2Of(cItem, "TriggerOffset");
			cEmitter.ReleaseImpulse = Vector2Of(cItem, "ReleaseImpulse");
			cEmitter.ReleaseSpeed = FloatRangeOf(cItem, "ReleaseSpeed");
			cEmitter.ReleaseColour = ColourRangeOf(cItem, "ReleaseColour");
			cEmitter.ReleaseOpacity = FloatRangeOf(cItem, "ReleaseOpacity", 1f);
			cEmitter.ReleaseScale = FloatRangeOf(cItem, "ReleaseScale", 1f);
			cEmitter.ReleaseRotation = FloatRangeOf(cItem, "ReleaseRotation");

			XElement cModsElem = cItem.Element("Modifiers");
			if (cModsElem != null)
				foreach (XElement cModItem in cModsElem.Elements("Item"))
					cEmitter.Modifiers.Add(ParseModifier(cModItem));

			return cEmitter;
		}

		static Modifier ParseModifier(XElement cItem)
		{
			string sClassName = ResolveSimpleClassName((string)cItem.Attribute("Type"));

			return sClassName switch
			{
				"ColourModifier" => new ColourModifier
				{
					InitialColour = Vector3Of(cItem, "InitialColour"),
					UltimateColour = Vector3Of(cItem, "UltimateColour"),
				},
				"ColourInterpolatorModifier" => new ColourInterpolatorModifier
				{
					InitialColour = Vector3Of(cItem, "InitialColour"),
					MiddleColour = Vector3Of(cItem, "MiddleColour"),
					FinalColour = Vector3Of(cItem, "FinalColour"),
					MiddlePosition = FloatOf(cItem, "MiddlePosition"),
				},
				"DampingModifier" => new DampingModifier { DampingCoefficient = FloatOf(cItem, "DampingCoefficient") },
				"HueShiftModifier" => new HueShiftModifier { HueShift = FloatOf(cItem, "HueShift") },
				"LinearGravityModifier" => new LinearGravityModifier { Gravity = Vector2Of(cItem, "Gravity") },
				"OpacityFastFadeModifier" => new OpacityFastFadeModifier(),
				"OpacityInterpolatorModifier" => new OpacityInterpolatorModifier
				{
					InitialOpacity = FloatOf(cItem, "InitialOpacity"),
					MiddleOpacity = FloatOf(cItem, "MiddleOpacity"),
					FinalOpacity = FloatOf(cItem, "FinalOpacity"),
					MiddlePosition = FloatOf(cItem, "MiddlePosition"),
				},
				"OpacityModifier" => new OpacityModifier
				{
					Initial = FloatOf(cItem, "Initial"),
					Ultimate = FloatOf(cItem, "Ultimate"),
				},
				"OpacityOscillator" => new OpacityOscillator
				{
					Frequency = FloatOf(cItem, "Frequency"),
					MinimumOpacity = FloatOf(cItem, "MinimumOpacity"),
					MaximumOpacity = FloatOf(cItem, "MaximumOpacity", 1f),
				},
				"RotationModifier" => new RotationModifier { RotationRate = FloatOf(cItem, "RotationRate") },
				"RotationRateModifier" => new RotationRateModifier
				{
					InitialRate = FloatOf(cItem, "InitialRate"),
					FinalRate = FloatOf(cItem, "FinalRate"),
				},
				"ScaleInterpolatorModifier" => new ScaleInterpolatorModifier
				{
					InitialScale = FloatOf(cItem, "InitialScale"),
					MiddleScale = FloatOf(cItem, "MiddleScale"),
					FinalScale = FloatOf(cItem, "FinalScale"),
					MiddlePosition = FloatOf(cItem, "MiddlePosition"),
				},
				"ScaleMergeModifier" => new ScaleMergeModifier { MergeScale = FloatOf(cItem, "MergeScale") },
				"ScaleModifier" => new ScaleModifier
				{
					InitialScale = FloatOf(cItem, "InitialScale"),
					UltimateScale = FloatOf(cItem, "UltimateScale"),
				},
				"TrajectoryRotationModifier" => new TrajectoryRotationModifier { RotationOffset = FloatOf(cItem, "RotationOffset") },
				"VelocityClampModifier" => new VelocityClampModifier { MaximumVelocity = FloatOf(cItem, "MaximumVelocity") },
				_ => throw new NotSupportedException($"Unrecognized particle modifier type '{sClassName}' -- add it to ParticleEffectXmlLoader.ParseModifier (see docs/PORT_NOTES.md Phase 4 inventory)."),
			};
		}

		// --- field parsing helpers ---

		static float FloatOf(XElement cParent, string sName, float fDefault = 0f)
		{
			XElement cElem = cParent.Element(sName);
			return cElem == null ? fDefault : float.Parse(cElem.Value, CultureInfo.InvariantCulture);
		}

		static int IntOf(XElement cParent, string sName, int iDefault = 0)
		{
			XElement cElem = cParent.Element(sName);
			return cElem == null ? iDefault : int.Parse(cElem.Value, CultureInfo.InvariantCulture);
		}

		static bool BoolOf(XElement cParent, string sName, bool bDefault = false)
		{
			XElement cElem = cParent.Element(sName);
			return cElem == null ? bDefault : bool.Parse(cElem.Value);
		}

		static Vector2 Vector2Of(XElement cParent, string sName)
		{
			XElement cElem = cParent.Element(sName);
			if (cElem == null)
				return Vector2.Zero;
			float[] cParts = ParseFloats(cElem.Value);
			return new Vector2(cParts[0], cParts[1]);
		}

		static Vector3 Vector3Of(XElement cParent, string sName)
		{
			XElement cElem = cParent.Element(sName);
			if (cElem == null)
				return Vector3.Zero;
			float[] cParts = ParseFloats(cElem.Value);
			return new Vector3(cParts[0], cParts[1], cParts[2]);
		}

		static FloatRange FloatRangeOf(XElement cParent, string sName, float fDefaultValue = 0f)
		{
			XElement cElem = cParent.Element(sName);
			if (cElem == null)
				return new FloatRange { Value = fDefaultValue };
			return new FloatRange
			{
				Value = FloatOf(cElem, "Value", fDefaultValue),
				Variation = FloatOf(cElem, "Variation"),
			};
		}

		static ColourRange ColourRangeOf(XElement cParent, string sName)
		{
			XElement cElem = cParent.Element(sName);
			if (cElem == null)
				return new ColourRange { Value = Vector3.One };
			return new ColourRange
			{
				Value = Vector3Of(cElem, "Value"),
				Variation = Vector3Of(cElem, "Variation"),
			};
		}

		static float[] ParseFloats(string sValue)
		{
			return sValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => float.Parse(s, CultureInfo.InvariantCulture))
				.ToArray();
		}
	}
}
