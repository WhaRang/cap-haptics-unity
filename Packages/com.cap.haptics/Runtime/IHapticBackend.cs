using System;

namespace Cap.Haptics
{
	/// <summary>
	/// L1 — the platform seam. <see cref="Haptics"/> picks exactly one implementation at
	/// init: the JNI-backed <see cref="AndroidHapticBackend"/> on an Android device, the
	/// log-only <see cref="EditorHapticBackend"/> everywhere else, so every call site is
	/// safe to hit in the Editor without a device attached.
	///
	/// Grows one phase at a time (U2 capabilities, U3 patterns, U4 waveforms) — the U1
	/// surface is deliberately just enough to prove the toolchain end to end.
	/// </summary>
	internal interface IHapticBackend : IDisposable
	{
		/// <summary>
		/// The ABI version of the native bridge actually packaged in the build. Read
		/// <b>before</b> <see cref="Initialize"/> — a mismatch means the AARs in
		/// <c>Plugins/Android</c> are not the ones this C# was written against, and
		/// nothing else can be trusted.
		/// </summary>
		int GetBridgeVersion();

		/// <param name="verboseLogging">Routes native SDK logging to
		/// <c>adb logcat -s CapHaptics:V</c>.</param>
		bool Initialize(bool verboseLogging);

		/// <summary>
		/// The capability snapshot as JSON — read once per session after
		/// <see cref="Initialize"/> and parsed by <see cref="HapticCapabilities.FromJson"/>.
		/// Empty string on failure. JSON is fine here and nowhere else: this feeds a
		/// diagnostics panel, not the per-call playback path.
		/// </summary>
		string GetCapabilitiesJson();

		/// <summary>
		/// Every marshalled enum with its wire id, for
		/// <see cref="EnumManifestValidator"/> to check the C# mirrors against at init.
		/// </summary>
		string GetEnumManifestJson();

		/// <summary>The hot path: wire id + intensity in, result code out. Primitives only —
		/// no strings, no allocation, this runs per vibration.</summary>
		int PlayPattern(int patternId, float intensity);

		/// <param name="tierLevel">A <see cref="HapticTier"/> level, or negative for automatic
		/// selection. Requests above the device's natural tier are clamped.</param>
		/// <returns>The tier actually in effect afterwards.</returns>
		int SetForcedTier(int tierLevel);

		/// <param name="timingsMs">Alternating off/on segment durations, starting with off.</param>
		/// <param name="amplitudes">Per-segment amplitudes 0..255, or empty for "no amplitude
		/// information" (a null array across JNI is more trouble than it is worth).</param>
		/// <param name="repeatIndex">-1 for no repeat. A repeating waveform runs until
		/// <see cref="Cancel"/>.</param>
		int PlayWaveform(long[] timingsMs, int[] amplitudes, int repeatIndex);

		/// <summary>Stops anything currently playing.</summary>
		void Cancel();
	}
}
