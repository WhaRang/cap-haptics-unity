using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Factory
{
	/// <summary>
	/// Instantiates the per-session gameplay hierarchy through the container so every
	/// <c>[Inject]</c>-annotated component in the prefab (bakers, views, ...) is injected.
	/// The prefab is destroyed as a whole when the session ends.
	/// </summary>
	public sealed class GameplaySessionObjectFactory : MonoBehaviour, IGameplaySessionObjectFactory
	{
		[SerializeField] private Transform _gameplayObjectParent = null!; 
		[SerializeField] private GameObject _gameplayObjectPrefab = null!;
		
		private IObjectResolver _objectResolver = null!;

		[Inject]
		public void Construct(IObjectResolver objectResolver)
		{
			_objectResolver = objectResolver;
		}
		
		public GameObject Create()
		{
			return _objectResolver.Instantiate(_gameplayObjectPrefab, _gameplayObjectParent); 
		}
	}
}
