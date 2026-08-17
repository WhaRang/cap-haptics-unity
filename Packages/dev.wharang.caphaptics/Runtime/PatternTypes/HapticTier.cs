namespace CapHaptics
{
	/// <summary>
	/// The playback strategy the native SDK selected. Mirrors the Kotlin <c>HapticTier</c>
	/// enum — values are the wire form and must match its <c>level</c> field exactly
	/// (append-only, never renumber; validated against the enum manifest at init in U3).
	/// </summary>
	public enum HapticTier
	{
		/// <summary>No vibrator on this device (or the Editor stub). Playback is a well-behaved no-op.</summary>
		None = 0,

		/// <summary>T1 — <c>createWaveform</c>, amplitude only if the motor supports it. The API 26 floor.</summary>
		Waveform = 1,

		/// <summary>T2 — <c>createPredefined</c>, OEM-tuned constants. API 29+.</summary>
		Predefined = 2,

		/// <summary>T3 — <c>Composition</c>, crisp and hardware-tuned. API 30+ <b>and</b> primitive support.</summary>
		Composed = 3,
	}
}
