namespace _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Service
{
	public interface IGameplaySessionLifetimeService
	{
		bool ShouldGameSessionBeRestored { get; set; }
		
		void InstantiateGameplaySessionGameObjects();
		void DisposeGameplaySessionGameObjects();
	}
}
