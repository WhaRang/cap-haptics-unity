using _Main.Scripts._ProjectAgnostic.Ecs.AppLifecycle.Cmp;
using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts.Ecs.GameSaving.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Time;
using Arch.Core;

namespace _Main.Scripts._ProjectAgnostic.Ecs.AppLifecycle.Sys
{
	/// <summary>
	/// Translates OS-level app lifecycle events received from the game manager
	/// into the appropriate gameplay ECS events. Register it before <c>UpdateGameplayTimeSys</c>
	/// and <c>SaveGameStateSys</c> so the fan-out events flush in the same tick — required
	/// for the synchronous flush before iOS suspends the process.
	/// </summary>
	public sealed class ProcessAppLifecycleEventsSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly IGameplayTimeService _timeService;
		private readonly Query _lifecycleEventsQuery;

		public ProcessAppLifecycleEventsSys(World world, IGameplayTimeService timeService)
		{
			_world = world;
			_timeService = timeService;
			_lifecycleEventsQuery = _world.Query(new QueryDescription().WithAny<OnAppBackgroundedEvt, OnAppForegroundedEvt>());
		}

		public void OnTick()
		{
			ProcessLifecycleEvents();
			RemoveAllLifecycleEventComponents();
		}

		private void ProcessLifecycleEvents()
		{
			if (_lifecycleEventsQuery.IsEmpty())
				return;

			foreach (var chunk in _lifecycleEventsQuery)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);

					if (_world.Has<OnAppBackgroundedEvt>(entity))
						HandleBackgrounded();
				}
			}
		}

		private void HandleBackgrounded()
		{
			ref var cmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();
			if (_timeService.WasSessionFinished(in cmp))
				return;

			_world.Create(new OnPauseGameplayRequestedEvt());
			_world.Create(new OnGameSavingRequestedEvt());
		}

		private void RemoveAllLifecycleEventComponents()
		{
			foreach (var chunk in _lifecycleEventsQuery)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);

					_world.RemoveIfPresent<OnAppBackgroundedEvt>(entity);
					_world.RemoveIfPresent<OnAppForegroundedEvt>(entity);
				}
			}
		}
	}
}
