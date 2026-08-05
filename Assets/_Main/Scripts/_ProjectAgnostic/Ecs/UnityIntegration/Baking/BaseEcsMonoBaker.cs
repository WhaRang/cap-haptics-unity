using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.EntityReference;
using Arch.Core;
using UnityEngine;
using VContainer;

namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking
{
	public abstract class BaseEcsMonoBaker : MonoBehaviour
	{
		[SerializeField] private BakerBakingStrategy _bakerBakingStrategy;
		[SerializeField] private bool _createEntityReference;
		
		private IEcsBakingService _bakingService = null!;

		public BakerBakingStrategy BakerBakingStrategy
		{
			get => _bakerBakingStrategy;
			set => _bakerBakingStrategy = value;
		}

		public bool CreateEntityReference
		{
			get => _createEntityReference;
			set => _createEntityReference = value;
		}

		[Inject]
		public void Construct(IEcsBakingService bakingService)
		{
			_bakingService = bakingService;
		}

		private void Awake()
		{
			_bakingService.AddBaker(this);
		}

		private void OnDestroy()
		{
			_bakingService.RemoveBaker(this);
		}

		protected abstract void OnBake(World world, Entity entity);
		
		public void Bake(World world, Entity entity)
		{
			if (_createEntityReference)
			{
				if(TryGetComponent(out EcsMonoEntityReference entityReference))
					entityReference.Entity = entity;
				else
					gameObject.AddComponent<EcsMonoEntityReference>().Entity = entity;
			}
			
			switch (_bakerBakingStrategy)
			{
				case BakerBakingStrategy.BakeAndDestroyBaker:
					OnBake(world, entity);
					Destroy(this);
					break;
				case BakerBakingStrategy.BakeAndKeepBaker:
					OnBake(world, entity);
					break;
				case BakerBakingStrategy.BakeAndDestroyGameObject:
					OnBake(world, entity);
					Destroy(gameObject);
					break;
			}
		}
	}
	
	public enum BakerBakingStrategy
	{
		BakeAndDestroyBaker = 0,
		BakeAndKeepBaker = 1,
		BakeAndDestroyGameObject = 2,
		DontBake = 3
	}
}
