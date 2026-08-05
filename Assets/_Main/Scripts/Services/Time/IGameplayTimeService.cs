using _Main.Scripts.Ecs.Time.Cmp;

namespace _Main.Scripts.Services.Time
{
	public interface IGameplayTimeService
	{
		int TimePassedSinceTheSessionStartInSeconds(in GameplayTimeSinglCmp cmp);
		int CompletedSessionDurationInSeconds(in GameplayTimeSinglCmp cmp);
		int RemainingSessionTimeInSeconds(in GameplayTimeSinglCmp cmp);
		int RemainingPauseTimeInSeconds(in GameplayTimeSinglCmp cmp);

		bool WasSessionEndedForcefullyByUser(in GameplayTimeSinglCmp cmp);
		bool IsPaused(in GameplayTimeSinglCmp cmp);
		bool WasSessionFinished(in GameplayTimeSinglCmp cmp);

		PauseShiftResult InitializeAfterLoadedSave(ref GameplayTimeSinglCmp cmp);
		void RecordLastServerTimeStamp(ref GameplayTimeSinglCmp cmp);
		void RecordSessionStart(ref GameplayTimeSinglCmp cmp);
		void RecordSessionEnd(ref GameplayTimeSinglCmp cmp, bool wasSessionForcefullyEndedByUser);
		void Pause(ref GameplayTimeSinglCmp cmp);
		PauseShiftResult Unpause(ref GameplayTimeSinglCmp cmp);
	}
}
