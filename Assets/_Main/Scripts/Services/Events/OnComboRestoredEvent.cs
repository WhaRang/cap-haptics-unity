using _Main.Scripts._ProjectAgnostic.Services.EventBus;

namespace _Main.Scripts.Services.Events
{
	public readonly struct OnComboRestoredEvent : IEventBusEvent
	{
		public int CurrentMultiplier { get; }
		public float RemainingDurationInSeconds { get; }

		public OnComboRestoredEvent(int currentMultiplier, float remainingDurationInSeconds)
		{
			CurrentMultiplier = currentMultiplier;
			RemainingDurationInSeconds = remainingDurationInSeconds;
		}
	}
}
