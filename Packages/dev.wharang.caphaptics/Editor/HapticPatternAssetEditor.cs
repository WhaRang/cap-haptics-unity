using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CapHaptics.PatternTypes;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CapHaptics.Editor
{
	/// <summary>
	/// Inspector for <see cref="HapticPatternAsset"/> with the M3 tuning loop: preview the
	/// asset on a USB-attached phone <b>from Edit mode</b>, via
	/// <c>adb shell cmd vibrator_manager</c> — no build, no Play mode, no app installed.
	///
	/// The asset's mode decides everything: a Waveform asset previews through the
	/// <c>waveform</c> shell effect, a Composition through <c>primitives</c>, a
	/// PredefinedEffect through <c>prebaked</c> — the same one-asset-one-rendering rule the
	/// runtime plays by. Known gaps versus the in-app path, stated rather than hidden: the
	/// shell applies no per-primitive scales and no intensity to prebaked effects, and it
	/// bypasses the SDK's native degradation — the debug panel in a running app remains the
	/// ground truth for what a lower-tier device gets.
	///
	/// adb output is quiet unless "Log adb commands" is on (errors always log). The exact
	/// shell syntax is OEM/version-sensitive, so with logging enabled the command and the
	/// device's reply appear verbatim — a dialect mismatch reads as evidence, not a dead
	/// button.
	/// </summary>
	[CustomEditor(typeof(HapticPatternAsset))]
	public sealed class HapticPatternAssetEditor : UnityEditor.Editor
	{
		private const string LogPrefsKey = "CapHaptics.Preview.LogAdb";

		// ------------------------------------------------------------------ platform ids
		//
		// The adb shell takes the platform's own constants, not our wire ids. These mirror
		// android.os.VibrationEffect (the same mapping :haptics-core's PlatformIds does at
		// runtime); duplicated here because an Edit-mode tool has no AAR to ask.

		private static int PlatformEffectId(PredefinedEffect effect) => effect switch
		{
			PredefinedEffect.Click => 0,        // EFFECT_CLICK
			PredefinedEffect.DoubleClick => 1,  // EFFECT_DOUBLE_CLICK
			PredefinedEffect.Tick => 2,         // EFFECT_TICK
			PredefinedEffect.HeavyClick => 5,   // EFFECT_HEAVY_CLICK
			_ => 0,
		};

		private static int PlatformPrimitiveId(HapticPrimitive primitive) => primitive switch
		{
			HapticPrimitive.Click => 1,     // PRIMITIVE_CLICK
			HapticPrimitive.Thud => 2,      // PRIMITIVE_THUD
			HapticPrimitive.Spin => 3,      // PRIMITIVE_SPIN
			HapticPrimitive.QuickRise => 4, // PRIMITIVE_QUICK_RISE
			HapticPrimitive.SlowRise => 5,  // PRIMITIVE_SLOW_RISE
			HapticPrimitive.QuickFall => 6, // PRIMITIVE_QUICK_FALL
			HapticPrimitive.Tick => 7,      // PRIMITIVE_TICK
			HapticPrimitive.LowTick => 8,   // PRIMITIVE_LOW_TICK
			_ => 1,
		};

		// ---------------------------------------------------------------------- fields

		private float _previewIntensity = 1f;

		private SerializedProperty _mode = null!;
		private SerializedProperty _segments = null!;
		private SerializedProperty _composition = null!;
		private SerializedProperty _predefinedEffect = null!;

		private void OnEnable()
		{
			_mode = serializedObject.FindProperty("mode");
			_segments = serializedObject.FindProperty("segments");
			_composition = serializedObject.FindProperty("composition");
			_predefinedEffect = serializedObject.FindProperty("predefinedEffect");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			var asset = (HapticPatternAsset)target;

			EditorGUILayout.PropertyField(_mode);
			EditorGUILayout.Space(4);

			var mode = (HapticPatternAsset.PatternMode)_mode.enumValueIndex;
			switch (mode)
			{
				case HapticPatternAsset.PatternMode.Waveform:
					EditorGUILayout.PropertyField(_segments);
					break;
				case HapticPatternAsset.PatternMode.Composition:
					EditorGUILayout.PropertyField(_composition);
					break;
				case HapticPatternAsset.PatternMode.PredefinedEffect:
					EditorGUILayout.PropertyField(_predefinedEffect);
					break;
			}

			serializedObject.ApplyModifiedProperties();

			// ----------------------------------------------------------------- preview
			EditorGUILayout.Space(8);
			EditorGUILayout.LabelField("Preview on attached device", EditorStyles.boldLabel);

			EditorGUILayout.LabelField(Summary(asset, mode), EditorStyles.miniLabel);

			if (mode == HapticPatternAsset.PatternMode.Waveform)
				_previewIntensity = EditorGUILayout.Slider("Intensity", _previewIntensity, 0f, 1f);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Play", GUILayout.Height(28)))
				Preview(asset, mode);
			if (GUILayout.Button("Stop", GUILayout.Height(28), GUILayout.Width(60)))
				RunAdb("shell cmd vibrator_manager cancel");
			EditorGUILayout.EndHorizontal();

			var log = EditorPrefs.GetBool(LogPrefsKey, false);
			var newLog = EditorGUILayout.ToggleLeft("Log adb commands and device replies", log);
			if (newLog != log)
				EditorPrefs.SetBool(LogPrefsKey, newLog);

			var caveat = mode switch
			{
				HapticPatternAsset.PatternMode.Composition =>
					"Preview plays primitives at full strength — the shell has no scale control. " +
					"Exact scales and per-primitive substitution need a running app.",
				HapticPatternAsset.PatternMode.PredefinedEffect =>
					"Preview plays the effect as the OEM tuned it; there is no intensity dial on this tier at all.",
				_ =>
					"Preview drives the raw vibrator directly; a device without amplitude control will flatten the envelope.",
			};
			EditorGUILayout.HelpBox(caveat, MessageType.Info);
		}

		private string Summary(HapticPatternAsset asset, HapticPatternAsset.PatternMode mode)
		{
			switch (mode)
			{
				case HapticPatternAsset.PatternMode.Composition:
					return $"{asset.Composition.Count} primitives";
				case HapticPatternAsset.PatternMode.PredefinedEffect:
					return asset.Effect.ToString();
				default:
					asset.BuildWaveform(_previewIntensity, out var timings, out _);
					return $"{asset.Segments.Count} segments → {timings.Length} steps, {asset.TotalDurationMs} ms";
			}
		}

		private void Preview(HapticPatternAsset asset, HapticPatternAsset.PatternMode mode)
		{
			switch (mode)
			{
				case HapticPatternAsset.PatternMode.Composition:
				{
					if (asset.Composition.Count == 0)
					{
						Debug.LogWarning("[cap-haptics] Nothing to preview — the composition is empty.");
						return;
					}
					// primitives ([-w delay] <primitive-id>)...
					var args = new StringBuilder("shell cmd vibrator_manager synced primitives");
					foreach (var step in asset.Composition)
					{
						if (step.delayMs > 0)
							args.Append(" -w ").Append(step.delayMs);
						args.Append(' ').Append(PlatformPrimitiveId(step.primitive));
					}
					RunAdb(args.ToString());
					return;
				}

				case HapticPatternAsset.PatternMode.PredefinedEffect:
					RunAdb($"shell cmd vibrator_manager synced prebaked {PlatformEffectId(asset.Effect)}");
					return;

				default:
				{
					asset.BuildWaveform(_previewIntensity, out var timings, out var amplitudes);
					if (timings.Length == 0)
					{
						Debug.LogWarning("[cap-haptics] Nothing to preview — the waveform is empty.");
						return;
					}
					// waveform -a takes <duration amplitude> pairs.
					var args = new StringBuilder("shell cmd vibrator_manager synced waveform -a");
					for (var i = 0; i < timings.Length; i++)
						args.Append(' ').Append(timings[i]).Append(' ').Append(amplitudes[i]);
					RunAdb(args.ToString());
					return;
				}
			}
		}

		private static void RunAdb(string arguments)
		{
			var adb = FindAdb();
			if (adb == null)
			{
				Debug.LogError(
					"[cap-haptics] adb not found. Set ANDROID_HOME/ANDROID_SDK_ROOT, or install " +
					"platform-tools to the default SDK location.");
				return;
			}

			try
			{
				using var process = Process.Start(new ProcessStartInfo
				{
					FileName = adb,
					Arguments = arguments,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				});
				if (process == null)
				{
					Debug.LogError("[cap-haptics] Could not start adb.");
					return;
				}

				var stdout = process.StandardOutput.ReadToEnd().Trim();
				var stderr = process.StandardError.ReadToEnd().Trim();
				process.WaitForExit(5000);

				var reply = string.IsNullOrEmpty(stdout + stderr) ? "(no output)" : $"{stdout} {stderr}".Trim();
				if (process.ExitCode != 0)
				{
					// Errors always log — a dialect mismatch or missing device must read as
					// evidence, not as a dead button.
					Debug.LogError($"[cap-haptics] adb {arguments}\n→ exit {process.ExitCode}: {reply}");
				}
				else if (EditorPrefs.GetBool(LogPrefsKey, false))
				{
					Debug.Log($"[cap-haptics] adb {arguments}\n→ {reply}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] adb preview failed: {e.Message}");
			}
		}

		private static string? FindAdb()
		{
			var exe = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";

			foreach (var root in new[]
			{
				Environment.GetEnvironmentVariable("ANDROID_HOME"),
				Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Android", "sdk"),
			})
			{
				if (string.IsNullOrEmpty(root))
					continue;
				var candidate = Path.Combine(root, "platform-tools", exe);
				if (File.Exists(candidate))
					return candidate;
			}

			// Last resort: PATH.
			return exe;
		}
	}
}
