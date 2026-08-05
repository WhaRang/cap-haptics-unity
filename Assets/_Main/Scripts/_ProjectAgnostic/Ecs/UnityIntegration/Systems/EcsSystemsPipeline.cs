using System;

namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems
{
	public sealed class EcsSystemsPipeline : IEcsSystemsPipeline
	{
		private event Action? OnInit;
		private event Action? OnTick;
		private event Action? OnDispose;
		
		public void Add<T>(T system) where T : IEcsSystem
		{
			if (system is IEcsInitSystem startSystem)
				OnInit += startSystem.OnInit;

			if (system is IEcsTickSystem updateSystem)
				OnTick += updateSystem.OnTick;

			if (system is IEcsDisposeSystem destroySystem)
				OnDispose += destroySystem.OnDispose;
		}

		public void Clear()
		{
			OnInit = null;
			OnTick = null;
			OnDispose = null;
		}

		public void FireInitSystems()
		{
			OnInit?.Invoke();
		}
		
		public void FireTickSystems()
		{
			OnTick?.Invoke();
		}
		
		public void FireDisposeSystems()
		{
			OnDispose?.Invoke();
		}
	}
}
