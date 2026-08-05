using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking;
using _Main.Scripts.Editor.Tests;
using Arch.Core;
using NUnit.Framework;
using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.Tests.Editor
{
	public class EcsBakingServiceTests
	{
		private sealed class MockBaker : BaseEcsMonoBaker
		{
			public int BakeCallCount { get; private set; }
			
			protected override void OnBake(World world, Entity entity)
			{
				BakeCallCount++;
			}
		}

		[Test]
		public void AddBaker_WithNewBaker_ReturnsSuccess()
		{
			// Arrange
			var bakingService = Create.EcsBakingService();
			var baker = Create.Baker<MockBaker>();

			// Act
			var result = bakingService.AddBaker(baker);

			// Assert
			Assert.IsTrue(result.IsT0);

			// Cleanup
			Object.DestroyImmediate(baker.gameObject);
		}

		[Test]
		public void AddBaker_WithSameBakerTwice_ReturnsErrorOnSecondAdd()
		{
			// Arrange
			var bakingService = Create.EcsBakingService();
			var baker = Create.Baker<MockBaker>();

			// Act
			bakingService.AddBaker(baker);
			var result = bakingService.AddBaker(baker);

			// Assert
			Assert.IsTrue(result.IsT1); // Error

			// Cleanup
			Object.DestroyImmediate(baker.gameObject);
		}

		[Test]
		public void Bake_WithNoBakers_DoesNotThrow()
		{
			// Arrange
			var bakingService = Create.EcsBakingService();
			var world = Create.EcsWorld();

			// Act & Assert
			Assert.DoesNotThrow(() => bakingService.Bake(world));

			// Cleanup
			World.Destroy(world);
		}

		[Test]
		public void Bake_WithMultipleBakers_CallsOnBakeOnEach()
		{
			// Arrange
			var bakingService = Create.EcsBakingService();
			var world = Create.EcsWorld();
			var baker1 = Create.Baker<MockBaker>();
			var baker2 = Create.Baker<MockBaker>();
			var baker3 = Create.Baker<MockBaker>();
			bakingService.AddBaker(baker1);
			bakingService.AddBaker(baker2);
			bakingService.AddBaker(baker3);

			// Act
			bakingService.Bake(world);

			// Assert
			Assert.AreEqual(1, baker1.BakeCallCount);
			Assert.AreEqual(1, baker2.BakeCallCount);
			Assert.AreEqual(1, baker3.BakeCallCount);

			// Cleanup
			Object.DestroyImmediate(baker1.gameObject);
			Object.DestroyImmediate(baker2.gameObject);
			Object.DestroyImmediate(baker3.gameObject);
			World.Destroy(world);
		}

		[Test]
		public void Bake_WhenBakerRemoved_DoesNotBakeIt()
		{
			// Arrange
			var bakingService = Create.EcsBakingService();
			var world = Create.EcsWorld();
			var baker = Create.Baker<MockBaker>();
			bakingService.AddBaker(baker);

			// Act
			bakingService.RemoveBaker(baker);
			bakingService.Bake(world);

			// Assert
			Assert.AreEqual(0, baker.BakeCallCount);

			// Cleanup
			Object.DestroyImmediate(baker.gameObject);
			World.Destroy(world);
		}
	}
}
