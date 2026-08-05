using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts.Editor.Tests;
using Arch.Core;
using NUnit.Framework;

namespace _Main.Scripts._ProjectAgnostic.Tests.Editor
{
	public sealed class SCommandBufferTests
	{
		private struct CompA { public int Value; }

		private World _world = null!;
		private SCommandBuffer _cb = null!;

		[SetUp]
		public void SetUp()
		{
			_world = Create.EcsWorld();
			_cb = new SCommandBuffer();
		}

		[TearDown]
		public void TearDown()
		{
			_cb.Dispose();
			_world.Dispose();
		}

		// --- Basics ---

		[Test]
		public void Playback_WithAddT_AddsComponent()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Add<CompA>(entity);

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.IsTrue(_world.Has<CompA>(entity));
		}

		[Test]
		public void Playback_WithAddTAndValue_StoresValueOnEntity()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Add(entity, new CompA { Value = 42 });

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.AreEqual(42, _world.Get<CompA>(entity).Value);
		}

		[Test]
		public void Playback_WithRemoveT_RemovesComponent()
		{
			// Arrange
			var entity = _world.Create(new CompA { Value = 1 });
			_cb.Remove<CompA>(entity);

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.IsFalse(_world.Has<CompA>(entity));
		}

		[Test]
		public void Playback_WithSetT_UpdatesComponentValue()
		{
			// Arrange
			var entity = _world.Create(new CompA { Value = 1 });
			_cb.Set(entity, new CompA { Value = 99 });

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.AreEqual(99, _world.Get<CompA>(entity).Value);
		}

		[Test]
		public void Playback_WithDestroy_KillsEntity()
		{
			// Arrange
			var entity = _world.Create(new CompA());
			_cb.Destroy(entity);

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.IsFalse(_world.IsAlive(entity));
		}

		[Test]
		public void Add_BeforePlayback_DoesNotMutateWorld()
		{
			// Arrange
			var entity = _world.Create();

			// Act
			_cb.Add<CompA>(entity);

			// Assert
			Assert.IsFalse(_world.Has<CompA>(entity));
		}

		// --- Silent skips (don't trip Assert) ---

		[Test]
		public void Playback_RemoveOnEntityWithoutComponent_LeavesEntityAlive()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Remove<CompA>(entity);

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.IsTrue(_world.IsAlive(entity));
		}

		[Test]
		public void Playback_DestroyAlreadyDeadEntity_DoesNotThrow()
		{
			// Arrange
			var entity = _world.Create();
			_world.Destroy(entity);
			_cb.Destroy(entity);

			// Act & Assert
			Assert.DoesNotThrow(() => _cb.Playback(_world));
		}

		// --- Ordering ---

		[Test]
		public void Playback_WithRemoveThenAddSameComponent_LeavesComponentPresent()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Remove<CompA>(entity);
			_cb.Add(entity, new CompA { Value = 3 });

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.IsTrue(_world.Has<CompA>(entity));
		}

		[Test]
		public void Playback_WithAddThenRemoveSameComponent_LeavesComponentAbsent()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Add(entity, new CompA { Value = 3 });
			_cb.Remove<CompA>(entity);

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.IsFalse(_world.Has<CompA>(entity));
		}

		[Test]
		public void Playback_PreservesInsertionOrderAcrossMixedOps()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Add(entity, new CompA { Value = 1 });
			_cb.Set(entity, new CompA { Value = 2 });

			// Act
			_cb.Playback(_world);

			// Assert
			Assert.AreEqual(2, _world.Get<CompA>(entity).Value);
		}

		// --- Lifecycle ---

		[Test]
		public void Playback_OnEmptyBuffer_DoesNotThrow()
		{
			// Arrange
			// Empty buffer.

			// Act & Assert
			Assert.DoesNotThrow(() => _cb.Playback(_world));
		}

		[Test]
		public void Playback_WithDisposeFalse_AllowsReuseForAnotherPlayback()
		{
			// Arrange
			var entityA = _world.Create();
			var entityB = _world.Create();
			_cb.Add<CompA>(entityA);
			_cb.Playback(_world, dispose: false);
			_cb.Add<CompA>(entityB);

			// Act
			_cb.Playback(_world, dispose: false);

			// Assert
			Assert.IsTrue(_world.Has<CompA>(entityB));
		}

		[Test]
		public void Reset_AfterEnqueueing_ClearsQueuedOps()
		{
			// Arrange
			var entity = _world.Create();
			_cb.Add<CompA>(entity);

			// Act
			_cb.Reset();
			_cb.Playback(_world);

			// Assert
			Assert.IsFalse(_world.Has<CompA>(entity));
		}

		[Test]
		public void Dispose_CalledTwice_DoesNotThrow()
		{
			// Arrange
			_cb.Dispose();

			// Act & Assert
			Assert.DoesNotThrow(() => _cb.Dispose());
		}
	}
}
