using System;
using System.Collections.Generic;
using System.Text;
using Cap.Haptics.Client;
using Cap.Haptics.PatternTypes;
using NUnit.Framework;

namespace Cap.Haptics.Tests
{
	/// <summary>
	/// The §8 drift guard, tested with manufactured drift: the validator must catch every
	/// way the C# mirrors and a native manifest can disagree, and must tolerate the one
	/// disagreement that is compatible (a newer native side with appended values).
	/// </summary>
	public sealed class EnumManifestValidatorTests
	{
		[Test]
		public void AgreementValidatesClean()
		{
			Assert.That(EnumManifestValidator.Validate(BuildManifest()), Is.Null);
		}

		[Test]
		public void KotlinNamingConventionIsAccepted()
		{
			// The AAR says IMPACT_LIGHT where C# says ImpactLight — normalization must bridge it.
			var manifest = BuildManifest(patterns: Entries(
				("SELECTION", 0), ("IMPACT_LIGHT", 1), ("IMPACT_MEDIUM", 2), ("IMPACT_HEAVY", 3),
				("SUCCESS", 4), ("WARNING", 5), ("ERROR", 6), ("RAMP_UP", 7),
				("HEARTBEAT", 8), ("LONG_PRESS", 9)));
			Assert.That(EnumManifestValidator.Validate(manifest), Is.Null);
		}

		[Test]
		public void IdDriftIsCaught()
		{
			var manifest = BuildManifest(patterns: Entries(
				("Selection", 99), ("ImpactLight", 1), ("ImpactMedium", 2), ("ImpactHeavy", 3),
				("Success", 4), ("Warning", 5), ("Error", 6), ("RampUp", 7),
				("Heartbeat", 8), ("LongPress", 9)));

			var problems = EnumManifestValidator.Validate(manifest);
			Assert.That(problems, Does.Contain("Selection is 0 in C# but 99"));
		}

		[Test]
		public void NameDriftIsCaught()
		{
			var manifest = BuildManifest(patterns: Entries(
				("Selektion", 0), ("ImpactLight", 1), ("ImpactMedium", 2), ("ImpactHeavy", 3),
				("Success", 4), ("Warning", 5), ("Error", 6), ("RampUp", 7),
				("Heartbeat", 8), ("LongPress", 9)));

			var problems = EnumManifestValidator.Validate(manifest);
			Assert.That(problems, Does.Contain("C# has Selection, the AAR does not"));
		}

		[Test]
		public void MissingSectionIsCaught()
		{
			var problems = EnumManifestValidator.Validate(BuildManifest(omitSection: "tiers"));
			Assert.That(problems, Does.Contain("tiers: missing from the manifest entirely"));
		}

		[Test]
		public void ExtraNativeEntriesAreTolerated()
		{
			// A newer AAR with an appended pattern is compatible: C# simply never sends it.
			var extra = new List<(string, int)>();
			foreach (HapticPattern value in Enum.GetValues(typeof(HapticPattern)))
				extra.Add((value.ToString(), (int)value));
			extra.Add(("FUTURE_PATTERN", 99));

			Assert.That(EnumManifestValidator.Validate(
				BuildManifest(patterns: Entries(extra.ToArray()))), Is.Null);
		}

		[Test]
		public void EmptyAndGarbageFailWithAMessageNotAThrow()
		{
			Assert.That(EnumManifestValidator.Validate(""), Does.Contain("empty"));
			Assert.That(EnumManifestValidator.Validate("][ nope"), Is.Not.Null);
		}

		[Test]
		public void EveryProblemIsReportedNotJustTheFirst()
		{
			var manifest = BuildManifest(
				patterns: Entries(("Selection", 42)),
				omitSection: "results");

			var problems = EnumManifestValidator.Validate(manifest);
			Assert.That(problems, Does.Contain("Selection is 0 in C# but 42"));
			Assert.That(problems, Does.Contain("C# has ImpactLight, the AAR does not"));
			Assert.That(problems, Does.Contain("results: missing from the manifest entirely"));
		}

		// MARK: helpers — a manifest generated from the C# enums themselves (agreement by
		// construction), with per-section overrides to manufacture each kind of drift.

		private static string Entries(params (string name, int id)[] entries)
		{
			var sb = new StringBuilder("[");
			for (var i = 0; i < entries.Length; i++)
			{
				if (i > 0)
					sb.Append(',');
				sb.Append("{\"name\":\"").Append(entries[i].name)
					.Append("\",\"id\":").Append(entries[i].id).Append('}');
			}
			return sb.Append(']').ToString();
		}

		private static string BuildManifest(
			string? patterns = null, string? omitSection = null)
		{
			var sections = new Dictionary<string, string?>
			{
				["patterns"] = patterns ?? FromEnum<HapticPattern>(),
				["primitives"] = FromEnum<HapticPrimitive>(),
				["effects"] = FromEnum<PredefinedEffect>(),
				["viewFeedback"] = FromEnum<ViewFeedback>(),
				["tiers"] = FromEnum<HapticTier>(),
				["results"] = FromEnum<HapticResult>(),
			};
			if (omitSection != null)
				sections.Remove(omitSection);

			var sb = new StringBuilder("{\"bridgeVersion\":2");
			foreach (var section in sections)
				sb.Append(",\"").Append(section.Key).Append("\":").Append(section.Value);
			return sb.Append('}').ToString();
		}

		private static string FromEnum<TEnum>() where TEnum : struct, Enum
		{
			var entries = new List<(string, int)>();
			foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
				entries.Add((value.ToString(), Convert.ToInt32(value)));
			return Entries(entries.ToArray());
		}
	}
}
