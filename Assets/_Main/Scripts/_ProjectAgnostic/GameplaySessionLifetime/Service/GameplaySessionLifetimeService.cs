using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Factory;
using UnityEngine;
using UnityEngine.Assertions;

namespace _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Service
{
	public sealed class GameplaySessionLifetimeService : IGameplaySessionLifetimeService
	{
		private readonly IGameplaySessionObjectFactory _gameplaySessionObjectFactory;
		
		private GameObject? _gameplaySessionParentGameObject;

		public bool ShouldGameSessionBeRestored { get; set; }

		public GameplaySessionLifetimeService(IGameplaySessionObjectFactory gameplaySessionObjectFactory)
		{
			_gameplaySessionObjectFactory = gameplaySessionObjectFactory;
		}

		public void InstantiateGameplaySessionGameObjects()
		{
			Assert.IsNull(_gameplaySessionParentGameObject, "Trying to instantiate multiple gameplay session game objects.");
			
			_gameplaySessionParentGameObject = _gameplaySessionObjectFactory.Create();
		}
		
		public void DisposeGameplaySessionGameObjects()
		{
			Assert.IsNotNull(_gameplaySessionParentGameObject, "Trying to dispose multiple gameplay session game objects.");

			if (Application.isPlaying)
				Object.Destroy(_gameplaySessionParentGameObject);
			else
				Object.DestroyImmediate(_gameplaySessionParentGameObject);

			_gameplaySessionParentGameObject = null;
		}
	}
}
