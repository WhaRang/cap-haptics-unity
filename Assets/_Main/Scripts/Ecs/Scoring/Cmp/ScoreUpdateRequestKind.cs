namespace _Main.Scripts.Ecs.Scoring.Cmp
{
	public enum ScoreUpdateRequestKind
	{
		/// <summary>Adds <c>Points * CurrentMultiplier</c> to the score and advances the combo.</summary>
		Gain = 0,
		/// <summary>Subtracts <c>Points</c> from the score (clamped at zero). Does not touch the combo.</summary>
		Penalty = 1,
		/// <summary>UI refresh only, raised after a session restore. Carries no payload to apply.</summary>
		Restore = 2
	}
}
