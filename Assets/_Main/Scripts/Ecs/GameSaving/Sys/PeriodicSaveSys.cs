using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.Providers.Time;
using _Main.Scripts._ProjectAgnostic.Utils;
using _Main.Scripts.Configs.Time;
using _Main.Scripts.Ecs.GameSaving.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Time;
using Arch.Core;

namespace _Main.Scripts.Ecs.GameSaving.Sys
{
	/// <summary>
	/// Emits an <see cref="OnGameSavingRequestedEvt"/> once per
	/// <see cref="ITimeConfig.SaveIntervalInSeconds"/> while the session is active and
	/// not paused or finished, so the persisted <c>LastRecordedServerTimeStamp</c> stays
	/// fresh even when the user is idle and no gameplay action emits a save event.
	/// Without this the only on-pause save path is racy (can be skipped if the OS
	/// suspends before the next Update tick), and idle wall time is later
	/// mis-classified as "covered pause" on resume, rewinding the visible timer.
	/// </summary>
	public sealed class PeriodicSaveSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly IServerTimeProvider _serverTimeProvider;
		private readonly IGameplayTimeService _timeService;
		private readonly ITimeConfig _timeConfig;

		public PeriodicSaveSys(World world, IServerTimeProvider serverTimeProvider, IGameplayTimeService timeService, ITimeConfig timeConfig)
		{
			_world = world;
			_serverTimeProvider = serverTimeProvider;
			_timeService = timeService;
			_timeConfig = timeConfig;
		}

		public void OnTick()
		{
			ref var cmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();

			if (_timeService.IsPaused(in cmp) || _timeService.WasSessionFinished(in cmp))
				return;

			long currentTimestamp = _serverTimeProvider.GetCurrentTime().ToUnixTimeSeconds();

			if (currentTimestamp - cmp.LastSaveRequestEmittedTimestamp < _timeConfig.SaveIntervalInSeconds)
				return;

			cmp.LastSaveRequestEmittedTimestamp = currentTimestamp;
			_world.Create(new OnGameSavingRequestedEvt());
		}
	}
}
