namespace CapHaptics.Client
{
	/// <summary>
	/// Outcome of a playback call. Nothing in this SDK throws — every failure comes back as
	/// one of these instead. This enum is the canonical definition; values are the wire
	/// codes, validated against the AAR's enum manifest at init. Append new codes, never
	/// renumber.
	/// </summary>
	public enum HapticResult
	{
		/// <summary>The platform accepted the effect. <b>Not</b> a promise the user felt it —
		/// system settings and OEM sliders can still suppress output silently.</summary>
		Ok = 0,

		/// <summary><see cref="Haptics.Initialize"/> was never called, or it failed.</summary>
		NotInitialized = 1,

		/// <summary>Device has no vibrator. Playback is a well-behaved no-op.</summary>
		NoVibrator = 2,

		/// <summary>The pattern has no rendering at the active tier. Should be unreachable.</summary>
		UnsupportedPattern = 3,

		/// <summary>The call carried something the platform would have rejected.</summary>
		InvalidArgument = 4,

		/// <summary>The platform call itself failed; detail is in the native log.</summary>
		PlatformError = 5,

		/// <summary>The system declined to play it — almost always the user's haptics are off.
		/// Only the view-feedback channel can report this, which makes it the single most
		/// useful code here: without it, "I felt nothing" looks identical to a bug.</summary>
		Suppressed = 6,

		/// <summary>App-level mute: <see cref="Haptics.Enabled"/> is false. Rejected in C#
		/// before reaching the device — never produced natively. Distinct from
		/// <see cref="Suppressed"/>, which reports the user's <i>system</i> setting.</summary>
		Disabled = 7,
	}

	public static class HapticResultExtensions
	{
		public static bool IsSuccess(this HapticResult result) => result == HapticResult.Ok;

		public static HapticResult FromCode(int code) =>
			code is >= 0 and <= 7 ? (HapticResult)code : HapticResult.PlatformError;
	}
}
