using System;
using UnityEngine;

namespace CapHaptics.Client
{
	public enum HapticsLogLevel
	{
		Info = 0,
		Warning = 1,
		Error = 2,
	}

	/// <summary>
	/// Where the SDK's C# layer sends its log lines. Inject one via
	/// <see cref="Haptics.SetLogger"/> to route them into your own pipeline — a file,
	/// an analytics backend, or a silent sink — instead of the Unity console.
	/// </summary>
	public interface IHapticsLogger
	{
		void Log(HapticsLogLevel level, string message);
	}

	/// <summary>
	/// The dispatcher every SDK log line goes through, so the no-throw guarantee holds
	/// even when the injected logger is the thing that throws.
	/// </summary>
	internal static class HapticsLog
	{
		private sealed class UnityLogger : IHapticsLogger
		{
			public void Log(HapticsLogLevel level, string message)
			{
				switch (level)
				{
					case HapticsLogLevel.Error: Debug.LogError(message); break;
					case HapticsLogLevel.Warning: Debug.LogWarning(message); break;
					default: Debug.Log(message); break;
				}
			}
		}

		private static readonly IHapticsLogger Default = new UnityLogger();
		private static IHapticsLogger _current = Default;

		internal static void Set(IHapticsLogger? logger) => _current = logger ?? Default;

		internal static void Info(string message) => Write(HapticsLogLevel.Info, message);
		internal static void Warning(string message) => Write(HapticsLogLevel.Warning, message);
		internal static void Error(string message) => Write(HapticsLogLevel.Error, message);

		private static void Write(HapticsLogLevel level, string message)
		{
			try
			{
				_current.Log(level, message);
			}
			catch (Exception e)
			{
				// The one line that must not route through the injected logger. The original
				// message rides along so a broken logger does not eat the SDK's diagnostics.
				Debug.LogError(
					$"[cap-haptics] Injected logger threw {e.GetType().Name}: {e.Message} — original line: {message}");
			}
		}
	}
}
