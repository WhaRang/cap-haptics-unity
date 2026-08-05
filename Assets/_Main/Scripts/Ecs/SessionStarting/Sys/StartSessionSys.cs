using System;
using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Service;
using _Main.Scripts._ProjectAgnostic.Providers.SaveLoading;
using _Main.Scripts._ProjectAgnostic.Providers.Serialization;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Ecs.GameSaving.Data;
using _Main.Scripts.Ecs.Scoring.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Events;
using Arch.Core;
using UnityEngine;

namespace _Main.Scripts.Ecs.SessionStarting.Sys
{
	/// <summary>
	/// Starts the gameplay session on top of the singletons created by <see cref="InitializeSessionSys"/>:
	/// either loads the persisted save data into them, or initializes a fresh session. Extend
	/// <see cref="TryRestoreFromSaveData"/> alongside <c>SaveGameStateSys</c> when adding
	/// game-specific save sections.
	/// </summary>
	public sealed class StartSessionSys : IEcsInitSystem
	{
		private readonly World _world;
		private readonly IGameplaySessionLifetimeService _gameplaySessionLifetimeService;
		private readonly IGameplaySerializationProvider _serializationProvider;
		private readonly IPersistantStorageProvider _storageProvider;
		private readonly IGameplayEventBusService _eventBus;

		public StartSessionSys(
			World world,
			IGameplaySessionLifetimeService gameplaySessionLifetimeService,
			IGameplaySerializationProvider serializationProvider,
			IPersistantStorageProvider storageProvider,
			IGameplayEventBusService eventBus)
		{
			_world = world;
			_gameplaySessionLifetimeService = gameplaySessionLifetimeService;
			_serializationProvider = serializationProvider;
			_storageProvider = storageProvider;
			_eventBus = eventBus;
		}

		public void OnInit()
		{
			bool wasRestored = TryRestoreFromSaveData();

			if (!wasRestored)
				_world.Create(new RecordSessionStartReq());

			_eventBus.Publish(new OnGameSessionStartedEvent(wasRestored));
		}

		/// <summary>
		/// Restores the session state from the save payload. The score/combo fields are written directly
		/// into the live <see cref="ScoreSinglCmp"/> here (rather than deferred via an <see cref="UpdateScoreReq"/>
		/// payload) so that <see cref="InitializeTimeAfterLoadedSaveReq"/> — processed by
		/// <c>UpdateGameplayTimeSys</c> ahead of <c>UpdateScoreSys</c> in the same tick — sees the restored combo
		/// state and can correctly shift/clear/restore it. The <see cref="UpdateScoreReq"/> raised afterwards only
		/// triggers a UI refresh; it carries no payload.
		/// </summary>
		private bool TryRestoreFromSaveData()
		{
			if (!_gameplaySessionLifetimeService.ShouldGameSessionBeRestored)
				return false;

			var storageReadResult = _storageProvider.Read();
			if (storageReadResult.IsT1)
				return false;

			var deserializationResult = _serializationProvider.Deserialize<GameSaveData>(storageReadResult.AsT0);
			if (deserializationResult.IsT1)
			{
				Debug.LogError($"[{nameof(StartSessionSys)}] Failed to deserialize save data: {deserializationResult.AsT1.Value}");
				return false;
			}

			var saveData = deserializationResult.AsT0.Value;

			if (saveData.Time.SessionStartTimeStamp <= 0)
			{
				Debug.LogError($"[{nameof(StartSessionSys)}] Save data carries no started session. Starting a new session instead.");
				return false;
			}

			RestoreTime(saveData.Time);
			RestoreScore(saveData.Score);

			_world.Create(new InitializeTimeAfterLoadedSaveReq());
			_world.Create(new UpdateScoreReq { Kind = ScoreUpdateRequestKind.Restore });

			return true;
		}

		private void RestoreTime(TimeSaveData timeSaveData)
		{
			ref var timeCmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();
			timeCmp.SessionStartTimeStamp = timeSaveData.SessionStartTimeStamp;
			timeCmp.LastRecordedServerTimeStamp = timeSaveData.LastRecordedServerTimeStamp;
			timeCmp.RemainingPauseDurationInSeconds = timeSaveData.RemainingPauseDurationInSeconds;
		}

		private void RestoreScore(ScoreSaveData scoreSaveData)
		{
			ref var scoreCmp = ref _world.GetSinglCmp<ScoreSinglCmp>();
			scoreCmp.CurrentScore = scoreSaveData.CurrentScore;
			scoreCmp.CurrentMultiplier = scoreSaveData.CurrentMultiplier;
			scoreCmp.ComboActivated = scoreSaveData.ComboActivated;
			scoreCmp.LastComboTriggerTimestamp = scoreSaveData.LastComboTriggerTimestampUnixSeconds.HasValue
				? DateTimeOffset.FromUnixTimeSeconds(scoreSaveData.LastComboTriggerTimestampUnixSeconds.Value).UtcDateTime
				: (DateTime?)null;
		}
	}
}
