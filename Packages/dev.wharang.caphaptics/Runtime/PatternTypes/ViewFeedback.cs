using Cap.Haptics.Client;

namespace Cap.Haptics.PatternTypes
{
	/// <summary>
	/// The platform's UI-gesture haptics — the one channel that obeys the user's haptic
	/// settings and can report <see cref="HapticResult.Suppressed"/>. An Android concept
	/// (<c>View.performHapticFeedback</c> constants), consulted natively when rendering
	/// patterns like <see cref="HapticPattern.LongPress"/>; iOS has no equivalent channel.
	/// On the C# side the enum exists to validate the wire ids against the AAR's enum
	/// manifest at init.
	/// </summary>
	public enum ViewFeedback
	{
		LongPress = 0,
		VirtualKey = 1,
		KeyboardTap = 2,
		ClockTick = 3,
		ContextClick = 4,
		TextHandleMove = 5,
		Confirm = 6,
		Reject = 7,
		GestureStart = 8,
		GestureEnd = 9,
		ToggleOn = 10,
		ToggleOff = 11,
		SegmentTick = 12,
		DragStart = 13,
	}
}
