using _Main.Scripts._ProjectAgnostic.Providers.Time;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts._ProjectAgnostic.Utils;
using _Main.Scripts.Configs.Time;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Events;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace _Main.Scripts.Services.Time
{
	[UsedImplicitly]
	public sealed class GameplayTimeService : IGameplayTimeService
	{
		private readonly IServerTimeProvider _serverTimeProvider;
		private readonly ITimeConfig _timeConfig;
		private readonly IGameplayEventBusService _eventBus;
		private readonly IGameSessionPauseDurationProvider _pauseDurationProvider;

		public GameplayTimeService(
			IServerTimeProvider serverTimeProvider,
			ITimeConfig timeConfig,
			IGameplayEventBusService eventBus,
			IGameSessionPauseDurationProvider pauseDurationProvider)
		{
			_serverTimeProvider = serverTimeProvider;
			_timeConfig = timeConfig;
			_eventBus = eventBus;
			_pauseDurationProvider = pauseDurationProvider;
		}

		public int TimePassedSinceTheSessionStartInSeconds(in GameplayTimeSinglCmp cmp)
			=> (int)(_serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds() - cmp.SessionStartTimeStamp);

		public int CompletedSessionDurationInSeconds(in GameplayTimeSinglCmp cmp)
		{
			Assert.IsTrue(cmp.SessionEndTimeStamp != null);
			return (int)(cmp.SessionEndTimeStamp!.Value - cmp.SessionStartTimeStamp);
		}

		public int RemainingSessionTimeInSeconds(in GameplayTimeSinglCmp cmp)
			=> (int)(cmp.SessionStartTimeStamp + _timeConfig.GameSessionDurationInSeconds - _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds());

		public int RemainingPauseTimeInSeconds(in GameplayTimeSinglCmp cmp)
		{
			Assert.IsTrue(cmp.SessionStartTimeStamp > 0);
			Assert.IsTrue(cmp.PauseStartTimeStamp != null);

			int currentPauseDuration = Mathf.Max(0, (int)(_serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds() - cmp.PauseStartTimeStamp!.Value));
			return Mathf.Max(0, cmp.RemainingPauseDurationInSeconds - currentPauseDuration);
		}

		public bool WasSessionEndedForcefullyByUser(in GameplayTimeSinglCmp cmp) => cmp.WasSessionEndedForcefullyByUser;

		public bool IsPaused(in GameplayTimeSinglCmp cmp) => cmp.PauseStartTimeStamp != null;

		public bool WasSessionFinished(in GameplayTimeSinglCmp cmp) => cmp.SessionEndTimeStamp != null;

		public PauseShiftResult InitializeAfterLoadedSave(ref GameplayTimeSinglCmp cmp)
		{
			long currentServerTimestamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();
			int secondsPassedSinceLastRecordedTime = (int)(currentServerTimestamp - cmp.LastRecordedServerTimeStamp);
			int coveredPauseDuration = Mathf.Min(secondsPassedSinceLastRecordedTime, cmp.RemainingPauseDurationInSeconds);
			int uncoveredPauseDuration = Mathf.Max(0, secondsPassedSinceLastRecordedTime - coveredPauseDuration);

			cmp.SessionStartTimeStamp += coveredPauseDuration;
			cmp.RemainingPauseDurationInSeconds = Mathf.Max(0, cmp.RemainingPauseDurationInSeconds - coveredPauseDuration);
			cmp.PauseStartTimeStamp = currentServerTimestamp;

			PublishTimeUpdated(ref cmp);

			return new PauseShiftResult(coveredPauseDuration, uncoveredPauseDuration);
		}

		public void RecordLastServerTimeStamp(ref GameplayTimeSinglCmp cmp)
		{
			cmp.LastRecordedServerTimeStamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();
		}

		public void RecordSessionStart(ref GameplayTimeSinglCmp cmp)
		{
			Assert.IsTrue(cmp.SessionEndTimeStamp == null);

			long serverTimestamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();

			cmp.SessionStartTimeStamp = serverTimestamp;
			cmp.LastRecordedServerTimeStamp = serverTimestamp;
			cmp.RemainingPauseDurationInSeconds = _pauseDurationProvider.MaximumPauseDurationInSeconds;

			PublishTimeUpdated(ref cmp);
		}

		public void RecordSessionEnd(ref GameplayTimeSinglCmp cmp, bool wasSessionForcefullyEndedByUser)
		{
			if (cmp.SessionEndTimeStamp != null)
				return;

			cmp.WasSessionEndedForcefullyByUser = wasSessionForcefullyEndedByUser;
			cmp.SessionEndTimeStamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();

			PublishTimeUpdated(ref cmp);
		}

		public void Pause(ref GameplayTimeSinglCmp cmp)
		{
			if (cmp.PauseStartTimeStamp != null)
				return;

			cmp.PauseStartTimeStamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();

			long serverTimestamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();
			int remainingSessionDurationInSecondsOverride = (int)(cmp.SessionStartTimeStamp + _timeConfig.GameSessionDurationInSeconds - serverTimestamp);

			PublishTimeUpdated(ref cmp, remainingSessionDurationInSecondsOverride);
		}

		public PauseShiftResult Unpause(ref GameplayTimeSinglCmp cmp)
		{
			if (cmp.PauseStartTimeStamp == null)
				return new PauseShiftResult(0, 0);

			long pauseStart = cmp.PauseStartTimeStamp!.Value;

			int pauseDurationInSeconds = (int)(_serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds() - pauseStart);
			int uncoveredPauseDurationInSeconds = Mathf.Max(0, pauseDurationInSeconds - cmp.RemainingPauseDurationInSeconds);
			int coveredPauseDurationInSeconds = pauseDurationInSeconds - uncoveredPauseDurationInSeconds;

			cmp.RemainingPauseDurationInSeconds = Mathf.Max(0, cmp.RemainingPauseDurationInSeconds - pauseDurationInSeconds);
			cmp.SessionStartTimeStamp += coveredPauseDurationInSeconds;
			cmp.PauseStartTimeStamp = null;

			PublishTimeUpdated(ref cmp);

			return new PauseShiftResult(coveredPauseDurationInSeconds, uncoveredPauseDurationInSeconds);
		}

		private void PublishTimeUpdated(ref GameplayTimeSinglCmp cmp, int? remainingSessionDurationInSecondsOverride = null)
		{
			bool isPaused = cmp.PauseStartTimeStamp != null;
			bool wasGameEnded = cmp.SessionEndTimeStamp != null;
			int remainingSessionDurationInSeconds = remainingSessionDurationInSecondsOverride
				?? (int)(cmp.SessionStartTimeStamp + _timeConfig.GameSessionDurationInSeconds - _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds());

			_eventBus.Publish(new OnTimeUpdatedEvent(isPaused, wasGameEnded, cmp.RemainingPauseDurationInSeconds, remainingSessionDurationInSeconds));
		}
	}
}
