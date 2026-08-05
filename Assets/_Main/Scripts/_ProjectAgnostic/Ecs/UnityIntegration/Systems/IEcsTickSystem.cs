namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems
{
	public interface IEcsTickSystem : IEcsSystem
	{
		void OnTick();
	}
}
