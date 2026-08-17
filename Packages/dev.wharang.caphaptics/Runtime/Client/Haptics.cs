using System;
using CapHaptics.Backend;
using CapHaptics.PatternTypes;
using UnityEngine;

namespace CapHaptics.Client
{
	/// <summary>
	/// L0 — the public API. A plain static service with no MonoBehaviour, scene or DI
	/// dependency, so ECS systems (or anything else) can call it directly.
	///
	/// Safe to call anywhere: in the Editor and on non-Android platforms every call is a
	/// well-behaved log-only stub, and nothing in this class ever throws. Playback methods
	/// arrive in U3; the U1 surface proves the whole toolchain — AAR packaging, manifest
	/// merge, JNI resolution — while the code is still trivial enough that failures are
	/// readable.
	/// </summary>
	public static class Haptics
	{
		/// <summary>
		/// The native ABI version this C# was written against — must match the Kotlin
		/// <c>BridgeVersion.CURRENT</c> in the packaged AAR. On mismatch, init fails
		/// loudly instead of letting a stale AAR fail mysteriously later.
		/// </summary>
		public const int ExpectedBridgeVersion = 2;

		private static IHapticBackend? _backend;

		public static bool IsInitialized { get; private set; }

		/// <summary>The bridge version the packaged AAR reported, or -1 before init.</summary>
		public static int BridgeVersion { get; private set; } = -1;

		/// <summary>
		/// What the device in front of us can actually do — probed natively at init, parsed
		/// once. Null before <see cref="Initialize"/> or when the parse failed.
		/// </summary>
		public static HapticCapabilities? Capabilities { get; private set; }

		/// <summary>
		/// What is actually playing back right now — differs from
		/// <see cref="HapticCapabilities.DeviceTier"/> while a tier is forced. Tracked here
		/// rather than fetched per read: it only changes through <see cref="SetForcedTier"/>.
		/// </summary>
		public static HapticTier ActiveTier { get; private set; } = HapticTier.None;

		/// <summary>
		/// Routes the SDK's C# log lines somewhere other than the Unity console — your own
		/// pipeline, a file, an analytics backend, or a silent sink. Pass null to restore
		/// the default <see cref="Debug"/> logger. Takes effect immediately; call before
		/// <see cref="Initialize"/> to capture init logging too. Native-side logging
		/// (logcat / os_log, tag <c>CapHaptics</c>) is a separate channel and stays put.
		/// </summary>
		public static void SetLogger(IHapticsLogger? logger) => HapticsLog.Set(logger);

		/// <summary>
		/// Idempotent; call once at startup. Returns false — after logging exactly why —
		/// rather than throwing, and every later call no-ops safely when init failed.
		/// </summary>
		/// <param name="verboseLogging">Routes native SDK logging to
		/// <c>adb logcat -s CapHaptics:V</c>; the Editor stub logs to the console.</param>
		public static bool Initialize(bool verboseLogging = false)
		{
			if (IsInitialized)
				return true;

			try
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				_backend = new AndroidHapticBackend();
#elif UNITY_IOS && !UNITY_EDITOR
				_backend = new IosHapticBackend();
#else
				_backend = new EditorHapticBackend();
#endif
				BridgeVersion = _backend.GetBridgeVersion();
				if (BridgeVersion != ExpectedBridgeVersion)
				{
					HapticsLog.Error(
						$"[cap-haptics] Bridge version mismatch: C# expects {ExpectedBridgeVersion}, " +
						$"packaged AAR reports {BridgeVersion}. Rebuild and reinstall the AARs " +
						"(gradlew installUnityPlugin) or update the C# package.");
					DisposeBackend();
					return false;
				}

				if (!_backend.Initialize(verboseLogging))
				{
					HapticsLog.Error("[cap-haptics] Native initialization failed.");
					DisposeBackend();
					return false;
				}

				var manifestProblems = EnumManifestValidator.Validate(_backend.GetEnumManifestJson());
				if (manifestProblems != null)
				{
					HapticsLog.Error(
						"[cap-haptics] Enum manifest mismatch — the C# enums and the packaged " +
						$"AAR disagree; refusing to initialize:\n{manifestProblems}");
					DisposeBackend();
					return false;
				}

				Capabilities = HapticCapabilities.FromJson(_backend.GetCapabilitiesJson());
				if (Capabilities == null)
					HapticsLog.Warning("[cap-haptics] Capabilities unavailable — diagnostics will be empty.");
				ActiveTier = Capabilities?.ActiveTier ?? HapticTier.None;

				IsInitialized = true;
				HapticsLog.Info($"[cap-haptics] Initialized, bridge version {BridgeVersion}, " +
					$"device tier {Capabilities?.DeviceTier.ToString() ?? "?"}, " +
					$"active tier {Capabilities?.ActiveTier.ToString() ?? "?"}.");
				return true;
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] Initialize failed: {e.Message}");
				DisposeBackend();
				return false;
			}
		}

		/// <summary>
		/// The main entry point: play a semantic pattern, rendered however the active tier
		/// can. Safe to call from anywhere, including before init (returns
		/// <see cref="HapticResult.NotInitialized"/>) and in the Editor (logs only).
		/// </summary>
		/// <param name="intensity">0..1, clamped natively. Reduces the authored rendering,
		/// never strengthens it — and 0 is the weakest <i>perceptible</i> setting, not
		/// silence; a caller wanting nothing should not call this.</param>
		public static HapticResult Play(HapticPattern pattern, float intensity = 1f)
		{
			if (_backend == null || !IsInitialized)
				return HapticResult.NotInitialized;
			
			return HapticResultExtensions.FromCode(_backend.PlayPattern((int)pattern, intensity));
		}

		/// <summary>
		/// Plays a designer-authored <see cref="HapticPatternAsset"/> (M3). The asset's
		/// <see cref="HapticPatternAsset.Mode"/> is authoritative — a Composition asset is a
		/// composition, always — and degradation on lower tiers happens natively, through
		/// the same per-primitive/effect approximation machinery the built-in patterns use.
		/// The forced-tier override therefore applies to assets too.
		/// </summary>
		/// <param name="intensity">0..1. Scales composition step strengths and waveform
		/// amplitudes; a predefined effect plays as tuned — the platform offers no dial.</param>
		public static HapticResult Play(HapticPatternAsset? asset, float intensity = 1f)
		{
			if (asset == null)
				return HapticResult.InvalidArgument;
			if (_backend == null || !IsInitialized)
				return HapticResult.NotInitialized;

			switch (asset.Mode)
			{
				case HapticPatternAsset.PatternMode.Composition:
				{
					var steps = asset.Composition;
					if (steps.Count == 0)
						return HapticResult.InvalidArgument;
					var ids = new int[steps.Count];
					var scales = new float[steps.Count];
					var delays = new int[steps.Count];
					var clamped = float.IsNaN(intensity) ? 1f : Mathf.Clamp01(intensity);
					for (var i = 0; i < steps.Count; i++)
					{
						ids[i] = (int)steps[i].primitive;
						scales[i] = Mathf.Clamp01(steps[i].scale * clamped);
						delays[i] = steps[i].delayMs;
					}
					return HapticResultExtensions.FromCode(_backend.PlayComposition(ids, scales, delays));
				}

				case HapticPatternAsset.PatternMode.PredefinedEffect:
					return HapticResultExtensions.FromCode(_backend.PlayEffect((int)asset.Effect));

				default:
				{
					asset.BuildWaveform(intensity, out var timings, out var amplitudes);
					if (timings.Length == 0)
						return HapticResult.InvalidArgument;
					return HapticResultExtensions.FromCode(_backend.PlayWaveform(timings, amplitudes, -1));
				}
			}
		}

		/// <summary>
		/// Debug affordance: pin playback to a lower tier to feel the fallback paths on
		/// hardware that would never choose them. Pass null to return to automatic
		/// selection; requests above the device's natural tier are clamped.
		/// </summary>
		/// <returns>The tier actually in effect afterwards.</returns>
		public static HapticTier SetForcedTier(HapticTier? tier)
		{
			if (_backend == null || !IsInitialized)
				return HapticTier.None;
			var level = _backend.SetForcedTier(tier.HasValue ? (int)tier.Value : -1);
			ActiveTier = level >= 0 && level <= 3 ? (HapticTier)level : HapticTier.None;
			return ActiveTier;
		}

		/// <summary>
		/// Plays a caller-authored envelope — the escape hatch under the semantic API. Validation happens natively;
		/// <see cref="HapticResult.InvalidArgument"/> comes back for anything the platform
		/// would have thrown on.
		/// </summary>
		/// <param name="timingsMs">Alternating off/on segment durations, starting with off.</param>
		/// <param name="amplitudes">Per-segment amplitudes 0..255; null when the rhythm alone
		/// carries the pattern (or the motor has no amplitude control).</param>
		/// <param name="repeatIndex">-1 for no repeat. <b>A repeating waveform runs until
		/// <see cref="Cancel"/>.</b></param>
		public static HapticResult PlayWaveform(long[] timingsMs, int[]? amplitudes = null, int repeatIndex = -1)
		{
			if (_backend == null || !IsInitialized)
				return HapticResult.NotInitialized;
			return HapticResultExtensions.FromCode(
				_backend.PlayWaveform(timingsMs, amplitudes ?? Array.Empty<int>(), repeatIndex));
		}

		/// <summary>Stops anything currently playing.</summary>
		public static void Cancel()
		{
			_backend?.Cancel();
		}

		private static void DisposeBackend()
		{
			try
			{
				_backend?.Dispose();
			}
			catch
			{
				// Disposing a half-constructed JNI object may fail again; there is
				// nothing more useful to do with that than swallow it.
			}
			_backend = null;
		}
	}
}
