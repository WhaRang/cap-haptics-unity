using System;

namespace _Main.Scripts._ProjectAgnostic.Utils
{
	public static class DateTimeExtensions
	{
		public static long ToUnixTimeSeconds(this DateTime dateTime)
		{
			return ((DateTimeOffset)dateTime.ToUniversalTime()).ToUnixTimeSeconds();
		}
	}
}
