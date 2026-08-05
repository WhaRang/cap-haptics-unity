using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using UnityEngine.Assertions;

namespace _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi
{
	public sealed class SCommandBuffer : IDisposable
	{
		private readonly List<Action<World>> _commands = new();
		private bool _disposed; 

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T>(in Entity entity) where T : struct
		{
			AssertNotDisposed();
			var captured = entity;
			_commands.Add(world => world.SAdd<T>(captured));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T>(in Entity entity, T value) where T : struct
		{
			AssertNotDisposed();
			var captured = entity;
			_commands.Add(world => world.SAdd(captured, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set<T>(in Entity entity, T value) where T : struct
		{
			AssertNotDisposed();
			var captured = entity;
			_commands.Add(world => world.SSet(captured, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove<T>(in Entity entity) where T : struct
		{
			AssertNotDisposed();
			var captured = entity;
			_commands.Add(world => world.SRemove<T>(captured));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Destroy(in Entity entity)
		{
			AssertNotDisposed();
			var captured = entity;
			_commands.Add(world => world.SDestroy(captured));
		}

		public void Playback(World world, bool dispose = true)
		{
			AssertNotDisposed();

			foreach (var t in _commands)
			{
				t(world);
			}

			_commands.Clear();

			if (dispose)
				Dispose();
		}

		public void Reset()
		{
			_commands.Clear();
		}

		public void Dispose()
		{
			_commands.Clear();
			_disposed = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AssertNotDisposed()
		{
			Assert.IsFalse(_disposed, "SCommandBuffer is disposed; create a new instance or call Reset before reuse");
		}
	}
}
