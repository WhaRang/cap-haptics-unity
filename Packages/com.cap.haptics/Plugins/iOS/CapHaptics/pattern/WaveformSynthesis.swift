import Foundation

/// Android-waveform semantics → CH events (§11.4): alternating off/on segments
/// starting with off; per-segment amplitudes 0..255 aligned with the timings, or empty
/// when the rhythm alone carries the pattern. Pure and validated here so the backend
/// only ever sees a playable event list — `nil` means `invalidArgument`, and the
/// validate/construct-agreement rule from §8 applies: nil exactly when invalid.
enum WaveformSynthesis {

	/// The native 500-step cap the Android side also enforces.
	static let maxSegments = 500

	/// Amplitude used for on-segments when the caller sent no amplitude array.
	static let defaultAmplitude: Int32 = 204 // ≈ 0.8

	static func specs(timingsMs: [Int64], amplitudes: [Int32]) -> [EventSpec]? {
		guard !timingsMs.isEmpty, timingsMs.count <= maxSegments else {
			return nil
		}
		guard amplitudes.isEmpty || amplitudes.count == timingsMs.count else {
			return nil
		}
		guard timingsMs.allSatisfy({ $0 >= 0 }),
			amplitudes.allSatisfy({ $0 >= 0 && $0 <= 255 })
		else {
			return nil
		}

		var events: [EventSpec] = []
		var cursor = 0.0
		for i in timingsMs.indices {
			let duration = Double(timingsMs[i]) / 1000
			// Even index = off segment (the convention: starting with off); with an
			// explicit amplitude array the amplitude is authoritative either way.
			let amplitude: Int32
			if amplitudes.isEmpty {
				amplitude = i % 2 == 0 ? 0 : defaultAmplitude
			} else {
				amplitude = amplitudes[i]
			}
			if amplitude > 0 && duration > 0 {
				events.append(.continuous(
					time: cursor,
					duration: duration,
					intensity: Float(amplitude) / 255,
					sharpness: 0.5))
			}
			cursor += duration
		}
		return events
	}
}
