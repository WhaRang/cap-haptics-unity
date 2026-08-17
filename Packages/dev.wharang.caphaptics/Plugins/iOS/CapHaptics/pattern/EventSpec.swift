import Foundation

/// One Core Haptics event, as pure data — times in seconds, intensity/sharpness 0..1.
/// The registry, synthesis and waveform code all speak EventSpec; only
/// `CoreHapticsBackend` converts to `CHHapticEvent`, which is what keeps every
/// rendering decision testable on the Mac.
///
/// Ramps are authored as stepped runs of short `continuous` events rather than
/// CHHapticParameterCurve: curves are pattern-global multipliers, so a rise ending at
/// 0.5 would also halve a transient scheduled at its end — steps compose, curves don't.
enum EventSpec: Equatable {
	case transient(time: Double, intensity: Float, sharpness: Float)
	case continuous(time: Double, duration: Double, intensity: Float, sharpness: Float)

	var time: Double {
		switch self {
		case .transient(let t, _, _): return t
		case .continuous(let t, _, _, _): return t
		}
	}

	var endTime: Double {
		switch self {
		case .transient(let t, _, _): return t
		case .continuous(let t, let d, _, _): return t + d
		}
	}

	var intensity: Float {
		switch self {
		case .transient(_, let i, _): return i
		case .continuous(_, _, let i, _): return i
		}
	}

	/// The intensity dial multiplies the authored value — it reduces, never strengthens.
	func scaled(by multiplier: Float) -> EventSpec {
		let m = max(0, min(1, multiplier))
		switch self {
		case .transient(let t, let i, let s):
			return .transient(time: t, intensity: min(1, i * m), sharpness: s)
		case .continuous(let t, let d, let i, let s):
			return .continuous(time: t, duration: d, intensity: min(1, i * m), sharpness: s)
		}
	}
}
