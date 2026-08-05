using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace _Main.Scripts._ProjectAgnostic.Services.EventBus
{
	[UsedImplicitly]
	public sealed class GameplayEventBusService : IGameplayEventBusService
	{
		private readonly Dictionary<Type, HashSet<Delegate>> _subscribers = new();
		
		public void Subscribe<T>(Action<T> action) where T : IEventBusEvent
		{
			var eventType = typeof(T);
			if (!_subscribers.TryGetValue(eventType, out var actions))
			{
				actions = new HashSet<Delegate>();
				_subscribers[eventType] = actions;
			}

			if (!actions.Add(action))
				Debug.LogError($"[{nameof(GameplayEventBusService)}]: Attempted to subscribe to event {eventType.Name} with an already registered action.");
		}

		public void Unsubscribe<T>(Action<T> action) where T : IEventBusEvent
		{
			var eventType = typeof(T);
			if (_subscribers.TryGetValue(eventType, out var actions))
			{
				if (actions.Remove(action))
					return;

				Debug.LogError($"[{nameof(GameplayEventBusService)}]: Attempted to unsubscribe from event {eventType.Name} with an action that was not registered.");
				return;
			}
			
			Debug.LogError($"[{nameof(GameplayEventBusService)}]: Attempted to unsubscribe from event {eventType.Name} but no subscribers were found.");
		}
		
		public void Publish<T>(T publishableEvent) where T : IEventBusEvent
		{
			var eventType = typeof(T);

			if (!_subscribers.TryGetValue(eventType, out var actions))
			{
				Debug.LogError($"[{nameof(GameplayEventBusService)}]: No subscribers found for event {eventType.Name}.");
				return;
			}

			foreach (var action in actions)
			{
				Assert.IsTrue(action is Action<T>, $"[{nameof(GameplayEventBusService)}]: Action for event {eventType.Name} is not of the expected type. Expected: Action<{eventType.Name}>");
				try
				{
					((Action<T>)action).Invoke(publishableEvent);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[{nameof(GameplayEventBusService)}]: Exception occurred while invoking action for event {eventType.Name}. Exception:\n{ex}");
					Debug.LogException(ex);
				}
			}
		}
	}
}
