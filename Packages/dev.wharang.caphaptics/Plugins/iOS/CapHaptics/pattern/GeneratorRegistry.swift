import Foundation

/// The §11.4 tier-2 column: what each semantic pattern (and predefined effect) becomes
/// when only `UIFeedbackGenerator` exists. Pure data — the backend converts to actual
/// generator calls.
///
/// Tier 2 is *richer* than Android T2 for the notification patterns — Success, Warning
/// and Error have native OEM-tuned renderings instead of falling to a waveform. Beats
/// are best-effort: generators cannot sequence precisely, and system load between
/// scheduled calls smears the rhythm — accepted and documented (§11.7).
enum GeneratorRendering: Equatable {
	case selection
	case notificationSuccess
	case notificationWarning
	case notificationError
	case beats([Beat])
}

/// One impact-generator hit. `intensity` is the iOS-13 `impactOccurred(intensity:)`
/// multiplier on the style's tuned strength — coarse, not an amplitude channel.
struct Beat: Equatable {
	let time: Double
	let style: BeatStyle
	let intensity: Float
}

enum BeatStyle: Equatable {
	case light, medium, heavy, soft, rigid
}

enum GeneratorRegistry {

	/// Nil for an unknown pattern wire id.
	static func rendering(forPattern id: Int32) -> GeneratorRendering? {
		switch id {
		case 0: return .selection
		case 1: return .beats([Beat(time: 0, style: .light, intensity: 1)])
		case 2: return .beats([Beat(time: 0, style: .medium, intensity: 1)])
		case 3: return .beats([Beat(time: 0, style: .heavy, intensity: 1)])
		case 4: return .notificationSuccess
		case 5: return .notificationWarning
		case 6: return .notificationError
		case 7: // RampUp — soft into rigid, the best two-point swell generators offer
			return .beats([
				Beat(time: 0, style: .soft, intensity: 0.6),
				Beat(time: 0.3, style: .rigid, intensity: 1),
			])
		case 8: // Heartbeat
			return .beats([
				Beat(time: 0, style: .heavy, intensity: 0.8),
				Beat(time: 0.09, style: .heavy, intensity: 0.5),
			])
		case 9: return .beats([Beat(time: 0, style: .medium, intensity: 1)])
		default: return nil
		}
	}

	/// PredefinedEffect wire id → generator hit(s). Effects play "exactly as tuned" —
	/// no intensity dial, mirroring the Android T2 contract.
	static func rendering(forEffect id: Int32) -> GeneratorRendering? {
		switch id {
		case 0: return .beats([Beat(time: 0, style: .light, intensity: 1)]) // TICK
		case 1: return .beats([Beat(time: 0, style: .medium, intensity: 1)]) // CLICK
		case 2: // DOUBLE_CLICK
			return .beats([
				Beat(time: 0, style: .medium, intensity: 1),
				Beat(time: 0.1, style: .medium, intensity: 1),
			])
		case 3: return .beats([Beat(time: 0, style: .heavy, intensity: 1)]) // HEAVY_CLICK
		default: return nil
		}
	}

	/// Degrades arbitrary tier-3 event lists (waveforms, compositions) to impact beats:
	/// events inside the merge window collapse into one beat carrying the strongest
	/// intensity — otherwise an 8-step ramp becomes an 8-hit drumroll. Style follows
	/// intensity; the cap defends the motor (and the ears) against pathological input.
	static func beats(
		approximating specs: [EventSpec], mergeWindow: Double = 0.06, maxBeats: Int = 16
	) -> [Beat] {
		let sorted = specs.sorted { $0.time < $1.time }
		var beats: [Beat] = []
		for spec in sorted {
			if let last = beats.last, spec.time - last.time < mergeWindow {
				if spec.intensity > last.intensity {
					beats[beats.count - 1] = Beat(
						time: last.time, style: style(for: spec.intensity), intensity: spec.intensity)
				}
				continue
			}
			if beats.count == maxBeats {
				break
			}
			beats.append(Beat(
				time: spec.time, style: style(for: spec.intensity), intensity: spec.intensity))
		}
		return beats
	}

	static func style(for intensity: Float) -> BeatStyle {
		if intensity < 0.45 {
			return .light
		}
		return intensity < 0.75 ? .medium : .heavy
	}
}
