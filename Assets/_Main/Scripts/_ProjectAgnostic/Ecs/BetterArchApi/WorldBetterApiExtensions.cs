using System;
using Arch.Core;

namespace _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi
{
	public static class WorldBetterApiExtensions
	{
		public static bool HasSinglCmp<T>(this World world) where T : struct
		{
			var queryDescription = new QueryDescription().WithAll<T>();
			return world.CountEntities(queryDescription) == 1;
		}

		public static Entity GetSinglEntity<T>(this World world) where T : struct
		{
			world.GetSinglCmpWithEntity<T>(out var entity);
			return entity;
		}
		
		public static ref T GetSinglCmp<T>(this World world) where T : struct
		{
			var entity = world.GetSinglEntity<T>();
			return ref world.Get<T>(entity);
		}

		public static ref T GetSinglCmpWithEntity<T>(this World world, out Entity entity) where T : struct
		{
			var queryDescription = new QueryDescription().WithAll<T>();
			var query = world.Query(queryDescription);

			int entitiesCount = query.EntitiesCount();
			
			if(entitiesCount < 1)
				throw new Exception($"No singleton entity found with component {typeof(T).Name}");
			
			if(entitiesCount > 1)
				throw new Exception($"Multiple entities found with component {typeof(T).Name}");
			
			if(query.TryGetFirstEntity(out entity))
				return ref world.Get<T>(entity);
			
			throw new Exception($"Failed to get singleton entity with component {typeof(T).Name}");
		}

		public static void RemoveIfPresent<T>(this World world, in Entity entity) where T : struct
		{
			if (world.Has<T>(entity))
				world.Remove<T>(entity);
		}

		public static ref T GetOrAdd<T>(this World world, in Entity entity) where T : struct
		{
			if (world.Has<T>(entity))
				return ref world.Get<T>(entity);
			
			world.Add<T>(entity);
			return ref world.Get<T>(entity);
		}

		public static void DestroyIfAlive(this World world, in Entity entity)
		{
			if (world.IsAlive(entity))
				world.Destroy(entity);
		}
		
		public static void RemoveAllComponentsInQuery<T>(this World world, Query query) where T : struct
		{
			using var cb = new SCommandBuffer();

			foreach (var chunk in query)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);
					cb.Remove<T>(entity);
				}
			}

			cb.Playback(world);
		}

		public static void DestroyAllEntitiesInQuery(this World world, Query query)
		{
			using var cb = new SCommandBuffer();

			foreach (var chunk in query)
			{
				foreach (int i in chunk)
				{
					ref var entity = ref chunk.Entity(i);
					cb.Destroy(entity);
				}
			}

			cb.Playback(world);
		}
	}
}
