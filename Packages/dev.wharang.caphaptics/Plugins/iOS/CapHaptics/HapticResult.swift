import Foundation

/// Wire result codes — must match the C# `HapticResult` enum exactly (append-only).
enum HapticResult {
	static let ok: Int32 = 0
	static let notInitialized: Int32 = 1
	static let noVibrator: Int32 = 2
	static let unsupportedPattern: Int32 = 3
	static let invalidArgument: Int32 = 4
	static let platformError: Int32 = 5
	static let suppressed: Int32 = 6
}
