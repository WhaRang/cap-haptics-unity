using CapHaptics.Client;
using NUnit.Framework;

namespace CapHaptics.Tests
{
	/// <summary>
	/// The playground's pulse-train builder must emit the wire's off/on-alternating
	/// shape exactly — it is the one place the overlay authors waveforms itself.
	/// </summary>
	public sealed class PulseTrainTests
	{
		[Test]
		public void SinglePulseStartsImmediately()
		{
			var (timings, amplitudes) = HapticsDiagnosticsOverlay.BuildPulseTrain(
				pulses: 1, pulseMs: 60, gapMs: 100, amplitude: 200);

			Assert.That(timings, Is.EqualTo(new long[] { 0, 60 }));
			Assert.That(amplitudes, Is.EqualTo(new[] { 0, 200 }));
		}

		[Test]
		public void MultiplePulsesAlternateGapAndPulse()
		{
			var (timings, amplitudes) = HapticsDiagnosticsOverlay.BuildPulseTrain(
				pulses: 3, pulseMs: 60, gapMs: 100, amplitude: 255);

			Assert.That(timings, Is.EqualTo(new long[] { 0, 60, 100, 60, 100, 60 }));
			Assert.That(amplitudes, Is.EqualTo(new[] { 0, 255, 0, 255, 0, 255 }));
		}

		[Test]
		public void OffSegmentsAreAlwaysSilent()
		{
			var (_, amplitudes) = HapticsDiagnosticsOverlay.BuildPulseTrain(
				pulses: 8, pulseMs: 10, gapMs: 10, amplitude: 128);

			for (var i = 0; i < amplitudes.Length; i += 2)
				Assert.That(amplitudes[i], Is.Zero, $"off segment {i} must be silent");
		}
	}
}
