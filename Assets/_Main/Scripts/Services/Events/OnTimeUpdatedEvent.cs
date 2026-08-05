using _Main.Scripts._ProjectAgnostic.Services.EventBus;

namespace _Main.Scripts.Services.Events
{
	public readonly struct OnTimeUpdatedEvent : IEventBusEvent
	{
		public bool IsPaused { get; }
		public bool WasGameEnded { get; }
		public int RemainingPauseDurationInSeconds { get; }
		public int RemainingSessionDurationInSeconds { get; }

		public OnTimeUpdatedEvent(bool isPaused, bool wasGameEnded, int remainingPauseDurationInSeconds, int remainingSessionDurationInSeconds)
		{
			IsPaused = isPaused;
			WasGameEnded = wasGameEnded;
			RemainingPauseDurationInSeconds = remainingPauseDurationInSeconds;
			RemainingSessionDurationInSeconds = remainingSessionDurationInSeconds;
		}
	}
}
