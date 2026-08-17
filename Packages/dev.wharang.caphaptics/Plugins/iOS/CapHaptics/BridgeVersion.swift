import Foundation

/// The native ABI version of this plugin. Must match `Haptics.ExpectedBridgeVersion`
/// on the C# side — checked before anything else at init, so a stale copy of the
/// plugin sources in the Unity package fails loudly instead of mysteriously later.
/// Same failure philosophy as the Android AAR's `BridgeVersion.CURRENT`.
enum BridgeVersion {
	static let current: Int32 = 2
}
