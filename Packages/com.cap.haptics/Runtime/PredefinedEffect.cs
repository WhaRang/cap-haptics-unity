namespace Cap.Haptics
{
	/// <summary>
	/// The four platform-tuned effects behind the native T2 tier. Mirrors the Kotlin
	/// <c>PredefinedEffect</c>; values are the wire ids, validated against the AAR's enum
	/// manifest at init.
	/// </summary>
	public enum PredefinedEffect
	{
		Tick = 0,
		Click = 1,
		DoubleClick = 2,
		HeavyClick = 3,
	}
}
