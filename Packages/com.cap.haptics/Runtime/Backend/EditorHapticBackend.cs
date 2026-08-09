using Cap.Haptics.Client;
using Cap.Haptics.PatternTypes;
using UnityEngine;

namespace Cap.Haptics.Backend
{
	/// <summary>
	/// L1 stub for the Editor and every non-Android platform: logs instead of vibrating,
	/// succeeds at everything, and reports the bridge version this C# was written against
	/// so init always passes.
	///
	/// This is what makes the SDK safe to call from anywhere — pressing Play in the Editor
	/// must never throw, and a game system calling <see cref="Haptics"/> should not have to
	/// care which platform it is on. SDKs that only work on-device are painful SDKs.
	/// </summary>
	internal sealed class EditorHapticBackend : IHapticBackend
	{
		private bool _verbose;

		public int GetBridgeVersion() => Client.Haptics.ExpectedBridgeVersion;

		public bool Initialize(bool verboseLogging)
		{
			_verbose = verboseLogging;
			if (_verbose)
				Debug.Log("[cap-haptics] Editor stub active — calls are logged, nothing vibrates.");
			return true;
		}

		/// <summary>
		/// An honest snapshot of what the Editor is: initialized, no vibrator, tier None.
		/// The diagnostics panel renders a real "nothing here" state instead of pretending
		/// to be a device it has never met.
		/// </summary>
		public string GetCapabilitiesJson() =>
			"{\"bridgeVersion\":" + Client.Haptics.ExpectedBridgeVersion + "," +
			"\"initialized\":true,\"sdkInt\":0,\"hasVibrator\":false," +
			"\"hasAmplitudeControl\":false,\"vibratorCount\":0," +
			"\"deviceTier\":0,\"activeTier\":0,\"viewFeedbackAvailable\":false," +
			"\"systemHapticsEnabled\":\"UNKNOWN\",\"effects\":[],\"primitives\":[]}";

		/// <summary>
		/// Generated from the local C# enums, so validation trivially passes: there is no
		/// AAR here for the mirrors to drift from, and failing Editor init over a manifest
		/// the Editor cannot have would make the stub useless.
		/// </summary>
		public string GetEnumManifestJson()
		{
			var sb = new System.Text.StringBuilder();
			sb.Append("{\"bridgeVersion\":").Append(Client.Haptics.ExpectedBridgeVersion);
			AppendEnum<HapticPattern>(sb, "patterns");
			AppendEnum<HapticPrimitive>(sb, "primitives");
			AppendEnum<PredefinedEffect>(sb, "effects");
			AppendEnum<ViewFeedback>(sb, "viewFeedback");
			AppendEnum<HapticTier>(sb, "tiers");
			AppendEnum<HapticResult>(sb, "results");
			sb.Append('}');
			return sb.ToString();
		}

		public int PlayPattern(int patternId, float intensity)
		{
			if (_verbose)
				Debug.Log($"[cap-haptics] Editor stub: PlayPattern({(HapticPattern)patternId}, {intensity:0.00})");
			return (int)HapticResult.Ok;
		}

		public int SetForcedTier(int tierLevel)
		{
			if (_verbose)
				Debug.Log($"[cap-haptics] Editor stub: SetForcedTier({tierLevel}) — no tiers here, staying None");
			return (int)HapticTier.None;
		}

		public int PlayWaveform(long[] timingsMs, int[] amplitudes, int repeatIndex)
		{
			if (_verbose)
				Debug.Log($"[cap-haptics] Editor stub: PlayWaveform([{string.Join(",", timingsMs)}], " +
					$"[{string.Join(",", amplitudes)}], repeat={repeatIndex})");
			return (int)HapticResult.Ok;
		}

		public void Cancel()
		{
			if (_verbose)
				Debug.Log("[cap-haptics] Editor stub: Cancel()");
		}

		public void Dispose()
		{
		}

		private static void AppendEnum<TEnum>(System.Text.StringBuilder sb, string label)
			where TEnum : struct, System.Enum
		{
			sb.Append(",\"").Append(label).Append("\":[");
			var first = true;
			foreach (TEnum value in System.Enum.GetValues(typeof(TEnum)))
			{
				if (!first)
					sb.Append(',');
				first = false;
				sb.Append("{\"name\":\"").Append(value.ToString())
					.Append("\",\"id\":").Append(System.Convert.ToInt32(value)).Append('}');
			}
			sb.Append(']');
		}
	}
}
