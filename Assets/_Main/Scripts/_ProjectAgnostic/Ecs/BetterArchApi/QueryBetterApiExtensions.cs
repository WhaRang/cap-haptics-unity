using Arch.Core;

namespace _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi
{
	public static class QueryBetterApiExtensions
	{
		public static bool IsEmpty(this Query query)
		{
			foreach (var chunk in query)
			{
				foreach (int unused in chunk)
				{
					return false;
				}
			}

			return true;
		}

		public static Entity GetFirstEntity(this Query query)
		{
			return query.TryGetFirstEntity(out var entity) ? entity : Entity.Null;
		}

		public static bool TryGetFirstEntity(this Query query, out Entity entity)
		{
			foreach (var chunk in query)
			{
				foreach (int i in chunk)
				{
					entity = chunk.Entity(i);
					return true;
				}
			}

			entity = Entity.Null;

			return false;
		}
		
		public static int EntitiesCount(this Query query)
		{
			int count = 0;
			
			foreach (var chunk in query)
			{
				count += chunk.Count;
			}

			return count;
		}
	}
}
