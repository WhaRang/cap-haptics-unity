using System;
using System.Collections.Generic;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using Arch.Core;

namespace _Main.Scripts.Ecs
{
	/// <summary>
	/// Owns the ECS world lifecycle for a gameplay session: on <see cref="Start"/> it bakes the
	/// registered scene bakers into the world and fires the init systems; the game manager drives
	/// <see cref="Tick"/> from <c>Update</c>; <see cref="Stop"/> fires the dispose systems and
	/// clears the world so the next session starts clean.
	/// </summary>
	public sealed class EcsRuntime : IDisposable
	{
		private readonly World _ecsWorld;
		private readonly IEcsBakingService _bakingService;
		private readonly IReadOnlyList<IEcsSystem> _ecsSystems;
		private readonly EcsSystemsPipeline _ecsSystemsPipeline;

		private bool _isRunning;

		public bool IsRunning => _isRunning;

		public EcsRuntime(World ecsWorld, IEcsBakingService bakingService, IReadOnlyList<IEcsSystem> ecsSystems)
		{
			_ecsWorld = ecsWorld;
			_bakingService = bakingService;
			_ecsSystems = ecsSystems;
			_ecsSystemsPipeline = new EcsSystemsPipeline();
		}

		private void SetupSystems()
		{
			_ecsSystemsPipeline.Clear();

			foreach (var system in _ecsSystems)
			{
				_ecsSystemsPipeline.Add(system);
			}
		}

		public void Start()
		{
			_ecsWorld.Clear();
			SetupSystems();

			_bakingService.Bake(_ecsWorld);
			_ecsSystemsPipeline.FireInitSystems();
			_isRunning = true;
		}

		public void Stop()
		{
			_isRunning = false;
			_ecsSystemsPipeline.FireDisposeSystems();

			_ecsWorld.Clear();
			_ecsSystemsPipeline.Clear();
		}

		public void Tick()
		{
			if (_isRunning)
				_ecsSystemsPipeline.FireTickSystems();
		}

		public void Dispose()
		{
			_ecsSystemsPipeline.Clear();
			_ecsWorld.Dispose();
		}
	}
}
