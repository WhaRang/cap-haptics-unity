import Foundation

/// The probe result — everything tier selection and the capabilities JSON need, and
/// nothing platform-typed, so all logic downstream of `CapabilityProbe` is a pure
/// function testable on any Mac (the §5.2 constraint, ported).
struct DeviceCapabilities {
	/// iOS major version — the `sdkInt` of the wire format.
	let sdkMajor: Int32

	/// `CHHapticEngine.capabilitiesForHardware().supportsHaptics` — the tier 3 gate.
	let supportsCoreHaptics: Bool

	/// iPhone idiom — the tier 2 gate (generators exist on no other idiom).
	let isPhone: Bool
}
