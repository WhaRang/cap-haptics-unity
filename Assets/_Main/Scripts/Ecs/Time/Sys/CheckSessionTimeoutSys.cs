using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts.Ecs.SessionEnding.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using _Main.Scripts.Services.Time;
using Arch.Core;

namespace _Main.Scripts.Ecs.Time.Sys
{
	/// <summary>
	/// Dispatches an <see cref="EndGameSessionReq"/> with <see cref="EndGameReason.RunOutOfTime"/> once the
	/// session clock reaches zero. Register it after your win-condition system so that a win landing on the
	/// same tick the timer expires wins the race: if any other end reason has already created an
	/// <see cref="EndGameSessionReq"/> this tick, this system defers to it.
	/// </summary>
	public sealed class CheckSessionTimeoutSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly IGameplayTimeService _timeService;
		private readonly Query _endGameSessionReqQuery;

		public CheckSessionTimeoutSys(World world, IGameplayTimeService timeService)
		{
			_world = world;
			_timeService = timeService;
			_endGameSessionReqQuery = _world.Query(new QueryDescription().WithAll<EndGameSessionReq>());
		}

		public void OnTick()
		{
			if (!_endGameSessionReqQuery.IsEmpty())
				return;

			ref var cmp = ref _world.GetSinglCmp<GameplayTimeSinglCmp>();

			if (cmp.SessionStartTimeStamp <= 0 || _timeService.WasSessionFinished(in cmp) || _timeService.IsPaused(in cmp))
				return;

			if (_timeService.RemainingSessionTimeInSeconds(in cmp) > 0)
				return;

			_world.Create(new EndGameSessionReq { Reason = EndGameReason.RunOutOfTime });
		}
	}
}
