namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems
{
	public interface IEcsSystemsPipeline
	{
		void Add<T>(T system) where T : IEcsSystem;
		void Clear();
		
		void FireInitSystems();
		void FireTickSystems();
		void FireDisposeSystems();
	}
}
