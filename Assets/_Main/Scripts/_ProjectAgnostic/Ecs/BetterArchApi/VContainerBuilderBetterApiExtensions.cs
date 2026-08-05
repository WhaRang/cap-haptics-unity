using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using Arch.Core;
using VContainer;

namespace _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi
{
	public static class VContainerBuilderBetterApiExtensions
	{
		public static void RegisterEcsWorld(this IContainerBuilder builder)
		{
			builder.RegisterInstance(World.Create()).As<World>();
		}
		
		public static void RegisterEcsBaking(this IContainerBuilder builder)
		{
			builder.Register<EcsBakingService>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		public static void RegisterEcsSystem<T>(this IContainerBuilder builder) where T : IEcsSystem
		{
			builder.Register<T>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}
