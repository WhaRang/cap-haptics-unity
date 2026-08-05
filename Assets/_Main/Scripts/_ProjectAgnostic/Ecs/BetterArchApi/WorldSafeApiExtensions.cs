using System.Runtime.CompilerServices;
using Arch.Core;
using UnityEngine.Assertions;

namespace _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi
{
	public static class WorldSafeApiExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1>(this World world, in Entity entity)
			where T1 : struct
		{
			var has = world.Has<T1>(entity);
			Assert.IsFalse(has, $"Entity {entity} already has component {typeof(T1)}");
			if (!has) world.Add<T1>(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1>(this World world, in Entity entity, T1 t1)
			where T1 : struct
		{
			var has = world.Has<T1>(entity);
			Assert.IsFalse(has, $"Entity {entity} already has component {typeof(T1)}");
			if (!has) world.Add(entity, t1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2>(this World world, in Entity entity, T1 t1, T2 t2)
			where T1 : struct where T2 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3)
			where T1 : struct where T2 : struct where T3 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4, T5>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
			where T5 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
			world.SAdd(entity, t5);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4, T5, T6>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
			where T5 : struct where T6 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
			world.SAdd(entity, t5);
			world.SAdd(entity, t6);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4, T5, T6, T7>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
			where T5 : struct where T6 : struct where T7 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
			world.SAdd(entity, t5);
			world.SAdd(entity, t6);
			world.SAdd(entity, t7);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4, T5, T6, T7, T8>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
			where T5 : struct where T6 : struct where T7 : struct where T8 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
			world.SAdd(entity, t5);
			world.SAdd(entity, t6);
			world.SAdd(entity, t7);
			world.SAdd(entity, t8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
			where T5 : struct where T6 : struct where T7 : struct where T8 : struct
			where T9 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
			world.SAdd(entity, t5);
			world.SAdd(entity, t6);
			world.SAdd(entity, t7);
			world.SAdd(entity, t8);
			world.SAdd(entity, t9);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this World world, in Entity entity, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10)
			where T1 : struct where T2 : struct where T3 : struct where T4 : struct
			where T5 : struct where T6 : struct where T7 : struct where T8 : struct
			where T9 : struct where T10 : struct
		{
			world.SAdd(entity, t1);
			world.SAdd(entity, t2);
			world.SAdd(entity, t3);
			world.SAdd(entity, t4);
			world.SAdd(entity, t5);
			world.SAdd(entity, t6);
			world.SAdd(entity, t7);
			world.SAdd(entity, t8);
			world.SAdd(entity, t9);
			world.SAdd(entity, t10);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SSet<T>(this World world, in Entity entity, T value) where T : struct
		{
			var has = world.Has<T>(entity);
			Assert.IsTrue(has, $"Entity {entity} does not have component {typeof(T)} to set");
			if (has) world.Set(entity, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SRemove<T>(this World world, in Entity entity) where T : struct
		{
			if (world.Has<T>(entity))
				world.Remove<T>(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SDestroy(this World world, in Entity entity)
		{
			if (world.IsAlive(entity))
				world.Destroy(entity);
		}
	}
}
