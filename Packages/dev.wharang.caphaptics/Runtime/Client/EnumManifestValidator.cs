using System;
using System.Collections.Generic;
using System.Text;
using Cap.Haptics.PatternTypes;
using UnityEngine;

namespace Cap.Haptics.Client
{
	/// <summary>
	/// Validates the C# enum mirrors against the enum manifest the packaged AAR reports —
	/// the PLAN §8 answer to "two hand-written enums always drift eventually".
	///
	/// Runs once at init, on the device, against the AAR actually installed: exactly where a
	/// mismatch would otherwise bite as the wrong pattern silently playing. Rules:
	/// every C# value must exist in the manifest with the same wire id (names are compared
	/// ignoring case and underscores, since Kotlin says <c>IMPACT_LIGHT</c> and C# says
	/// <c>ImpactLight</c>). Extra manifest entries are fine — a newer AAR with appended
	/// values is compatible; this C# simply never sends them.
	/// </summary>
	internal static class EnumManifestValidator
	{
		/// <summary>Null when everything matches; otherwise a human-readable list of every disagreement.</summary>
		public static string? Validate(string manifestJson)
		{
			if (string.IsNullOrEmpty(manifestJson))
				return "enum manifest is empty — getEnumManifestJson failed on the native side";

			ManifestDto manifest;
			try
			{
				manifest = JsonUtility.FromJson<ManifestDto>(manifestJson);
			}
			catch (Exception e)
			{
				return $"enum manifest is unparseable: {e.Message}";
			}

			var problems = new StringBuilder();
			
			Check<HapticPattern>(manifest.patterns, "patterns", problems);
			Check<HapticPrimitive>(manifest.primitives, "primitives", problems);
			Check<PredefinedEffect>(manifest.effects, "effects", problems);
			Check<ViewFeedback>(manifest.viewFeedback, "viewFeedback", problems);
			Check<HapticTier>(manifest.tiers, "tiers", problems);
			Check<HapticResult>(manifest.results, "results", problems);

			return problems.Length == 0 ? null : problems.ToString().TrimEnd();
		}

		private static void Check<TEnum>(Entry[]? wire, string label, StringBuilder problems)
			where TEnum : struct, Enum
		{
			if (wire == null || wire.Length == 0)
			{
				problems.AppendLine($"{label}: missing from the manifest entirely");
				return;
			}

			var byName = new Dictionary<string, int>(wire.Length);
			foreach (var entry in wire)
				byName[Normalize(entry.name)] = entry.id;

			foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
			{
				var name = value.ToString();
				if (!byName.TryGetValue(Normalize(name), out var wireId))
				{
					problems.AppendLine($"{label}: C# has {name}, the AAR does not");
				}
				else if (wireId != Convert.ToInt32(value))
				{
					problems.AppendLine(
						$"{label}: {name} is {Convert.ToInt32(value)} in C# but {wireId} in the AAR");
				}
			}
		}

		private static string Normalize(string name) => name.Replace("_", "").ToUpperInvariant();

#pragma warning disable 0649 // assigned by JsonUtility via reflection
		[Serializable]
		private sealed class ManifestDto
		{
			public int bridgeVersion;
			public Entry[]? patterns;
			public Entry[]? primitives;
			public Entry[]? effects;
			public Entry[]? viewFeedback;
			public Entry[]? tiers;
			public Entry[]? results;
		}

		[Serializable]
		private sealed class Entry
		{
			public string name = "";
			public int id;
		}
#pragma warning restore 0649
	}
}
