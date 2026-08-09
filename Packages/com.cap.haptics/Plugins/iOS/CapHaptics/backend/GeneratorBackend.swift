#if os(iOS)
import Foundation
import UIKit

/// Tier 2 — `UIFeedbackGenerator` playback. Serves two audiences: pre-Core-Haptics
/// iPhones that land here naturally, and the forced-tier override on modern hardware
/// (the only way the fallback is feelable on the one test device — §11.1).
///
/// Main-thread only — the bridge marshals. Generators are kept alive and `prepare()`d
/// after each use so the Taptic Engine stays warm for the next hit.
final class GeneratorBackend {

	private lazy var selection: UISelectionFeedbackGenerator = {
		if #available(iOS 17.5, *), let view = Self.hostView() {
			return UISelectionFeedbackGenerator(view: view)
		}
		return UISelectionFeedbackGenerator()
	}()

	private lazy var notification: UINotificationFeedbackGenerator = {
		if #available(iOS 17.5, *), let view = Self.hostView() {
			return UINotificationFeedbackGenerator(view: view)
		}
		return UINotificationFeedbackGenerator()
	}()

	private var impacts: [BeatStyle: UIImpactFeedbackGenerator] = [:]
	private var pending: [DispatchWorkItem] = []

	/// Unity's root view. Since iOS 17.5 the view-associated generator initializers
	/// exist, and on recent iOS feedback from an unassociated generator can be
	/// silently dropped — found on-device in I4: generators fired and returned OK
	/// with nothing felt.
	private static func hostView() -> UIView? {
		let window = UIApplication.shared.connectedScenes
			.compactMap { $0 as? UIWindowScene }
			.flatMap { $0.windows }
			.first { $0.isKeyWindow }
		return (window ?? UIApplication.shared.delegate?.window ?? nil)?.rootViewController?.view
	}

	func playPattern(_ patternId: Int32, intensity: Float) -> Int32 {
		guard let rendering = GeneratorRegistry.rendering(forPattern: patternId) else {
			HLog.e("playPattern: unknown pattern id \(patternId)")
			return HapticResult.unsupportedPattern
		}
		// Notification/selection renderings play as tuned — the platform offers no dial.
		return play(rendering, dial: IntensityScaler.multiplier(for: intensity))
	}

	func playEffect(_ effectId: Int32) -> Int32 {
		guard let rendering = GeneratorRegistry.rendering(forEffect: effectId) else {
			HLog.e("playEffect: unknown effect id \(effectId)")
			return HapticResult.unsupportedPattern
		}
		return play(rendering, dial: nil)
	}

	func playComposition(primitiveIds: [Int32], scales: [Float], delaysMs: [Int32]) -> Int32 {
		guard let specs = PrimitiveSynthesis.composition(
			primitiveIds: primitiveIds, scales: scales, delaysMs: delaysMs)
		else {
			return HapticResult.invalidArgument
		}
		return play(.beats(GeneratorRegistry.beats(approximating: specs)), dial: nil)
	}

	/// Best effort: repeat is unsupported here (plays once), rhythm precision is
	/// whatever the dispatch queue delivers.
	func playWaveform(timingsMs: [Int64], amplitudes: [Int32], repeatIndex: Int32) -> Int32 {
		guard repeatIndex >= -1, repeatIndex < timingsMs.count,
			let specs = WaveformSynthesis.specs(timingsMs: timingsMs, amplitudes: amplitudes)
		else {
			return HapticResult.invalidArgument
		}
		if specs.isEmpty {
			return HapticResult.ok
		}
		return play(.beats(GeneratorRegistry.beats(approximating: specs)), dial: nil)
	}

	func cancel() {
		pending.forEach { $0.cancel() }
		pending.removeAll()
	}

	// MARK: -

	private func play(_ rendering: GeneratorRendering, dial: Float?) -> Int32 {
		cancel() // Android semantics: a new vibrate replaces the current one.
		HLog.d("generators: playing \(rendering)")
		switch rendering {
		case .selection:
			selection.selectionChanged()
			selection.prepare()
		case .notificationSuccess:
			notification.notificationOccurred(.success)
			notification.prepare()
		case .notificationWarning:
			notification.notificationOccurred(.warning)
			notification.prepare()
		case .notificationError:
			notification.notificationOccurred(.error)
			notification.prepare()
		case .beats(let beats):
			for beat in beats {
				let intensity = CGFloat(min(1, beat.intensity * (dial ?? 1)))
				let generator = impactGenerator(for: beat.style)
				if beat.time <= 0 {
					generator.impactOccurred(intensity: intensity)
					generator.prepare()
				} else {
					let work = DispatchWorkItem {
						generator.impactOccurred(intensity: intensity)
						generator.prepare()
					}
					pending.append(work)
					DispatchQueue.main.asyncAfter(deadline: .now() + beat.time, execute: work)
				}
			}
		}
		return HapticResult.ok
	}

	private func impactGenerator(for style: BeatStyle) -> UIImpactFeedbackGenerator {
		if let existing = impacts[style] {
			return existing
		}
		let uiStyle: UIImpactFeedbackGenerator.FeedbackStyle
		switch style {
		case .light: uiStyle = .light
		case .medium: uiStyle = .medium
		case .heavy: uiStyle = .heavy
		case .soft: uiStyle = .soft
		case .rigid: uiStyle = .rigid
		}
		let generator: UIImpactFeedbackGenerator
		if #available(iOS 17.5, *), let view = Self.hostView() {
			generator = UIImpactFeedbackGenerator(style: uiStyle, view: view)
		} else {
			generator = UIImpactFeedbackGenerator(style: uiStyle)
		}
		generator.prepare()
		impacts[style] = generator
		return generator
	}
}
#endif
