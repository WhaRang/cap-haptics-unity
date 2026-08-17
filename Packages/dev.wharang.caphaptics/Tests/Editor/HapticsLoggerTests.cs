using System;
using System.Collections.Generic;
using CapHaptics.Client;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CapHaptics.Tests
{
	/// <summary>
	/// The injectable log adaptor: an injected logger receives what the SDK would have
	/// sent to the console, null restores the default, and a throwing logger cannot
	/// break the no-throw guarantee.
	/// </summary>
	public sealed class HapticsLoggerTests
	{
		private sealed class CaptureLogger : IHapticsLogger
		{
			public readonly List<(HapticsLogLevel level, string message)> Lines = new();
			public void Log(HapticsLogLevel level, string message) => Lines.Add((level, message));
		}

		private sealed class ThrowingLogger : IHapticsLogger
		{
			public void Log(HapticsLogLevel level, string message) => throw new InvalidOperationException("broken logger");
		}

		[TearDown]
		public void RestoreDefaultLogger() => Haptics.SetLogger(null);

		[Test]
		public void InjectedLoggerReceivesSdkLines()
		{
			var capture = new CaptureLogger();
			Haptics.SetLogger(capture);

			// The parse-error path is a known, deterministic SDK log line.
			Assert.That(HapticCapabilities.FromJson("this is not json {"), Is.Null);

			Assert.That(capture.Lines, Has.Count.EqualTo(1));
			Assert.That(capture.Lines[0].level, Is.EqualTo(HapticsLogLevel.Error));
			Assert.That(capture.Lines[0].message, Does.StartWith("[cap-haptics]"));
			// Nothing reached the Unity console — that is the point of the adaptor.
			LogAssert.NoUnexpectedReceived();
		}

		[Test]
		public void NullRestoresTheUnityDefault()
		{
			Haptics.SetLogger(new CaptureLogger());
			Haptics.SetLogger(null);

			LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
				@"\[cap-haptics\] Could not parse capabilities JSON"));
			Assert.That(HapticCapabilities.FromJson("this is not json {"), Is.Null);
		}

		[Test]
		public void ThrowingLoggerIsCaughtAndReported()
		{
			Haptics.SetLogger(new ThrowingLogger());

			LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
				@"\[cap-haptics\] Injected logger threw InvalidOperationException.*original line"));
			// Must not throw, and must still return the honest null.
			Assert.That(HapticCapabilities.FromJson("this is not json {"), Is.Null);
		}
	}
}
