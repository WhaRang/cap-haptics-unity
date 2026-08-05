using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Ecs.Scoring.Cmp;

namespace _Main.Scripts.Services.Events
{
	public readonly struct OnScoreUpdatedEvent : IEventBusEvent
	{
		public int PreviousScore { get; }
		public int NewScore { get; }
		public int CurrentMultiplier { get; }
		public ScoreUpdateKind Kind { get; }

		public OnScoreUpdatedEvent(int previousScore, int newScore, int currentMultiplier, ScoreUpdateKind kind)
		{
			PreviousScore = previousScore;
			NewScore = newScore;
			CurrentMultiplier = currentMultiplier;
			Kind = kind;
		}
	}
}
