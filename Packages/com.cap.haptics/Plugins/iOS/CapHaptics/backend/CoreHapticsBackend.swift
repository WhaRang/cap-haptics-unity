#if os(iOS)
import CoreHaptics
import Foundation

/// Tier 3 — everything plays through one `CHHapticEngine`.
///
/// The engine is the iOS-specific risk (§11.4): it stops on backgrounding, audio
/// interruptions and system policy, so the lifecycle contract here is "never assume
/// running": recreate on reset, restart lazily before every play. Getting this wrong
/// works for the first minute and dies after the first phone call.
///
/// Main-thread only — the bridge marshals.
final class CoreHapticsBackend {

	private var engine: CHHapticEngine?
	private var engineStarted = false
	private var lastStopReason: CHHapticEngine.StoppedReason?
	private var activePlayer: CHHapticAdvancedPatternPlayer?

	func playPattern(_ patternId: Int32, intensity: Float) -> Int32 {
		guard let specs = PatternRegistry.specs(forPattern: patternId) else {
			HLog.e("playPattern: unknown pattern id \(patternId)")
			return HapticResult.unsupportedPattern
		}
		return play(specs, intensityDial: intensity, loop: false)
	}

	func playComposition(primitiveIds: [Int32], scales: [Float], delaysMs: [Int32]) -> Int32 {
		guard let specs = PrimitiveSynthesis.composition(
			primitiveIds: primitiveIds, scales: scales, delaysMs: delaysMs)
		else {
			HLog.e("playComposition: invalid steps (unknown primitive, negative delay, or empty)")
			return HapticResult.invalidArgument
		}
		// Step scales already carry the caller's intensity (multiplied in C#, like the
		// Kotlin path) — no second dial application here.
		return play(specs, intensityDial: nil, loop: false)
	}

	func playWaveform(timingsMs: [Int64], amplitudes: [Int32], repeatIndex: Int32) -> Int32 {
		guard repeatIndex >= -1, repeatIndex < timingsMs.count,
			let specs = WaveformSynthesis.specs(timingsMs: timingsMs, amplitudes: amplitudes)
		else {
			return HapticResult.invalidArgument
		}
		if specs.isEmpty {
			// Valid input that renders to silence (all-zero amplitudes) — an honest no-op.
			return HapticResult.ok
		}
		// CH players loop the whole pattern; a mid-pattern repeatIndex is approximated
		// as full-pattern looping — documented divergence, runs until cancel() either way.
		return play(specs, intensityDial: nil, loop: repeatIndex >= 0)
	}

	func cancel() {
		try? activePlayer?.cancel()
		activePlayer = nil
	}

	/// Releases the engine so the haptic hardware is free for `UIFeedbackGenerator`: a
	/// running haptics-only engine suppresses generator (and system) haptics — found
	/// on-device in I4 when forcing tier 2 played nothing after any tier-3 playback.
	/// Destroyed rather than stopped: `stop()` is async and the handover is not
	/// guaranteed prompt; releasing the object is the strong form. The next tier-3
	/// play recreates lazily via `ensureRunningEngine`.
	func suspend() {
		guard engine != nil else {
			return
		}
		try? activePlayer?.cancel()
		activePlayer = nil
		engine?.stop()
		engine = nil
		engineStarted = false
		HLog.d("engine released — handing the actuator to the generators")
	}

	// MARK: - Engine lifecycle

	private func play(_ specs: [EventSpec], intensityDial: Float?, loop: Bool) -> Int32 {
		guard let engine = ensureRunningEngine() else {
			// A lifecycle stop (backgrounded, audio interruption) is suppression, not
			// breakage — the Android SUPPRESSED distinction, ported.
			return lastStopReason == .applicationSuspended || lastStopReason == .audioSessionInterrupt
				? HapticResult.suppressed
				: HapticResult.platformError
		}

		let scaled: [EventSpec]
		if let dial = intensityDial {
			let multiplier = IntensityScaler.multiplier(for: dial)
			scaled = specs.map { $0.scaled(by: multiplier) }
		} else {
			scaled = specs
		}

		do {
			let pattern = try CHHapticPattern(events: scaled.map(chEvent(for:)), parameters: [])
			let player = try engine.makeAdvancedPlayer(with: pattern)
			player.loopEnabled = loop
			// Android semantics: a new vibrate replaces the current one.
			try? activePlayer?.cancel()
			try player.start(atTime: CHHapticTimeImmediate)
			activePlayer = player
			return HapticResult.ok
		} catch {
			HLog.e("play failed: \(error.localizedDescription)")
			return HapticResult.platformError
		}
	}

	private func ensureRunningEngine() -> CHHapticEngine? {
		if engine == nil {
			do {
				let created = try CHHapticEngine()
				created.playsHapticsOnly = true
				created.stoppedHandler = { [weak self] reason in
					self?.engineStarted = false
					self?.lastStopReason = reason
					HLog.d("engine stopped: \(reason.rawValue)")
				}
				created.resetHandler = { [weak self] in
					// Server recovered from a failure — players are dead; start fresh
					// lazily on the next play.
					self?.engineStarted = false
					self?.activePlayer = nil
					HLog.d("engine reset")
				}
				engine = created
			} catch {
				HLog.e("CHHapticEngine creation failed: \(error.localizedDescription)")
				return nil
			}
		}
		if !engineStarted {
			do {
				try engine?.start()
				engineStarted = true
				lastStopReason = nil
			} catch {
				HLog.e("CHHapticEngine start failed: \(error.localizedDescription)")
				return nil
			}
		}
		return engine
	}

	private func chEvent(for spec: EventSpec) -> CHHapticEvent {
		switch spec {
		case .transient(let time, let intensity, let sharpness):
			return CHHapticEvent(
				eventType: .hapticTransient,
				parameters: [
					CHHapticEventParameter(parameterID: .hapticIntensity, value: intensity),
					CHHapticEventParameter(parameterID: .hapticSharpness, value: sharpness),
				],
				relativeTime: time)
		case .continuous(let time, let duration, let intensity, let sharpness):
			return CHHapticEvent(
				eventType: .hapticContinuous,
				parameters: [
					CHHapticEventParameter(parameterID: .hapticIntensity, value: intensity),
					CHHapticEventParameter(parameterID: .hapticSharpness, value: sharpness),
				],
				relativeTime: time,
				duration: duration)
		}
	}
}
#endif
