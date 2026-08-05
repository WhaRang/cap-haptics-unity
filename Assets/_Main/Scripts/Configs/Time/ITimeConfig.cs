namespace _Main.Scripts.Configs.Time
{
	public interface ITimeConfig
	{
		long CountdownAnimationDurationInSeconds { get; }
		long GameSessionDurationInSeconds { get; }
		long SaveIntervalInSeconds { get; }
		int MaximumPauseDurationInSeconds { get; }
	}
}
