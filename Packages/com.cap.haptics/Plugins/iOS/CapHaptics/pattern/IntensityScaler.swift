import Foundation

/// The §3.2 rule, ported: below roughly a third of full strength the Taptic Engine
/// produces nothing a hand reliably detects, so the 0..1 dial maps onto
/// [floor, 1] through a compressive curve — it fades instead of falling off a cliff.
/// Consequence: intensity 0 is the weakest perceptible setting, not silence.
enum IntensityScaler {

	static let floor: Float = 0.3

	/// NaN and out-of-range input clamp rather than reject: this sits on the playback
	/// hot path where "play something sensible" beats "return an error".
	static func multiplier(for intensity: Float) -> Float {
		let clamped = intensity.isNaN ? 1 : max(0, min(1, intensity))
		return floor + (1 - floor) * sqrt(clamped)
	}
}
