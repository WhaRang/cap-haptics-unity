#if os(iOS)
import Foundation

// The @_cdecl surface Unity reaches over P/Invoke ([DllImport("__Internal")]).
// Rules of this boundary (PLAN.md §11.3): C types only, no throw/fatalError may
// escape, returned strings are strdup'd and freed by C# via capHapticsFreeString.
//
// I3 surface: handshake, init + probe, capabilities, forced tier, and Core Haptics
// playback (playPattern / playComposition / playWaveform / cancel). Generator playback
// (playEffect, and forced-tier-2 routing) arrives in I4.

/// SDK state. Unity calls the bridge from its single main thread and every mutation
/// below additionally hops to the app main queue, so plain vars suffice — the
/// `nonisolated(unsafe)` is the documented version of that contract.
private final class CapHapticsCore {
	nonisolated(unsafe) static let shared = CapHapticsCore()

	var initialized = false
	var caps: DeviceCapabilities?
	var deviceTier: Int32 = 0
	var forcedTier: Int32 = -1

	var activeTier: Int32 {
		TierSelector.activeTier(deviceTier: deviceTier, forcedTier: forcedTier)
	}

	/// Created at init when the device tier allows it; nil otherwise.
	var coreHaptics: CoreHapticsBackend?
}

/// Tier routing for one playback call — the per-call twin of the Kotlin backend
/// selection. Runs on the main queue (callers marshal).
private func routedPlay(
	_ what: String, _ viaCoreHaptics: (CoreHapticsBackend) -> Int32
) -> Int32 {
	let core = CapHapticsCore.shared
	guard core.initialized else {
		return HapticResult.notInitialized
	}
	switch core.activeTier {
	case 3:
		guard let backend = core.coreHaptics else {
			return HapticResult.platformError
		}
		return viaCoreHaptics(backend)
	case 2:
		// I4 brings the generator backend; until then a forced (or natural) tier 2 is
		// honest about not playing rather than silently pretending.
		HLog.d("\(what): tier 2 (generators) not implemented until I4")
		return HapticResult.unsupportedPattern
	default:
		return HapticResult.noVibrator
	}
}

/// Copies a C array crossing the boundary — never retain the pointer (§11.3).
private func copyArray<T>(_ pointer: UnsafePointer<T>?, _ count: Int32) -> [T]? {
	if count == 0 {
		return []
	}
	guard let pointer, count > 0, count <= 10_000 else {
		return nil
	}
	return Array(UnsafeBufferPointer(start: pointer, count: Int(count)))
}

/// `UIDevice`/`UIFeedbackGenerator`/`CHHapticEngine` work belongs on the main thread,
/// and Unity's main thread is not it — the marshalling happens here, inside the
/// plugin, mirroring how the Kotlin side wraps `performHapticFeedback`.
private func onMainSync<T>(_ body: () -> T) -> T {
	if Thread.isMainThread {
		return body()
	}
	return DispatchQueue.main.sync(execute: body)
}

@_cdecl("capHapticsGetBridgeVersion")
public func capHapticsGetBridgeVersion() -> Int32 {
	return BridgeVersion.current
}

/// Idempotent. Returns true even on haptic-less hardware (iPad, simulator): the SDK is
/// up and every later call is a well-behaved no-op — tier 0 in the capabilities is the
/// honest signal, not a failed init.
@_cdecl("capHapticsInitialize")
public func capHapticsInitialize(_ verbose: Bool) -> Bool {
	HLog.verbose = verbose
	return onMainSync {
		let core = CapHapticsCore.shared
		if core.initialized {
			return true
		}
		let caps = CapabilityProbe.probe()
		core.caps = caps
		core.deviceTier = TierSelector.deviceTier(caps)
		if core.deviceTier >= 3 {
			core.coreHaptics = CoreHapticsBackend()
		}
		core.initialized = true
		HLog.d("initialized: iOS \(caps.sdkMajor), coreHaptics=\(caps.supportsCoreHaptics), " +
			"phone=\(caps.isPhone) → tier \(core.deviceTier)")
		return true
	}
}

/// Returned pointer is strdup'd — C# must call capHapticsFreeString after marshalling.
@_cdecl("capHapticsGetCapabilitiesJson")
public func capHapticsGetCapabilitiesJson() -> UnsafeMutablePointer<CChar>? {
	let json = onMainSync { () -> String in
		let core = CapHapticsCore.shared
		guard core.initialized, let caps = core.caps else {
			return CapabilitiesJson.notInitialized()
		}
		return CapabilitiesJson.of(caps, deviceTier: core.deviceTier, activeTier: core.activeTier)
	}
	return strdup(json)
}

@_cdecl("capHapticsFreeString")
public func capHapticsFreeString(_ s: UnsafeMutablePointer<CChar>?) {
	free(s)
}

/// Negative = automatic. Clamped to the device's natural tier; iOS has no tier 1, so a
/// request for 1 lands on 2 (see TierSelector). Returns the tier actually in effect.
@_cdecl("capHapticsSetForcedTier")
public func capHapticsSetForcedTier(_ tierLevel: Int32) -> Int32 {
	return onMainSync {
		let core = CapHapticsCore.shared
		guard core.initialized else {
			HLog.e("setForcedTier(\(tierLevel)) before initialize — ignored")
			return 0
		}
		core.forcedTier = tierLevel
		HLog.d("setForcedTier(\(tierLevel)) → active tier \(core.activeTier)")
		return core.activeTier
	}
}

/// The hot path: wire id + intensity in, result code out.
@_cdecl("capHapticsPlayPattern")
public func capHapticsPlayPattern(_ patternId: Int32, _ intensity: Float) -> Int32 {
	return onMainSync {
		routedPlay("playPattern") { $0.playPattern(patternId, intensity: intensity) }
	}
}

@_cdecl("capHapticsPlayComposition")
public func capHapticsPlayComposition(
	_ primitiveIds: UnsafePointer<Int32>?, _ scales: UnsafePointer<Float>?,
	_ delaysMs: UnsafePointer<Int32>?, _ count: Int32
) -> Int32 {
	guard count > 0,
		let ids = copyArray(primitiveIds, count),
		let scaleValues = copyArray(scales, count),
		let delays = copyArray(delaysMs, count)
	else {
		return HapticResult.invalidArgument
	}
	return onMainSync {
		routedPlay("playComposition") {
			$0.playComposition(primitiveIds: ids, scales: scaleValues, delaysMs: delays)
		}
	}
}

@_cdecl("capHapticsPlayWaveform")
public func capHapticsPlayWaveform(
	_ timingsMs: UnsafePointer<Int64>?, _ amplitudes: UnsafePointer<Int32>?,
	_ timingsCount: Int32, _ amplitudesCount: Int32, _ repeatIndex: Int32
) -> Int32 {
	guard timingsCount > 0,
		let timings = copyArray(timingsMs, timingsCount),
		let amps = copyArray(amplitudes, amplitudesCount)
	else {
		return HapticResult.invalidArgument
	}
	return onMainSync {
		routedPlay("playWaveform") {
			$0.playWaveform(timingsMs: timings, amplitudes: amps, repeatIndex: repeatIndex)
		}
	}
}

@_cdecl("capHapticsCancel")
public func capHapticsCancel() {
	onMainSync {
		CapHapticsCore.shared.coreHaptics?.cancel()
	}
}
#endif
