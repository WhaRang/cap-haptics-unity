#if os(iOS)
import CoreHaptics
import Foundation
import UIKit

/// The only file that touches the platform to answer "what can this device do" —
/// everything downstream is a pure function over `DeviceCapabilities`, which is what
/// keeps the tier logic testable without an iPhone (and why this file is `os(iOS)`-gated
/// while the logic files are not).
enum CapabilityProbe {

	/// Call on the main thread — `UIDevice` is a UIKit object; the bridge marshals.
	static func probe() -> DeviceCapabilities {
		DeviceCapabilities(
			sdkMajor: Int32(ProcessInfo.processInfo.operatingSystemVersion.majorVersion),
			supportsCoreHaptics: CHHapticEngine.capabilitiesForHardware().supportsHaptics,
			isPhone: UIDevice.current.userInterfaceIdiom == .phone
		)
	}
}
#endif
