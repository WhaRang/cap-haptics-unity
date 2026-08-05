using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.Providers.SaveLoading;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Ecs.Scoring.Cmp;
using _Main.Scripts.Ecs.SessionEnding.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Events;
using _Main.Scripts.Services.Time;
using Arch.Core;
using UnityEngine;

namespace _Main.Scripts.Ecs.SessionEnding.Sys
{
	/// <summary>
	/// Consumes <see cref="EndGameSessionReq"/>: records the session end, clears the persisted save,
	/// publishes <see cref="OnGameSessionEndedEvent"/> for the UI/meta layer, and requests the gameplay
	/// scene to close. Extend it with game-specific result assembly (final score bonuses, reports, ...).
	/// </summary>
	public sealed class EndGameSessionSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly Query _endSessionReqQuery;

		private readonly IGameplayTimeService _timeService;
		private readonly IGameplayEventBusService _eventBus;
		private readonly IPersistantStorageProvider _storageProvider;

		public EndGameSessionSys(
			World world,
			IGameplayTimeService timeService,
			IGameplayEventBusService eventBus,
			IPersistantStorageProvider storageProvider)
		{
			_world = world;
			_timeService = timeService;
			_eventBus = eventBus;
			_storageProvider = storageProvider;

			_endSessionReqQuery = _world.Query(new QueryDescription().WithAll<EndGameSessionReq>());
		}

		public void OnTick()
		{
			if (!_endSessionReqQuery.TryGetFirstEntity(out var entity))
				return;

			ref var gameplayTimeSinglCmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();

			if (_timeService.WasSessionFinished(in gameplayTimeSinglCmp))
			{
				_world.RemoveAllComponentsInQuery<EndGameSessionReq>(_endSessionReqQuery);

				return;
			}

			var endGameReason = _world.Get<EndGameSessionReq>(entity).Reason;

			if (endGameReason == EndGameReason.ErrorOccurred)
				Debug.LogError($"[{nameof(EndGameSessionSys)}] Game session ended with an error.");

			_timeService.RecordSessionEnd(ref gameplayTimeSinglCmp, endGameReason == EndGameReason.ForcefullyEndedByUser);

			_world.RemoveAllComponentsInQuery<EndGameSessionReq>(_endSessionReqQuery);

			// A finished session must not be restorable.
			_storageProvider.Clear();

			ref var scoreCmp = ref _world.GetSinglCmp<ScoreSinglCmp>();
			_eventBus.Publish(new OnGameSessionEndedEvent(endGameReason, scoreCmp.CurrentScore));

			_world.Create(new OnCloseGameplaySceneRequestedEvt());
		}
	}
}
