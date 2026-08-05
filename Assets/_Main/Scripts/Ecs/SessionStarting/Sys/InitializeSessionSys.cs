using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts.Configs.Scoring;
using _Main.Scripts.Ecs.Scoring.Cmp;
using _Main.Scripts.Ecs.Time.Cmp;
using Arch.Core;

namespace _Main.Scripts.Ecs.SessionStarting.Sys
{
	/// <summary>
	/// Creates the essential singleton ECS components every session needs, restored or fresh.
	/// After this system ran, every singleton required by the base runtime exists with its
	/// new-session defaults; <see cref="StartSessionSys"/> then overwrites them from the save data
	/// (or leaves them as-is for a new session) and produces the session startup requests.
	/// Extend it with your game's own singletons.
	/// </summary>
	public sealed class InitializeSessionSys : IEcsInitSystem
	{
		private readonly World _world;
		private readonly IScoringConfig _scoringConfig;

		public InitializeSessionSys(World world, IScoringConfig scoringConfig)
		{
			_world = world;
			_scoringConfig = scoringConfig;
		}

		public void OnInit()
		{
			_world.Create(new GameplayTimeSinglCmp { SessionStartTimeStamp = 0 });
			_world.Create(new ScoreSinglCmp { CurrentMultiplier = _scoringConfig.MinComboMultiplier });
		}
	}
}
