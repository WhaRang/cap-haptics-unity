using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Ecs.SessionEnding.Cmp;

namespace _Main.Scripts.Services.Events
{
	public readonly struct OnGameSessionEndedEvent : IEventBusEvent
	{
		public EndGameReason Reason { get; }
		public int FinalScore { get; }

		public OnGameSessionEndedEvent(EndGameReason reason, int finalScore)
		{
			Reason = reason;
			FinalScore = finalScore;
		}
	}
}
