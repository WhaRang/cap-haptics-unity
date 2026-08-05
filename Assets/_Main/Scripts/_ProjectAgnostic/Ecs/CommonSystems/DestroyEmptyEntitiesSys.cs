using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using Arch.Core;

namespace _Main.Scripts._ProjectAgnostic.Ecs.CommonSystems
{
	public sealed class DestroyEmptyEntitiesSys : IEcsTickSystem
	{
		private readonly World _world;

		public DestroyEmptyEntitiesSys(World world)
		{
			_world = world;
		}
		
		public void OnTick()
		{
			var archetypes = _world.Archetypes;
			for (int archetypeIndex = 0; archetypeIndex < archetypes.Count; archetypeIndex++)
			{
				var archetype = archetypes[archetypeIndex];

				if (archetype.Signature.Count <= 0)
				{
					foreach (var chunk in archetype)
					{
						foreach (int i in chunk)
						{
							ref var entity = ref chunk.Entity(i);
							_world.DestroyIfAlive(entity);
						}
					}
					
					break;
				}
			}
		}
	}
}
