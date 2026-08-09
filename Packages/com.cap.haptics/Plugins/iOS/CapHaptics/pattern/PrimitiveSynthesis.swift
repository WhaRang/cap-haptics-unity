import Foundation

/// The iOS answer to the per-primitive hardware lottery Android has: every
/// `HapticPrimitive` wire id is *synthesized* from CH events (PLAN.md §11.4), so
/// support is all-or-nothing with the engine. Recipes are pure data — tune by feel
/// on-device, verify shape here.
enum PrimitiveSynthesis {

	/// Nominal recipe duration in ms — reported as the capability `durationMs` and used
	/// as the composition time cursor (a step's delay runs after the previous
	/// primitive *ends*, matching Android's `Composition.addPrimitive` semantics).
	static func durationMs(forPrimitive id: Int32) -> Int32 {
		switch id {
		case 0: return 20   // CLICK — transient
		case 1: return 15   // TICK — transient
		case 2: return 150  // QUICK_RISE
		case 3: return 500  // SLOW_RISE
		case 4: return 100  // QUICK_FALL
		case 5: return 15   // LOW_TICK — transient
		case 6: return 40   // THUD — soft continuous
		case 7: return 200  // SPIN — wobble
		default: return -1
		}
	}

	/// Events for one primitive at `time`, scaled by `scale` (0..1, the composition
	/// step's strength). Nil for an unknown wire id.
	static func specs(forPrimitive id: Int32, at time: Double, scale: Float) -> [EventSpec]? {
		let s = max(0, min(1, scale))
		switch id {
		case 0: // CLICK — the workhorse: crisp, full-bodied
			return [.transient(time: time, intensity: s, sharpness: 0.7)]
		case 1: // TICK — lighter and sharper than CLICK
			return [.transient(time: time, intensity: s * 0.6, sharpness: 1.0)]
		case 2: // QUICK_RISE — 150 ms stepped ramp up
			return ramp(at: time, durationMs: 150, from: 0.2 * s, to: s, sharpness: 0.5)
		case 3: // SLOW_RISE — 500 ms stepped ramp up
			return ramp(at: time, durationMs: 500, from: 0.15 * s, to: s, sharpness: 0.45)
		case 4: // QUICK_FALL — 100 ms stepped ramp down
			return ramp(at: time, durationMs: 100, from: s, to: 0.2 * s, sharpness: 0.5)
		case 5: // LOW_TICK — dull tick
			return [.transient(time: time, intensity: s * 0.6, sharpness: 0.2)]
		case 6: // THUD — soft, round body blow
			return [.continuous(time: time, duration: 0.04, intensity: s, sharpness: 0.1)]
		case 7: // SPIN — 200 ms sharpness/intensity wobble
			return [
				.continuous(time: time, duration: 0.05, intensity: s * 0.7, sharpness: 0.3),
				.continuous(time: time + 0.05, duration: 0.05, intensity: s, sharpness: 0.7),
				.continuous(time: time + 0.10, duration: 0.05, intensity: s * 0.7, sharpness: 0.3),
				.continuous(time: time + 0.15, duration: 0.05, intensity: s, sharpness: 0.7),
			]
		default:
			return nil
		}
	}

	/// Composition steps → one event list, one time axis (a single engine play call —
	/// no scheduling smear, the Android-T2 lesson from §3.2).
	static func composition(
		primitiveIds: [Int32], scales: [Float], delaysMs: [Int32]
	) -> [EventSpec]? {
		guard !primitiveIds.isEmpty,
			primitiveIds.count == scales.count,
			primitiveIds.count == delaysMs.count
		else {
			return nil
		}

		var events: [EventSpec] = []
		var cursor = 0.0
		for i in primitiveIds.indices {
			guard delaysMs[i] >= 0 else {
				return nil
			}
			cursor += Double(delaysMs[i]) / 1000
			guard let specs = specs(forPrimitive: primitiveIds[i], at: cursor, scale: scales[i]) else {
				return nil
			}
			events.append(contentsOf: specs)
			cursor += Double(durationMs(forPrimitive: primitiveIds[i])) / 1000
		}
		return events
	}

	/// Stepped intensity ramp — see EventSpec on why not a parameter curve.
	static func ramp(
		at time: Double, durationMs: Int, from: Float, to: Float, sharpness: Float,
		steps: Int = 8
	) -> [EventSpec] {
		let stepDuration = Double(durationMs) / 1000 / Double(steps)
		return (0..<steps).map { i in
			let progress = Float(i) / Float(steps - 1)
			return .continuous(
				time: time + Double(i) * stepDuration,
				duration: stepDuration,
				intensity: from + (to - from) * progress,
				sharpness: sharpness)
		}
	}
}
