namespace Cap.Haptics
{
	/// <summary>
	/// The SDK's semantic vocabulary — ask for a <i>meaning</i> and the native library
	/// decides how to render it on the hardware in front of it.
	///
	/// Mirrors the Kotlin <c>HapticPattern</c> enum; values are the wire ids, validated
	/// against the AAR's enum manifest at init. Append new patterns, never renumber.
	/// </summary>
	public enum HapticPattern
	{
		/// <summary>Moving through discrete options: a picker detent, a list snap. The lightest thing.</summary>
		Selection = 0,

		ImpactLight = 1,
		ImpactMedium = 2,
		ImpactHeavy = 3,

		/// <summary>Affirmative, rising.</summary>
		Success = 4,

		/// <summary>Attention-seeking but not final.</summary>
		Warning = 5,

		/// <summary>Final and unwelcome: three insistent beats.</summary>
		Error = 6,

		/// <summary>A swelling envelope. Loses the most on weaker hardware.</summary>
		RampUp = 7,

		Heartbeat = 8,

		/// <summary>Routed through the system view-feedback channel, so it obeys the user's settings.</summary>
		LongPress = 9,
	}
}
