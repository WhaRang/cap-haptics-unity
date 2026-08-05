using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.Providers.SaveLoading;
using _Main.Scripts._ProjectAgnostic.Providers.Serialization;
using _Main.Scripts._ProjectAgnostic.Utils;
using _Main.Scripts.Ecs.GameSaving.Cmp;
using _Main.Scripts.Ecs.GameSaving.Data;
using _Main.Scripts.Ecs.Scoring.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Time;
using Arch.Core;
using UnityEngine;

namespace _Main.Scripts.Ecs.GameSaving.Sys
{
	/// <summary>
	/// Consumes <see cref="OnGameSavingRequestedEvt"/>: snapshots the session singletons into a
	/// <see cref="GameSaveData"/> and writes it to persistent storage. Extend the snapshot with
	/// game-specific sections as the game grows.
	/// </summary>
	public sealed class SaveGameStateSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly IGameplayTimeService _timeService;
		private readonly IGameplaySerializationProvider _serializationProvider;
		private readonly IPersistantStorageProvider _storageProvider;
		private readonly Query _saveRequestedQuery;

		public SaveGameStateSys(
			World world,
			IGameplayTimeService timeService,
			IGameplaySerializationProvider serializationProvider,
			IPersistantStorageProvider storageProvider)
		{
			_world = world;
			_timeService = timeService;
			_serializationProvider = serializationProvider;
			_storageProvider = storageProvider;
			_saveRequestedQuery = _world.Query(new QueryDescription().WithAll<OnGameSavingRequestedEvt>());
		}

		public void OnTick()
		{
			if (_saveRequestedQuery.IsEmpty())
				return;

			_world.DestroyAllEntitiesInQuery(_saveRequestedQuery);

			ref var timeCmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();

			if (_timeService.WasSessionFinished(in timeCmp))
				return;

			_timeService.RecordLastServerTimeStamp(ref timeCmp);

			var saveData = AssembleSaveData(in timeCmp);

			_serializationProvider.Serialize(saveData)
								.Switch
								(
									success => _storageProvider.Write(success.Value),
									error => Debug.LogError($"[{nameof(SaveGameStateSys)}] Failed to serialize save data: {error.Value}")
								);
		}

		private GameSaveData AssembleSaveData(in GameplayTimeSinglCmp timeCmp)
		{
			ref var scoreCmp = ref _world.GetSinglCmp<ScoreSinglCmp>();

			return new GameSaveData
			{
				Time = new TimeSaveData
				{
					SessionStartTimeStamp = timeCmp.SessionStartTimeStamp,
					LastRecordedServerTimeStamp = timeCmp.LastRecordedServerTimeStamp,
					RemainingPauseDurationInSeconds = timeCmp.RemainingPauseDurationInSeconds
				},
				Score = new ScoreSaveData
				{
					CurrentScore = scoreCmp.CurrentScore,
					CurrentMultiplier = scoreCmp.CurrentMultiplier,
					ComboActivated = scoreCmp.ComboActivated,
					LastComboTriggerTimestampUnixSeconds = scoreCmp.LastComboTriggerTimestamp?.ToUnixTimeSeconds()
				}
			};
		}
	}
}
