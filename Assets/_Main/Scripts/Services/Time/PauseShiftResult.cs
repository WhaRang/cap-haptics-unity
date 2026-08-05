namespace _Main.Scripts.Services.Time
{
	public readonly struct PauseShiftResult
	{
		public int CoveredPauseDurationInSeconds { get; }
		public int UncoveredPauseDurationInSeconds { get; }

		public PauseShiftResult(int covered, int uncovered)
		{
			CoveredPauseDurationInSeconds = covered;
			UncoveredPauseDurationInSeconds = uncovered;
		}
	}
}
