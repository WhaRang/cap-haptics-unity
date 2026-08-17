using CapHaptics.Client;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CapHaptics.Tests
{
	/// <summary>
	/// The C# side of the wire contract: whatever the native side emits must parse, and
	/// whatever is not JSON must come back as an honest null, never an exception — the
	/// M2 test for the one layer Phase 1 left uncovered.
	/// </summary>
	public sealed class HapticCapabilitiesTests
	{
		private const string FullBlob =
			"{\"bridgeVersion\":2,\"initialized\":true,\"sdkInt\":36,\"hasVibrator\":true," +
			"\"hasAmplitudeControl\":true,\"vibratorCount\":1,\"deviceTier\":3,\"activeTier\":2," +
			"\"viewFeedbackAvailable\":true,\"systemHapticsEnabled\":\"YES\"," +
			"\"effects\":[{\"name\":\"TICK\",\"id\":0,\"support\":\"YES\"}," +
			"{\"name\":\"CLICK\",\"id\":1,\"support\":\"UNKNOWN\"}]," +
			"\"primitives\":[{\"name\":\"CLICK\",\"id\":0,\"support\":\"YES\",\"durationMs\":20}," +
			"{\"name\":\"THUD\",\"id\":6,\"support\":\"NO\",\"durationMs\":-1}]}";

		[Test]
		public void WellFormedBlobParsesCompletely()
		{
			var caps = HapticCapabilities.FromJson(FullBlob);

			Assert.That(caps, Is.Not.Null);
			Assert.That(caps!.BridgeVersion, Is.EqualTo(2));
			Assert.That(caps.Initialized, Is.True);
			Assert.That(caps.SdkInt, Is.EqualTo(36));
			Assert.That(caps.HasVibrator, Is.True);
			Assert.That(caps.HasAmplitudeControl, Is.True);
			Assert.That(caps.VibratorCount, Is.EqualTo(1));
			Assert.That(caps.DeviceTier, Is.EqualTo(HapticTier.Composed));
			Assert.That(caps.ActiveTier, Is.EqualTo(HapticTier.Predefined));
			Assert.That(caps.ViewFeedbackAvailable, Is.True);
			Assert.That(caps.SystemHapticsEnabled, Is.EqualTo(SupportLevel.Yes));

			Assert.That(caps.Effects, Has.Length.EqualTo(2));
			Assert.That(caps.Effects[0].Name, Is.EqualTo("TICK"));
			Assert.That(caps.Effects[1].Support, Is.EqualTo(SupportLevel.Unknown));

			Assert.That(caps.Primitives, Has.Length.EqualTo(2));
			Assert.That(caps.Primitives[0].DurationMs, Is.EqualTo(20));
			Assert.That(caps.Primitives[1].Support, Is.EqualTo(SupportLevel.No));
			Assert.That(caps.Primitives[1].DurationMs, Is.EqualTo(-1));
		}

		[Test]
		public void EditorAndIosStubShapesParse()
		{
			// The Editor stub's exact emission — the iOS not-initialized blob is a subset.
			var backend = new Backend.EditorHapticBackend();
			var caps = HapticCapabilities.FromJson(backend.GetCapabilitiesJson());

			Assert.That(caps, Is.Not.Null);
			Assert.That(caps!.DeviceTier, Is.EqualTo(HapticTier.None));
			Assert.That(caps.SystemHapticsEnabled, Is.EqualTo(SupportLevel.Unknown));
			Assert.That(caps.Effects, Is.Empty);
		}

		[Test]
		public void EmptyAndNullComeBackNull()
		{
			Assert.That(HapticCapabilities.FromJson(""), Is.Null);
			Assert.That(HapticCapabilities.FromJson(null!), Is.Null);
		}

		[Test]
		public void GarbageComesBackNullNotThrown()
		{
			// The logged parse error is the expected behavior here, not a test failure —
			// absorb it rather than pattern-matching the platform's message wording.
			LogAssert.ignoreFailingMessages = true;
			try
			{
				Assert.That(HapticCapabilities.FromJson("this is not json {"), Is.Null);
			}
			finally
			{
				LogAssert.ignoreFailingMessages = false;
			}
		}

		[Test]
		public void MissingFieldsFallToHonestDefaults()
		{
			// A minimal (native-init-failed) blob: unknown support, no capabilities claimed.
			var caps = HapticCapabilities.FromJson("{\"bridgeVersion\":2,\"initialized\":false}");

			Assert.That(caps, Is.Not.Null);
			Assert.That(caps!.Initialized, Is.False);
			Assert.That(caps.HasVibrator, Is.False);
			Assert.That(caps.DeviceTier, Is.EqualTo(HapticTier.None));
			Assert.That(caps.SystemHapticsEnabled, Is.EqualTo(SupportLevel.Unknown));
			Assert.That(caps.Effects, Is.Empty);
			Assert.That(caps.Primitives, Is.Empty);
		}
	}
}
