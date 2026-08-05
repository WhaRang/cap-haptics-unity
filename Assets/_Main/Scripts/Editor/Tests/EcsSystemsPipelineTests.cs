using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using NUnit.Framework;

namespace _Main.Scripts._ProjectAgnostic.Tests.Editor
{
	public class EcsSystemsPipelineTests
	{
		private class MockInitSystem : IEcsInitSystem
		{
			public int InitCallCount { get; private set; }

			public void OnInit()
			{
				InitCallCount++;
			}
		}

		private class MockTickSystem : IEcsTickSystem
		{
			public int TickCallCount { get; private set; }

			public void OnTick()
			{
				TickCallCount++;
			}
		}

		private class MockDisposeSystem : IEcsDisposeSystem
		{
			public int DisposeCallCount { get; private set; }

			public void OnDispose()
			{
				DisposeCallCount++;
			}
		}

		private class MockCompositeSystem : IEcsInitSystem, IEcsTickSystem, IEcsDisposeSystem
		{
			public int InitCallCount { get; private set; }
			public int TickCallCount { get; private set; }
			public int DisposeCallCount { get; private set; }

			public void OnInit()
			{
				InitCallCount++;
			}

			public void OnTick()
			{
				TickCallCount++;
			}

			public void OnDispose()
			{
				DisposeCallCount++;
			}
		}

		private EcsSystemsPipeline _pipeline = null!;

		[SetUp]
		public void SetUp()
		{
			_pipeline = new EcsSystemsPipeline();
		}

		[Test]
		public void FireInitSystems_WithNoSystems_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => _pipeline.FireInitSystems());
		}

		[Test]
		public void FireTickSystems_WithNoSystems_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => _pipeline.FireTickSystems());
		}

		[Test]
		public void FireDisposeSystems_WithNoSystems_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => _pipeline.FireDisposeSystems());
		}

		[Test]
		public void FireInitSystems_WithInitSystem_CallsOnInit()
		{
			// Arrange
			var system = new MockInitSystem();
			_pipeline.Add(system);

			// Act
			_pipeline.FireInitSystems();

			// Assert
			Assert.AreEqual(1, system.InitCallCount);
		}

		[Test]
		public void FireInitSystems_WithMultipleInitSystems_CallsAllOnInit()
		{
			// Arrange
			var system1 = new MockInitSystem();
			var system2 = new MockInitSystem();
			_pipeline.Add(system1);
			_pipeline.Add(system2);

			// Act
			_pipeline.FireInitSystems();

			// Assert
			Assert.AreEqual(1, system1.InitCallCount);
			Assert.AreEqual(1, system2.InitCallCount);
		}

		[Test]
		public void FireTickSystems_CalledMultipleTimes_InvokesSystemsEachTime()
		{
			// Arrange
			var system = new MockTickSystem();
			_pipeline.Add(system);

			// Act
			_pipeline.FireTickSystems();
			_pipeline.FireTickSystems();
			_pipeline.FireTickSystems();

			// Assert
			Assert.AreEqual(3, system.TickCallCount);
		}

		[Test]
		public void FireDisposeSystems_WithDisposeSystem_CallsOnDispose()
		{
			// Arrange
			var system = new MockDisposeSystem();
			_pipeline.Add(system);

			// Act
			_pipeline.FireDisposeSystems();

			// Assert
			Assert.AreEqual(1, system.DisposeCallCount);
		}

		[Test]
		public void FireInitSystems_DoesNotInvokeTickOrDispose()
		{
			// Arrange
			var tickSystem = new MockTickSystem();
			var disposeSystem = new MockDisposeSystem();
			_pipeline.Add(tickSystem);
			_pipeline.Add(disposeSystem);

			// Act
			_pipeline.FireInitSystems();

			// Assert
			Assert.AreEqual(0, tickSystem.TickCallCount);
			Assert.AreEqual(0, disposeSystem.DisposeCallCount);
		}

		[Test]
		public void Add_WithCompositeSystem_RegistersAllPhases()
		{
			// Arrange
			var system = new MockCompositeSystem();
			_pipeline.Add(system);

			// Act
			_pipeline.FireInitSystems();
			_pipeline.FireTickSystems();
			_pipeline.FireDisposeSystems();

			// Assert
			Assert.AreEqual(1, system.InitCallCount);
			Assert.AreEqual(1, system.TickCallCount);
			Assert.AreEqual(1, system.DisposeCallCount);
		}

		[Test]
		public void Clear_AfterAddingSystems_RemovesAllRegistrations()
		{
			// Arrange
			var system = new MockCompositeSystem();
			_pipeline.Add(system);

			// Act
			_pipeline.Clear();
			_pipeline.FireInitSystems();
			_pipeline.FireTickSystems();
			_pipeline.FireDisposeSystems();

			// Assert
			Assert.AreEqual(0, system.InitCallCount);
			Assert.AreEqual(0, system.TickCallCount);
			Assert.AreEqual(0, system.DisposeCallCount);
		}
	}
}
