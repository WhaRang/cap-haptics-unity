import Foundation
import os.log

/// Tagged, gated logging — the `HLog.kt` twin. Errors always log; verbose chatter only
/// when the game asked for it at init. Read with Console.app filtered to subsystem
/// `com.cap.haptics`, or `xcrun devicectl` log streaming.
///
/// Uses the `os_log` function rather than `os.Logger` — the latter is iOS 14+ and the
/// deployment floor is 13 (PLAN.md §11.1).
enum HLog {
	private static let log = OSLog(subsystem: "com.cap.haptics", category: "CapHaptics")

	/// Set once from `capHapticsInitialize`; reads/writes race harmlessly (worst case
	/// a dropped debug line during init).
	nonisolated(unsafe) static var verbose = false

	static func d(_ message: String) {
		if verbose {
			os_log("%{public}@", log: log, type: .debug, message)
		}
	}

	static func e(_ message: String) {
		os_log("%{public}@", log: log, type: .error, message)
	}
}
