namespace _Main.Scripts.Configs.Scoring
{
	public interface IScoringConfig
	{
		int MinComboMultiplier { get; }
		int MaxComboMultiplier { get; }

		/// <summary>
		/// The decay window (in seconds) a combo at <paramref name="multiplier"/> survives before it resets.
		/// </summary>
		int GetComboDurationInSeconds(int multiplier);
	}
}
