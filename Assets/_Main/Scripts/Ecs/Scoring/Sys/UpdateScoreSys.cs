using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.Providers.Time;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Configs.Scoring;
using _Main.Scripts.Ecs.GameSaving.Cmp;
using _Main.Scripts.Ecs.Scoring.Cmp;
using _Main.Scripts.Services.Events;
using Arch.Core;
using UnityEngine;

namespace _Main.Scripts.Ecs.Scoring.Sys
{
	/// <summary>
	/// The single writer of <see cref="ScoreSinglCmp"/>: consumes <see cref="UpdateScoreReq"/> requests
	/// produced anywhere in the game, applies the combo multiplier on gains, and publishes
	/// <see cref="OnScoreUpdatedEvent"/> for the UI. Every non-restore update also requests a save.
	/// </summary>
	public sealed class UpdateScoreSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly IGameplayEventBusService _eventBus;
		private readonly IScoringConfig _scoringConfig;
		private readonly IServerTimeProvider _serverTimeProvider;
		private readonly Query _updateScoreReqQuery;

		public UpdateScoreSys(
			World world,
			IGameplayEventBusService eventBus,
			IScoringConfig scoringConfig,
			IServerTimeProvider serverTimeProvider)
		{
			_world = world;
			_eventBus = eventBus;
			_scoringConfig = scoringConfig;
			_serverTimeProvider = serverTimeProvider;
			_updateScoreReqQuery = world.Query(new QueryDescription().WithAll<UpdateScoreReq>());
		}

		public void OnTick()
		{
			if (_updateScoreReqQuery.IsEmpty())
				return;

			ref var scoreCmp = ref _world.GetSinglCmp<ScoreSinglCmp>();
			var shouldRequestSave = false;
			using var commandBuffer = new SCommandBuffer();

			foreach (var chunk in _updateScoreReqQuery)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);
					var request = _world.Get<UpdateScoreReq>(entity);

					ApplyUpdate(ref scoreCmp, request);
					shouldRequestSave |= request.Kind != ScoreUpdateRequestKind.Restore;
					commandBuffer.Remove<UpdateScoreReq>(entity);
				}
			}

			commandBuffer.Playback(_world);

			if (shouldRequestSave)
				_world.Create(new OnGameSavingRequestedEvt());
		}

		private void ApplyUpdate(ref ScoreSinglCmp scoreCmp, UpdateScoreReq request)
		{
			var previousScore = scoreCmp.CurrentScore;
			ScoreUpdateKind updateKind;

			switch (request.Kind)
			{
				case ScoreUpdateRequestKind.Gain:
					UpdateCombo(ref scoreCmp);
					scoreCmp.CurrentScore += request.Points * scoreCmp.CurrentMultiplier;
					updateKind = ScoreUpdateKind.Gain;
					break;
				case ScoreUpdateRequestKind.Penalty:
					scoreCmp.CurrentScore = Mathf.Max(0, scoreCmp.CurrentScore - request.Points);
					updateKind = ScoreUpdateKind.Penalty;
					break;
				case ScoreUpdateRequestKind.Restore:
					// StartSessionSys writes the restored values into ScoreSinglCmp directly (so the same-tick
					// combo pause-shift in UpdateGameplayTimeSys sees the restored combo state). This request
					// only triggers the UI refresh below — it carries no payload to apply.
					updateKind = ScoreUpdateKind.Restore;
					break;
				default:
					throw new System.ArgumentOutOfRangeException(nameof(request), request.Kind, null);
			}

			_eventBus.Publish(new OnScoreUpdatedEvent(previousScore, scoreCmp.CurrentScore, scoreCmp.CurrentMultiplier, updateKind));
		}

		/// <summary>
		/// Advances the combo on a gain using the time-provider decay window. The first gain activates the
		/// combo at <see cref="IScoringConfig.MinComboMultiplier"/>; each consecutive gain inside the current
		/// tier's window increments the multiplier (clamped to <see cref="IScoringConfig.MaxComboMultiplier"/>);
		/// a gain after the window elapsed resets to the minimum and re-activates.
		/// </summary>
		private void UpdateCombo(ref ScoreSinglCmp scoreCmp)
		{
			var now = _serverTimeProvider.GetCurrentTime().ToUniversalTime();

			var withinWindow = scoreCmp.ComboActivated && scoreCmp.LastComboTriggerTimestamp.HasValue &&
				(now - scoreCmp.LastComboTriggerTimestamp.Value).TotalSeconds <= _scoringConfig.GetComboDurationInSeconds(scoreCmp.CurrentMultiplier);

			if (!scoreCmp.ComboActivated)
				scoreCmp.CurrentMultiplier = _scoringConfig.MinComboMultiplier;
			else if (withinWindow)
				scoreCmp.CurrentMultiplier = Mathf.Min(scoreCmp.CurrentMultiplier + 1, _scoringConfig.MaxComboMultiplier);
			else
				scoreCmp.CurrentMultiplier = _scoringConfig.MinComboMultiplier;

			scoreCmp.ComboActivated = true;
			scoreCmp.LastComboTriggerTimestamp = now;
		}
	}
}
