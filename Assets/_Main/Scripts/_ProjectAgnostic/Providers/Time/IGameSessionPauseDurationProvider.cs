namespace _Main.Scripts._ProjectAgnostic.Providers.Time
{
	public interface IGameSessionPauseDurationProvider
	{
		int MaximumPauseDurationInSeconds { get; }
	}
}
