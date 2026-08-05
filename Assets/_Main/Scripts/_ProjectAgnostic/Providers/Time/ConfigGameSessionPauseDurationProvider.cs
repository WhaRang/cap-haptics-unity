using _Main.Scripts.Configs.Time;
using JetBrains.Annotations;

namespace _Main.Scripts._ProjectAgnostic.Providers.Time
{
	/// <summary>
	/// Default pause budget sourced from <see cref="ITimeConfig"/>. Replace with a
	/// backend-driven provider when the allowed pause time comes from a server.
	/// </summary>
	[UsedImplicitly]
	public sealed class ConfigGameSessionPauseDurationProvider : IGameSessionPauseDurationProvider
	{
		private readonly ITimeConfig _timeConfig;

		public int MaximumPauseDurationInSeconds => _timeConfig.MaximumPauseDurationInSeconds;

		public ConfigGameSessionPauseDurationProvider(ITimeConfig timeConfig)
		{
			_timeConfig = timeConfig;
		}
	}
}
