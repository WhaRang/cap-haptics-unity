using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking;
using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Factory;
using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Service;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using Arch.Core;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace _Main.Scripts.Editor.Tests
{
	public static partial class Create
	{
		public static T Baker<T>() where T : BaseEcsMonoBaker
		{
			var baker = new GameObject().AddComponent<T>();
			baker.BakerBakingStrategy = BakerBakingStrategy.BakeAndKeepBaker;
			return baker;
		}

		public static EcsBakingService EcsBakingService()
		{
			return new EcsBakingService();
		}

		public static World EcsWorld()
		{
			return World.Create();
		}

		public static GameplaySessionLifetimeService GameplaySessionLifetimeService(IGameplaySessionObjectFactory factory)
		{
			return new GameplaySessionLifetimeService(factory);
		}
		
		public static GameplayEventBusService GameplayEventBusService()
		{
			return new GameplayEventBusService();
		}
	}
}
