namespace Cap.Haptics
{
	/// <summary>
	/// Whether the device supports a given effect or primitive. Mirrors the Kotlin
	/// <c>SupportLevel</c> enum; travels as its name string in the capabilities JSON.
	///
	/// <see cref="Unknown"/> is a real platform condition, not hedging: API 29 can create
	/// predefined effects but has no way to query support (the query arrives in API 30), and
	/// it deliberately sits at 0 so an unparseable value degrades to "don't know" rather
	/// than a confident wrong answer.
	/// </summary>
	public enum SupportLevel
	{
		Unknown = 0,
		Yes = 1,
		No = 2,
	}

	public static class SupportLevelExtensions
	{
		/// <summary>Parses the wire string; anything unrecognized is <see cref="SupportLevel.Unknown"/>.</summary>
		public static SupportLevel ParseSupportLevel(string? value) => value switch
		{
			"YES" => SupportLevel.Yes,
			"NO" => SupportLevel.No,
			_ => SupportLevel.Unknown,
		};

		/// <summary>
		/// True when it is worth attempting — <see cref="SupportLevel.Unknown"/> counts as
		/// usable, because the platform substitutes a generic fallback rather than failing.
		/// </summary>
		public static bool IsUsable(this SupportLevel level) => level != SupportLevel.No;
	}
}
