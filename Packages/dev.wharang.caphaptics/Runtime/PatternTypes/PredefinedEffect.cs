namespace Cap.Haptics.PatternTypes
{
	/// <summary>
	/// The four platform-tuned effects behind the native T2 tier — rendered through
	/// <c>VibrationEffect.createPredefined</c> on Android and the closest
	/// <c>UIFeedbackGenerator</c> feedback on iOS. Values are the wire ids, validated
	/// against the AAR's enum manifest at init.
	/// </summary>
	public enum PredefinedEffect
	{
		Tick = 0,
		Click = 1,
		DoubleClick = 2,
		HeavyClick = 3,
	}
}
