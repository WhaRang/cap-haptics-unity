import Foundation

/// Pure tier arithmetic — no platform imports, unit-tested on the Mac.
///
/// Wire levels reuse `HapticTier`: 3 = Core Haptics, 2 = UIFeedbackGenerators,
/// 0 = no-op. **There is no iOS tier 1** (no arbitrary-waveform API below Core
/// Haptics), so a forced request for 1 lands on 2 — the lowest playable tier —
/// rather than silencing the device.
enum TierSelector {

	static func deviceTier(_ caps: DeviceCapabilities) -> Int32 {
		if caps.supportsCoreHaptics {
			return 3
		}
		if caps.isPhone {
			return 2
		}
		return 0
	}

	/// Forced requests are clamped to the device's natural tier, like Android.
	/// Negative = automatic selection.
	static func activeTier(deviceTier: Int32, forcedTier: Int32) -> Int32 {
		if forcedTier < 0 {
			return deviceTier
		}
		if forcedTier == 0 {
			return 0
		}
		let requested: Int32 = forcedTier <= 2 ? 2 : 3
		return min(requested, deviceTier)
	}
}
