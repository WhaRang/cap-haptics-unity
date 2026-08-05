using System;
using JetBrains.Annotations;

namespace _Main.Scripts._ProjectAgnostic.Providers.Time
{
	[UsedImplicitly]
	public sealed class LocalServerTimeProvider : IServerTimeProvider
	{
		public DateTime GetCurrentTime() => DateTime.UtcNow;
	}
}
