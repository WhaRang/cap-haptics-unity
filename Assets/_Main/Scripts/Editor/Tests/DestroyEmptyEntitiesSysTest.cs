using _Main.Scripts._ProjectAgnostic.Ecs.CommonSystems;
using _Main.Scripts.Editor.Tests;
using NUnit.Framework;

namespace _Main.Scripts._ProjectAgnostic.Tests.Editor
{
	public sealed class DestroyEmptyEntitiesSysTest
	{
		private struct MockEcsComponent
		{
			
		}
		
		[Test]
		public void OnTick_DestroysEmptyEntities()
		{
			// Arrange
			var world = Create.EcsWorld();
			var sys = new DestroyEmptyEntitiesSys(world);
			var emptyEntity = world.Create();
			
			//Act
			sys.OnTick();
			
			//Assert
			Assert.IsFalse(world.IsAlive(emptyEntity));
		}

		[Test]
		public void OnTick_DoesNotDestroyNonEmptyEntities()
		{
			// Arrange
			var world = Create.EcsWorld();
			var sys = new DestroyEmptyEntitiesSys(world);
			var nonEmptyEntity = world.Create(new MockEcsComponent());

			//Act
			sys.OnTick();

			//Assert
			Assert.IsTrue(world.IsAlive(nonEmptyEntity));
		}
	}
}
