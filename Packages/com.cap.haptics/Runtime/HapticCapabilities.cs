using System;
using UnityEngine;

namespace Cap.Haptics
{
	/// <summary>
	/// C# mirror of the capability snapshot the native SDK probed at init — what the device
	/// in front of us can actually do, and which tier that landed it on.
	///
	/// Parsed once per session from <c>getCapabilitiesJson()</c> with
	/// <see cref="JsonUtility"/>, which is why the serialized fields below are mutable,
	/// lowerCamelCase, and string-typed where the wire says so: they must match the JSON
	/// keys byte for byte, and JsonUtility has no enum-from-string or nullable support.
	/// Consume the typed properties, not the fields.
	/// </summary>
	[Serializable]
	public sealed class HapticCapabilities
	{
#pragma warning disable 0649 // assigned by JsonUtility via reflection
		[SerializeField] private int bridgeVersion;
		[SerializeField] private bool initialized;
		[SerializeField] private int sdkInt;
		[SerializeField] private bool hasVibrator;
		[SerializeField] private bool hasAmplitudeControl;
		[SerializeField] private int vibratorCount;
		[SerializeField] private int deviceTier;
		[SerializeField] private int activeTier;
		[SerializeField] private bool viewFeedbackAvailable;
		[SerializeField] private string systemHapticsEnabled = "UNKNOWN";
		[SerializeField] private EffectEntry[] effects = Array.Empty<EffectEntry>();
		[SerializeField] private PrimitiveEntry[] primitives = Array.Empty<PrimitiveEntry>();
#pragma warning restore 0649

		public int BridgeVersion => bridgeVersion;

		/// <summary>False when the blob was produced before native init completed.</summary>
		public bool Initialized => initialized;

		/// <summary>Android API level of the device.</summary>
		public int SdkInt => sdkInt;

		public bool HasVibrator => hasVibrator;
		public bool HasAmplitudeControl => hasAmplitudeControl;
		public int VibratorCount => vibratorCount;

		/// <summary>What this device could do if nothing were forced.</summary>
		public HapticTier DeviceTier => (HapticTier)deviceTier;

		/// <summary>What is actually playing back — lower than <see cref="DeviceTier"/> when a tier is forced.</summary>
		public HapticTier ActiveTier => (HapticTier)activeTier;

		public bool ViewFeedbackAvailable => viewFeedbackAvailable;

		/// <summary>
		/// The user's system-wide haptics preference. <see cref="SupportLevel.Unknown"/>
		/// means unreadable, which is not the same answer as switched off — and either way
		/// OEM intensity sliders can still silence output this cannot see.
		/// </summary>
		public SupportLevel SystemHapticsEnabled =>
			SupportLevelExtensions.ParseSupportLevel(systemHapticsEnabled);

		/// <summary>Per-effect support for the T2 predefined effects.</summary>
		public EffectEntry[] Effects => effects;

		/// <summary>Per-primitive support (and measured duration) for the T3 primitives.</summary>
		public PrimitiveEntry[] Primitives => primitives;

		/// <summary>Null when the JSON is empty or unparseable — callers get honesty, not an exception.</summary>
		public static HapticCapabilities? FromJson(string json)
		{
			if (string.IsNullOrEmpty(json))
				return null;

			try
			{
				return JsonUtility.FromJson<HapticCapabilities>(json);
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] Could not parse capabilities JSON: {e.Message}");
				return null;
			}
		}

		[Serializable]
		public sealed class EffectEntry
		{
#pragma warning disable 0649
			[SerializeField] private string name = "";
			[SerializeField] private int id;
			[SerializeField] private string support = "UNKNOWN";
#pragma warning restore 0649

			public string Name => name;
			public int Id => id;
			public SupportLevel Support => SupportLevelExtensions.ParseSupportLevel(support);
		}

		[Serializable]
		public sealed class PrimitiveEntry
		{
#pragma warning disable 0649
			[SerializeField] private string name = "";
			[SerializeField] private int id;
			[SerializeField] private string support = "UNKNOWN";
			[SerializeField] private int durationMs = -1;
#pragma warning restore 0649

			public string Name => name;
			public int Id => id;
			public SupportLevel Support => SupportLevelExtensions.ParseSupportLevel(support);

			/// <summary>Hardware-measured duration, or -1 when the platform could not say.</summary>
			public int DurationMs => durationMs;
		}
	}
}
