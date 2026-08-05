using System;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Editor.Tests;
using _Main.Scripts.Services.Events;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace _Main.Scripts._ProjectAgnostic.Tests.Editor
{
	[TestFixture]
	public class GameplayEventBusServiceTests
	{
		private GameplayEventBusService _eventBusService = null!;

		[SetUp]
		public void SetUp()
		{
			_eventBusService = Create.GameplayEventBusService();
			// The bus reports misuse (duplicate subscribe, publish without subscribers, subscriber
			// exceptions) via Debug.LogError; several tests exercise those paths intentionally.
			LogAssert.ignoreFailingMessages = true;
		}

		[TearDown]
		public void TearDown()
		{
			LogAssert.ignoreFailingMessages = false;
		}

		private static OnGameSessionStartedEvent SampleEvent() => new(wasRestoredFromSave: false);

		[Test]
		public void Subscribe_WithValidAction_ShouldRegisterAction()
		{
			// Arrange
			bool eventReceived = false;
			Action<OnGameSessionStartedEvent> action = _ => eventReceived = true;

			// Act
			_eventBusService.Subscribe(action);

			// Assert
			_eventBusService.Publish(SampleEvent());
			Assert.IsTrue(eventReceived);
		}

		[Test]
		public void Subscribe_WithSameActionTwice_ShouldNotRegisterDuplicate()
		{
			// Arrange
			int callCount = 0;
			Action<OnGameSessionStartedEvent> action = _ => callCount++;

			// Act
			_eventBusService.Subscribe(action);
			_eventBusService.Subscribe(action); // Same action again

			// Assert
			_eventBusService.Publish(SampleEvent());
			Assert.AreEqual(1, callCount); // Should only be called once
		}

		[Test]
		public void Subscribe_WithDifferentActions_ShouldRegisterBoth()
		{
			// Arrange
			int callCount = 0;
			Action<OnGameSessionStartedEvent> action1 = _ => callCount++;
			Action<OnGameSessionStartedEvent> action2 = _ => callCount++;

			// Act
			_eventBusService.Subscribe(action1);
			_eventBusService.Subscribe(action2);

			// Assert
			_eventBusService.Publish(SampleEvent());
			Assert.AreEqual(2, callCount);
		}

		[Test]
		public void Unsubscribe_WithRegisteredAction_ShouldRemoveAction()
		{
			// Arrange
			int callCount = 0;
			Action<OnGameSessionStartedEvent> action = _ => callCount++;
			_eventBusService.Subscribe(action);

			// Act
			_eventBusService.Unsubscribe(action);

			// Assert
			_eventBusService.Publish(SampleEvent());
			Assert.AreEqual(0, callCount);
		}

		[Test]
		public void Unsubscribe_WithUnregisteredAction_ShouldNotThrow()
		{
			// Arrange
			Action<OnGameSessionStartedEvent> action = _ => { };

			// Act & Assert
			Assert.DoesNotThrow(() => _eventBusService.Unsubscribe(action));
		}

		[Test]
		public void Publish_WithNoSubscribers_ShouldNotThrow()
		{
			// Act & Assert
			Assert.DoesNotThrow(() => _eventBusService.Publish(SampleEvent()));
		}

		[Test]
		public void Publish_WithExceptionInSubscriber_ShouldNotStopOtherSubscribers()
		{
			// Arrange
			bool otherSubscriberCalled = false;
			Action<OnGameSessionStartedEvent> throwingAction = _ => throw new Exception("Test exception");
			Action<OnGameSessionStartedEvent> normalAction = _ => otherSubscriberCalled = true;

			_eventBusService.Subscribe(throwingAction);
			_eventBusService.Subscribe(normalAction);

			// Act & Assert
			Assert.DoesNotThrow(() => _eventBusService.Publish(SampleEvent()));
			Assert.IsTrue(otherSubscriberCalled);
		}

		[Test]
		public void Publish_WithDifferentEventTypes_ShouldOnlyCallCorrectSubscribers()
		{
			// Arrange
			int sessionEventCount = 0;
			int timeEventCount = 0;
			Action<OnGameSessionStartedEvent> sessionAction = _ => sessionEventCount++;
			Action<OnTimeUpdatedEvent> timeAction = _ => timeEventCount++;

			_eventBusService.Subscribe(sessionAction);
			_eventBusService.Subscribe(timeAction);

			// Act
			_eventBusService.Publish(SampleEvent());
			_eventBusService.Publish(new OnTimeUpdatedEvent(false, false, 0, 100));

			// Assert
			Assert.AreEqual(1, sessionEventCount);
			Assert.AreEqual(1, timeEventCount);
		}
	}
}
