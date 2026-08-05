using _Main.Scripts._ProjectAgnostic.Services.EventBus;

namespace _Main.Scripts.Services.Events
{
	public readonly struct OnGameSessionStartedEvent : IEventBusEvent
	{
		public bool WasRestoredFromSave { get; }

		public OnGameSessionStartedEvent(bool wasRestoredFromSave)
		{
			WasRestoredFromSave = wasRestoredFromSave;
		}
	}
}
