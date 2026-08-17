import Foundation

/// Tri-state support answer, mirroring the Kotlin `SupportLevel` and the C# parser's
/// vocabulary. `unknown` is a real answer, not a shrug: a pre-Core-Haptics iPhone has
/// generators with no query API — "fire and hope", exactly like Android API 29's T2.
enum SupportLevel: String {
	case yes = "YES"
	case no = "NO"
	case unknown = "UNKNOWN"
}
