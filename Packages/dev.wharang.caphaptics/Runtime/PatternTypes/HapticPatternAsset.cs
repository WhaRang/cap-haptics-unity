using System;
using System.Collections.Generic;
using UnityEngine;

namespace CapHaptics.PatternTypes
{
	/// <summary>
	/// A designer-authored haptic pattern (M3). The built-in <see cref="HapticPattern"/>
	/// enum stays the tuned, tier-aware default vocabulary; assets are the extension point —
	/// a new pattern is a right-click in the Project window, not a Kotlin change and an AAR
	/// rebuild.
	///
	/// One asset is one rendering, chosen by <see cref="Mode"/>:
	/// - <b>Waveform</b> (default): a segment list, where each segment is either a static
	///   buzz (duration + strength) or a drawn <see cref="AnimationCurve"/> envelope sampled
	///   into steps — plus an optional leading delay. Mix freely: a click, a gap, a swell.
	/// - <b>Composition</b>: a T3 primitive sequence.
	/// - <b>PredefinedEffect</b>: one OEM-tuned T2 effect.
	///
	/// Degradation is native: a Composition asset on a waveform-only motor plays the
	/// library's per-primitive approximation, a PredefinedEffect asset likewise — the same
	/// machinery the built-in patterns use, so an asset never silently no-ops on weaker
	/// hardware. Everything plays over the existing bridge; no ABI change.
	/// </summary>
	[CreateAssetMenu(menuName = "cap-haptics/Haptic Pattern", fileName = "NewHapticPattern")]
	public sealed class HapticPatternAsset : ScriptableObject
	{
		public enum PatternMode
		{
			Waveform = 0,
			Composition = 1,
			PredefinedEffect = 2,
		}

		public enum SegmentType
		{
			Buzz = 0,
			Curve = 1,
		}

		[Serializable]
		public struct Segment
		{
			public SegmentType type;

			[Tooltip("Silence before this segment starts.")]
			[Min(0)] public int delayMs;

			[Tooltip("How long this segment lasts.")]
			[Min(0)] public int durationMs;

			[Tooltip("Buzz only: constant motor strength. 0 = silence.")]
			[Range(0, MaxAmplitude)] public int amplitude;

			[Tooltip("Curve only: the strength envelope over this segment. x = normalized time, y = strength 0..1.")]
			public AnimationCurve envelope;

			[Tooltip("Curve only: how many points the curve is sampled at. More probes = smoother, but most motors cannot articulate much below 10 ms per probe.")]
			[Range(MinProbes, MaxProbes)] public int probes;
		}

		[Serializable]
		public struct CompositionStep
		{
			public HapticPrimitive primitive;

			[Tooltip("Strength of this primitive, 0..1.")]
			[Range(0f, 1f)] public float scale;

			[Tooltip("Pause before this primitive, on top of the previous one finishing.")]
			[Min(0)] public int delayMs;
		}

		/// <summary>Native `Waveform.MAX_STEPS` is 500; building stays under it with margin.</summary>
		public const int MaxSteps = 450;

		/// <summary>The motor strength scale (`VibrationEffect` convention): 0 = silence, 255 = full.</summary>
		public const int MaxAmplitude = 255;

		/// <summary>Below this a scaled step would round to silence; audible steps never do.</summary>
		private const int MinAudibleAmplitude = 1;

		private const int MinProbes = 2;
		private const int MaxProbes = 64;
		private const int DefaultProbes = 20;

		private const int DefaultSegmentDurationMs = 400;
		private const int DefaultAmplitude = 200;

		/// <summary>Where the default envelope peaks: a quick rise, then a long decay.</summary>
		private const float DefaultEnvelopePeakTime = 0.12f;

		[SerializeField] private PatternMode mode = PatternMode.Waveform;

		[SerializeField]
		private List<Segment> segments = new()
		{
			new Segment
			{
				type = SegmentType.Curve,
				durationMs = DefaultSegmentDurationMs,
				amplitude = DefaultAmplitude,
				envelope = DefaultEnvelope(),
				probes = DefaultProbes,
			},
		};

		[SerializeField] private List<CompositionStep> composition = new()
		{
			new CompositionStep { primitive = HapticPrimitive.Click, scale = 1f },
		};

		[SerializeField] private PredefinedEffect predefinedEffect = PredefinedEffect.Click;

		public PatternMode Mode => mode;
		public IReadOnlyList<Segment> Segments => segments;
		public IReadOnlyList<CompositionStep> Composition => composition;
		public PredefinedEffect Effect => predefinedEffect;

		/// <summary>Total duration of the waveform rendering (delays included), for display.</summary>
		public int TotalDurationMs
		{
			get
			{
				var total = 0;
				foreach (var segment in segments)
					total += segment.delayMs + segment.durationMs;
				return total;
			}
		}

		/// <summary>
		/// The wire form for <c>playWaveform</c>: parallel duration/amplitude arrays.
		/// Delays become zero-amplitude steps; curve segments are sampled at probe midpoints
		/// and consecutive equal amplitudes merge, so a flat stretch costs one step. When the
		/// requested probes would exceed <see cref="MaxSteps"/>, curve sampling is coarsened
		/// proportionally rather than truncating the pattern.
		///
		/// Intensity here is a plain multiply (clamped so an audible step never rounds to
		/// full silence) — unlike the built-in patterns, which scale perceptually inside the
		/// native library. Deliberate: an asset is caller-authored raw waveform territory,
		/// where the SDK treats the author as authoritative rather than reinterpreting them.
		/// </summary>
		public void BuildWaveform(float intensity, out long[] timingsMs, out int[] amplitudes)
		{
			var clamped = float.IsNaN(intensity) ? 1f : Mathf.Clamp01(intensity);

			// Budget check up front: coarsen curves proportionally if the ask is too big.
			var requested = 0;
			foreach (var segment in segments)
				requested += (segment.delayMs > 0 ? 1 : 0)
					+ (segment.type == SegmentType.Curve ? Mathf.Max(MinProbes, segment.probes) : 1);
			var coarsen = requested > MaxSteps ? MaxSteps / (float)requested : 1f;

			var steps = new List<Segment>(Mathf.Min(requested, MaxSteps));
			foreach (var segment in segments)
			{
				if (segment.delayMs > 0)
					Append(steps, segment.delayMs, 0);

				if (segment.durationMs <= 0)
					continue;

				if (segment.type == SegmentType.Buzz)
				{
					Append(steps, segment.durationMs, segment.amplitude);
					continue;
				}

				var envelope = segment.envelope ?? DefaultEnvelope();
				var probes = Mathf.Max(MinProbes, Mathf.FloorToInt(Mathf.Max(MinProbes, segment.probes) * coarsen));
				var probeMs = segment.durationMs / (float)probes;
				for (var i = 0; i < probes; i++)
				{
					// Midpoint sampling: a probe represents its middle, not its leading edge.
					var t = (i + 0.5f) / probes;
					var amplitude = Mathf.Clamp(
						Mathf.RoundToInt(Mathf.Clamp01(envelope.Evaluate(t)) * MaxAmplitude), 0, MaxAmplitude);
					var duration = Mathf.RoundToInt((i + 1) * probeMs) - Mathf.RoundToInt(i * probeMs);
					if (duration > 0)
						Append(steps, duration, amplitude);
				}
			}

			timingsMs = new long[steps.Count];
			amplitudes = new int[steps.Count];
			for (var i = 0; i < steps.Count; i++)
			{
				timingsMs[i] = steps[i].durationMs;
				var amplitude = steps[i].amplitude;
				amplitudes[i] = amplitude == 0
					? 0
					: Mathf.Clamp(Mathf.RoundToInt(amplitude * clamped), MinAudibleAmplitude, MaxAmplitude);
			}
		}

		/// <summary>Merges runs of equal amplitude as it appends.</summary>
		private static void Append(List<Segment> steps, int durationMs, int amplitude)
		{
			if (steps.Count > 0 && steps[^1].amplitude == amplitude)
			{
				var last = steps[^1];
				last.durationMs += durationMs;
				steps[^1] = last;
				return;
			}
			steps.Add(new Segment { durationMs = durationMs, amplitude = amplitude });
		}

		private static AnimationCurve DefaultEnvelope() => new(
			new Keyframe(0f, 0f),
			new Keyframe(DefaultEnvelopePeakTime, 1f),
			new Keyframe(1f, 0f));

		private void OnValidate()
		{
			// [Min]/[Range] guard the Inspector; this guards SetValue/serialization edits —
			// and gives freshly-added list rows a usable curve instead of a null one.
			for (var i = 0; i < segments.Count; i++)
			{
				var segment = segments[i];
				segment.delayMs = Mathf.Max(0, segment.delayMs);
				segment.durationMs = Mathf.Max(0, segment.durationMs);
				segment.amplitude = Mathf.Clamp(segment.amplitude, 0, MaxAmplitude);
				segment.probes = Mathf.Clamp(
					segment.probes < MinProbes ? DefaultProbes : segment.probes, MinProbes, MaxProbes);
				
				if (segment.envelope == null || segment.envelope.length == 0)
					segment.envelope = DefaultEnvelope();
				
				segments[i] = segment;
			}

			for (var i = 0; i < composition.Count; i++)
			{
				var step = composition[i];
				step.scale = Mathf.Clamp01(step.scale);
				step.delayMs = Mathf.Max(0, step.delayMs);
				composition[i] = step;
			}
		}
	}
}
