using Cap.Haptics.Client;

namespace Cap.Haptics.PatternTypes
{
	/// <summary>
	/// Composition primitives behind the native T3 tier. The names come from Android's
	/// <c>VibrationEffect.Composition</c> constants, but the vocabulary is cross-platform:
	/// iOS synthesizes each primitive as a Core Haptics event pattern. Values are the wire
	/// ids, validated against the AAR's enum manifest at init; per-device support lives in
	/// <see cref="HapticCapabilities.Primitives"/>.
	/// </summary>
	public enum HapticPrimitive
	{
		Click = 0,
		Tick = 1,
		QuickRise = 2,
		SlowRise = 3,
		QuickFall = 4,
		LowTick = 5,
		Thud = 6,
		Spin = 7,
	}
}
