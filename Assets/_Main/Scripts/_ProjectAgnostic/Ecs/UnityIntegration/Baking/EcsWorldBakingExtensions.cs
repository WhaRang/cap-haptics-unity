using Arch.Core;
using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking
{
	public static class EcsWorldBakingExtensions
	{
		public static Entity Bake(this World world, BaseEcsMonoBaker baker)
		{
			var entity = world.Create();
			baker.Bake(world, entity);

			return entity;
		}
		
		public static Entity[] BakeAllInHierarchy(this World world, GameObject gameObject)
		{
			var bakers = gameObject.GetComponentsInChildren<BaseEcsMonoBaker>(includeInactive: true);
			var entities = new Entity[bakers.Length];
			for (int i = 0; i < bakers.Length; i++)
			{
				var entity = world.Create();
				bakers[i].Bake(world, entity);
				entities[i] = entity;
			}
			return entities;
		}
	}
}
