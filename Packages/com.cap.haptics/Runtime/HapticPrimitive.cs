namespace Cap.Haptics
{
	/// <summary>
	/// Composition primitives behind the native T3 tier. Mirrors the Kotlin
	/// <c>HapticPrimitive</c>; values are the wire ids, validated against the AAR's enum
	/// manifest at init. Per-primitive support lives in
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
