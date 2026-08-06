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
		Selection = 0,

		ImpactLight = 1,
		ImpactMedium = 2,
		ImpactHeavy = 3,

		Success = 4,
		Warning = 5,
		Error = 6,

		RampUp = 7,

		Heartbeat = 8,

		LongPress = 9,
	}
}
