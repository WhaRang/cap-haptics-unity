using CapHaptics.Client;
using NUnit.Framework;

namespace CapHaptics.Tests
{
	/// <summary>
	/// The app-level kill switch: while off, every playback path answers
	/// <see cref="HapticResult.Disabled"/> — even before init, which is what makes this
	/// testable without touching the static facade's lifecycle — and switching back on
	/// restores the previous behavior with nothing to re-arm.
	/// </summary>
	public sealed class HapticsEnabledTests
	{
		[TearDown]
		public void RestoreEnabled() => Haptics.Enabled = true;

		[Test]
		public void DisabledGatesEveryPlaybackPath()
		{
			Haptics.Enabled = false;

			Assert.That(Haptics.Play(PatternTypes.HapticPattern.Success), Is.EqualTo(HapticResult.Disabled));
			Assert.That(Haptics.Play((PatternTypes.HapticPatternAsset?)null), Is.EqualTo(HapticResult.Disabled));
			Assert.That(Haptics.PlayWaveform(new long[] { 0, 50 }), Is.EqualTo(HapticResult.Disabled));
		}

		[Test]
		public void ReEnablingRestoresNormalAnswers()
		{
			Haptics.Enabled = false;
			Assert.That(Haptics.Play(PatternTypes.HapticPattern.Success), Is.EqualTo(HapticResult.Disabled));

			Haptics.Enabled = true;
			// The suite never initializes the facade, so "normal" here is NotInitialized —
			// the point is that Disabled no longer answers.
			Assert.That(Haptics.Play(PatternTypes.HapticPattern.Success), Is.EqualTo(HapticResult.NotInitialized));
		}

		[Test]
		public void CancelStaysCallableWhileDisabled()
		{
			Haptics.Enabled = false;
			Assert.DoesNotThrow(Haptics.Cancel);
			Assert.DoesNotThrow(() => Haptics.Enabled = false); // idempotent re-disable
		}
	}
}
