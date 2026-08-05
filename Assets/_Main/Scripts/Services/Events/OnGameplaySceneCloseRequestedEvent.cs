using _Main.Scripts._ProjectAgnostic.Services.EventBus;

namespace _Main.Scripts.Services.Events
{
	/// <summary>
	/// Published by <c>CloseGameplaySceneSys</c> when the session is over and the gameplay
	/// scene should shut down. The game manager listens for this and stops the ECS runtime
	/// on the next frame.
	/// </summary>
	public readonly struct OnGameplaySceneCloseRequestedEvent : IEventBusEvent
	{
		
	}
}
