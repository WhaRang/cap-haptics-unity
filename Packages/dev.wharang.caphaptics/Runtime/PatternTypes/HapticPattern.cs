namespace Cap.Haptics.PatternTypes
{
	/// <summary>
	/// The SDK's semantic vocabulary — ask for a <i>meaning</i> and the active native
	/// backend decides how to render it on the hardware in front of it.
	///
	/// This enum is the canonical definition, tied to no platform: the Kotlin side carries
	/// a copy that is validated against it via the AAR's enum manifest at init, and the
	/// iOS plugin renders the same ids under the bridge-version handshake. Values are the
	/// wire ids — append new patterns, never renumber.
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
