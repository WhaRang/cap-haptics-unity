import Foundation

/// The §11.4 tier-3 column: semantic pattern wire id → CH event recipe. Starting-draft
/// values, tuned by feel on-device (the A5 session, iOS edition). Authored intensities
/// live above the perceptible floor by construction; the 0..1 dial multiplies through
/// `IntensityScaler`, it never re-authors.
enum PatternRegistry {

	/// Nil for an unknown wire id — the caller turns that into `unsupportedPattern`.
	static func specs(forPattern id: Int32) -> [EventSpec]? {
		switch id {
		case 0: // Selection
			return [.transient(time: 0, intensity: 0.4, sharpness: 0.6)]
		case 1: // ImpactLight
			return [.transient(time: 0, intensity: 0.4, sharpness: 0.5)]
		case 2: // ImpactMedium
			return [.transient(time: 0, intensity: 0.7, sharpness: 0.5)]
		case 3: // ImpactHeavy
			return [.transient(time: 0, intensity: 1.0, sharpness: 0.6)]
		case 4: // Success — short rise into a full-strength click
			return PrimitiveSynthesis.ramp(
				at: 0, durationMs: 60, from: 0.2, to: 0.5, sharpness: 0.4, steps: 3)
				+ [.transient(time: 0.06, intensity: 1.0, sharpness: 0.7)]
		case 5: // Warning — strong beat, weaker echo
			return [
				.transient(time: 0, intensity: 0.8, sharpness: 0.6),
				.transient(time: 0.12, intensity: 0.5, sharpness: 0.6),
			]
		case 6: // Error — three insistent beats
			return [
				.transient(time: 0, intensity: 0.9, sharpness: 0.7),
				.transient(time: 0.09, intensity: 0.9, sharpness: 0.7),
				.transient(time: 0.18, intensity: 0.9, sharpness: 0.7),
			]
		case 7: // RampUp — 400 ms swell
			return PrimitiveSynthesis.ramp(
				at: 0, durationMs: 400, from: 0.2, to: 1.0, sharpness: 0.5)
		case 8: // Heartbeat — two soft thuds, lub-dub
			return [
				.transient(time: 0, intensity: 0.8, sharpness: 0.25),
				.transient(time: 0.09, intensity: 0.5, sharpness: 0.25),
			]
		case 9: // LongPress
			return [.transient(time: 0, intensity: 0.6, sharpness: 0.4)]
		default:
			return nil
		}
	}
}
