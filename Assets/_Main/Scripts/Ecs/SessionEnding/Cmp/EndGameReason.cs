namespace _Main.Scripts.Ecs.SessionEnding.Cmp
{
	public enum EndGameReason
	{
		/// <summary>The player reached the game's win/finish condition.</summary>
		Completed = 0,
		RunOutOfTime = 1,
		ForcefullyEndedByUser = 2,
		ErrorOccurred = 3
	}
}
