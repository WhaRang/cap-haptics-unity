using System;

namespace _Main.Scripts._ProjectAgnostic.Services.EventBus
{
	public interface IGameplayEventBusService
	{
		void Subscribe<T>(Action<T> action) where T : IEventBusEvent;
		void Unsubscribe<T>(Action<T> action) where T : IEventBusEvent;
		void Publish<T>(T publishableEvent) where T : IEventBusEvent;
	}
}
