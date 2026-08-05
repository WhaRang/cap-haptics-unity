using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.Providers.Time;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Configs.Scoring;
using _Main.Scripts.Ecs.Scoring.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Events;
using _Main.Scripts.Services.Time;
using Arch.Core;

namespace _Main.Scripts.Ecs.Time.Sys
{
	/// <summary>
	/// The single writer of <see cref="GameplayTimeSinglCmp"/>: consumes the time request/event
	/// components produced anywhere in the game and applies them through <see cref="IGameplayTimeService"/>.
	/// On unpause (and on restore-after-load) it also slides the combo trigger timestamp by the pause
	/// time covered by the pause budget so the combo survives legitimate pauses.
	/// </summary>
	public sealed class UpdateGameplayTimeSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly IGameplayTimeService _timeService;
		private readonly IGameplayEventBusService _eventBus;
		private readonly IScoringConfig _scoringConfig;
		private readonly IServerTimeProvider _serverTimeProvider;
		private readonly Query _timeEventsQuery;

		public UpdateGameplayTimeSys(
			World world,
			IGameplayTimeService timeService,
			IGameplayEventBusService eventBus,
			IScoringConfig scoringConfig,
			IServerTimeProvider serverTimeProvider)
		{
			_world = world;
			_timeService = timeService;
			_eventBus = eventBus;
			_scoringConfig = scoringConfig;
			_serverTimeProvider = serverTimeProvider;

			_timeEventsQuery = _world.Query(new QueryDescription().WithAny<OnPauseGameplayRequestedEvt, OnResumeGameplayRequestedEvt, RecordLastServerTimeStampReq, RecordSessionStartReq, InitializeTimeAfterLoadedSaveReq>());
		}

		public void OnTick()
		{
			ProcessTimeInputEvents();
			RemoveAllRequestsAndEventsComponents();
		}

		private void ProcessTimeInputEvents()
		{
			if (_timeEventsQuery.IsEmpty())
				return;

			ref var cmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();

			foreach (var chunk in _timeEventsQuery)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);

					if (_world.Has<OnPauseGameplayRequestedEvt>(entity))
					{
						_timeService.Pause(ref cmp);
					}
					else if (_world.Has<OnResumeGameplayRequestedEvt>(entity))
					{
						var pauseShift = _timeService.Unpause(ref cmp);
						ApplyComboPauseShift(pauseShift);
					}
					else if (_world.Has<RecordLastServerTimeStampReq>(entity))
					{
						_timeService.RecordLastServerTimeStamp(ref cmp);
					}
					else if (_world.Has<RecordSessionStartReq>(entity))
					{
						_timeService.RecordSessionStart(ref cmp);
					}
					else if (_world.Has<InitializeTimeAfterLoadedSaveReq>(entity))
					{
						var pauseShift = _timeService.InitializeAfterLoadedSave(ref cmp);
						ApplyComboPauseShift(pauseShift);
					}
				}
			}
		}

		/// <summary>
		/// Slides the combo trigger timestamp forward by the pause time that was legitimately "covered" by the
		/// pause budget, then decides whether the combo survived: if its decay window fully elapsed it is reset
		/// and <see cref="OnComboClearedEvent"/> is raised; otherwise <see cref="OnComboRestoredEvent"/> is raised
		/// so the UI meter can resume mid-drain. No-ops when there is no active combo.
		/// </summary>
		private void ApplyComboPauseShift(PauseShiftResult pauseShift)
		{
			if (!_world.HasSinglCmp<ScoreSinglCmp>())
				return;

			ref var scoreCmp = ref _world.GetSinglCmp<ScoreSinglCmp>();

			if (!scoreCmp.ComboActivated || !scoreCmp.LastComboTriggerTimestamp.HasValue)
				return;

			scoreCmp.LastComboTriggerTimestamp = scoreCmp.LastComboTriggerTimestamp.Value.AddSeconds(pauseShift.CoveredPauseDurationInSeconds);

			var now = _serverTimeProvider.GetCurrentTime().ToUniversalTime();
			var elapsedSeconds = (now - scoreCmp.LastComboTriggerTimestamp.Value).TotalSeconds;
			var comboDurationInSeconds = _scoringConfig.GetComboDurationInSeconds(scoreCmp.CurrentMultiplier);

			if (elapsedSeconds >= comboDurationInSeconds)
			{
				scoreCmp.ComboActivated = false;
				scoreCmp.LastComboTriggerTimestamp = null;
				scoreCmp.CurrentMultiplier = _scoringConfig.MinComboMultiplier;
				_eventBus.Publish(new OnComboClearedEvent());
				return;
			}

			var remainingDurationInSeconds = (float)(comboDurationInSeconds - elapsedSeconds);
			_eventBus.Publish(new OnComboRestoredEvent(scoreCmp.CurrentMultiplier, remainingDurationInSeconds));
		}

		private void RemoveAllRequestsAndEventsComponents()
		{
			using var cb = new SCommandBuffer();

			foreach (var chunk in _timeEventsQuery)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);

					cb.Remove<OnPauseGameplayRequestedEvt>(entity);
					cb.Remove<OnResumeGameplayRequestedEvt>(entity);
					cb.Remove<RecordLastServerTimeStampReq>(entity);
					cb.Remove<RecordSessionStartReq>(entity);
					cb.Remove<InitializeTimeAfterLoadedSaveReq>(entity);
				}
			}

			cb.Playback(_world);
		}
	}
}
