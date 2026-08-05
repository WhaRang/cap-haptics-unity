using System;

namespace _Main.Scripts._ProjectAgnostic.Providers.Time
{
	/// <summary>
	/// The single time authority for gameplay. In an offline/local build this is the device clock;
	/// once the game is connected to a backend, register a provider that returns the server time
	/// so session/pause/combo windows can't be manipulated by changing the device clock.
	/// </summary>
	public interface IServerTimeProvider
	{
		DateTime GetCurrentTime();
	}
}
