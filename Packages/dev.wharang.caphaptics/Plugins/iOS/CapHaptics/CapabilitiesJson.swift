import Foundation

/// Serialises the capability snapshot for the C# side — the `CapabilitiesJson.kt` twin.
/// Same keys, same shapes, byte-for-byte the vocabulary `HapticCapabilities.FromJson`
/// already parses; the C# side needs zero changes for iOS.
///
/// Hand-rolled string building on purpose: every value is produced by this file from
/// static names and numbers, nothing needs escaping, and the output stays deterministic
/// and diffable in tests.
enum CapabilitiesJson {

	/// Wire ids of the C# `PredefinedEffect` enum, Kotlin-manifest names.
	static let effects: [(name: String, id: Int32)] = [
		("TICK", 0), ("CLICK", 1), ("DOUBLE_CLICK", 2), ("HEAVY_CLICK", 3),
	]

	/// Wire ids of the C# `HapticPrimitive` enum, Kotlin-manifest names.
	static let primitives: [(name: String, id: Int32)] = [
		("CLICK", 0), ("TICK", 1), ("QUICK_RISE", 2), ("SLOW_RISE", 3),
		("QUICK_FALL", 4), ("LOW_TICK", 5), ("THUD", 6), ("SPIN", 7),
	]

	static func notInitialized() -> String {
		"{\"bridgeVersion\":\(BridgeVersion.current),\"initialized\":false}"
	}

	static func of(_ caps: DeviceCapabilities, deviceTier: Int32, activeTier: Int32) -> String {
		// Generators are OEM-tuned with no query API: certain on Core Haptics hardware,
		// "fire and hope" on older iPhones, absent everywhere else.
		let effectSupport: SupportLevel =
			caps.supportsCoreHaptics ? .yes : (caps.isPhone ? .unknown : .no)
		// Primitives are synthesized from CH events — no per-primitive hardware lottery
		// on iOS: all or nothing. Durations come with the I3 synthesis table.
		let primitiveSupport: SupportLevel = caps.supportsCoreHaptics ? .yes : .no

		let effectsJson = effects
			.map { "{\"name\":\"\($0.name)\",\"id\":\($0.id),\"support\":\"\(effectSupport.rawValue)\"}" }
			.joined(separator: ",")
		let primitivesJson = primitives
			.map {
				let duration = caps.supportsCoreHaptics
					? PrimitiveSynthesis.durationMs(forPrimitive: $0.id) : -1
				return "{\"name\":\"\($0.name)\",\"id\":\($0.id)," +
					"\"support\":\"\(primitiveSupport.rawValue)\",\"durationMs\":\(duration)}"
			}
			.joined(separator: ",")

		return "{\"bridgeVersion\":\(BridgeVersion.current)," +
			"\"initialized\":true," +
			"\"sdkInt\":\(caps.sdkMajor)," +
			"\"hasVibrator\":\(deviceTier > 0)," +
			// Only Core Haptics has a real amplitude channel; generator intensity is
			// a coarse style hint, not amplitude control.
			"\"hasAmplitudeControl\":\(caps.supportsCoreHaptics)," +
			"\"vibratorCount\":\(deviceTier > 0 ? 1 : 0)," +
			"\"deviceTier\":\(deviceTier)," +
			"\"activeTier\":\(activeTier)," +
			// No View-feedback channel exists on iOS.
			"\"viewFeedbackAvailable\":false," +
			// The Settings → Sounds & Haptics switch is not queryable; the engine's
			// stopped-reason upgrades this to a logged hint at playback time, not here.
			"\"systemHapticsEnabled\":\"UNKNOWN\"," +
			"\"effects\":[\(effectsJson)]," +
			"\"primitives\":[\(primitivesJson)]}"
	}
}
