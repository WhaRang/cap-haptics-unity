namespace _Main.Scripts.Ecs.Time.Cmp
{
	public struct GameplayTimeSinglCmp
	{
		public long SessionStartTimeStamp;
		public long? SessionEndTimeStamp;
		public long? PauseStartTimeStamp;
		public long LastRecordedServerTimeStamp;
		
		public int RemainingPauseDurationInSeconds;

		public bool WasSessionEndedForcefullyByUser;
		public long LastSaveRequestEmittedTimestamp;
	}
}
