namespace _Main.Scripts.Ecs.GameSaving.Data
{
	/// <summary>
	/// The serialized session snapshot. Extend this with game-specific sections (board state,
	/// inventory, ...) and fill them in <c>SaveGameStateSys</c> / restore them in <c>StartSessionSys</c>.
	/// </summary>
	public struct GameSaveData
	{
		public TimeSaveData Time;
		public ScoreSaveData Score;
	}

	public struct TimeSaveData
	{
		public long SessionStartTimeStamp;
		public long LastRecordedServerTimeStamp;
		public int RemainingPauseDurationInSeconds;
	}

	public struct ScoreSaveData
	{
		public int CurrentScore;
		public int CurrentMultiplier;
		public bool ComboActivated;
		public long? LastComboTriggerTimestampUnixSeconds;
	}
}
