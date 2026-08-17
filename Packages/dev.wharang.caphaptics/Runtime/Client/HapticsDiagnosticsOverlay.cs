using System;
using System.Text;
using CapHaptics.PatternTypes;
using UnityEngine;

namespace CapHaptics.Client
{
	/// <summary>
	/// The SDK's debug panel, three tabs deep: <b>Caps</b> mirrors the native harness's
	/// diagnostics screen; <b>Patterns</b> is the U3 grid — one button per
	/// <see cref="HapticPattern"/>, generated from the enum, plus the tier override that
	/// makes fallbacks feelable on hardware that would never choose them; <b>Playground</b>
	/// is the U4 waveform designer — sliders in, <see cref="Haptics.PlayWaveform"/> out.
	///
	/// Deliberately IMGUI — no canvas, no prefab, no scene wiring, works in any project the
	/// SDK lands in, and a debug panel has no business being pretty. It does respect
	/// <see cref="Screen.safeArea"/>, though: a debug panel under a camera cutout is a
	/// debug panel you cannot press.
	///
	/// Attach with <see cref="Attach"/> after <see cref="Haptics.Initialize"/>, or add the
	/// component to any GameObject by hand.
	/// </summary>
	public sealed class HapticsDiagnosticsOverlay : MonoBehaviour
	{
		private static readonly string[] Tabs = { "Caps", "Patterns", "Playground" };

		private static readonly string[] TierChoices = { "Auto", "T1", "T2", "T3" };

		private static readonly HapticPattern[] Patterns =
			(HapticPattern[])Enum.GetValues(typeof(HapticPattern));

		private string _report = "";
		private Vector2 _scroll;
		private int _tab;
		private int _tierChoice;
		private float _intensity = 1f;
		private string _lastAction = "—";

		private float _pulseCount = 3f;
		private float _pulseMs = 60f;
		private float _gapMs = 90f;
		private float _amplitude = 200f;
		private bool _repeat;

		public static HapticsDiagnosticsOverlay Attach()
		{
			var existing = FindAnyObjectByType<HapticsDiagnosticsOverlay>();
			if (existing != null)
				return existing;

			var host = new GameObject("[cap-haptics diagnostics]");
			DontDestroyOnLoad(host);
			return host.AddComponent<HapticsDiagnosticsOverlay>();
		}

		private void Start()
		{
			_report = BuildReport();
		}

		private void OnGUI()
		{
			var scale = Mathf.Max(1f, Screen.dpi / 160f);
			GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

			var safe = Screen.safeArea;
			var area = new Rect(
				safe.x / scale + 8,
				(Screen.height - safe.yMax) / scale + 8,
				safe.width / scale - 16,
				safe.height / scale - 16);

			GUILayout.BeginArea(area);
			_tab = GUILayout.Toolbar(_tab, Tabs, GUILayout.Height(28));

			_scroll = GUILayout.BeginScrollView(_scroll, false, false,
				GUIStyle.none, GUI.skin.verticalScrollbar);

			switch (_tab)
			{
				case 0:
					GUILayout.Label(_report);
					break;
				case 1:
					DrawPatternGrid();
					break;
				default:
					DrawPlayground();
					break;
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void DrawPatternGrid()
		{
			GUILayout.Label($"active tier: {Haptics.ActiveTier}   " +
				$"device tier: {Haptics.Capabilities?.DeviceTier.ToString() ?? "?"}");

			var tierChoice = GUILayout.SelectionGrid(_tierChoice, TierChoices, 4, GUILayout.Height(30));
			if (tierChoice != _tierChoice)
			{
				_tierChoice = tierChoice;
				var forced = _tierChoice == 0 ? (HapticTier?)null : (HapticTier)_tierChoice;
				var actual = Haptics.SetForcedTier(forced);
				_lastAction = $"SetForcedTier({(forced.HasValue ? forced.Value.ToString() : "Auto")}) → {actual}";
			}

			GUILayout.Space(4);
			DrawSlider("intensity", ref _intensity, 0f, 1f, "0.00");
			GUILayout.Space(4);

			for (var i = 0; i < Patterns.Length; i += 2)
			{
				GUILayout.BeginHorizontal();
				DrawPatternButton(Patterns[i]);
				if (i + 1 < Patterns.Length)
					DrawPatternButton(Patterns[i + 1]);
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(4);
			if (GUILayout.Button("Cancel", GUILayout.Height(26)))
			{
				Haptics.Cancel();
				_lastAction = "Cancel()";
			}

			GUILayout.Label($"last: {_lastAction}");
		}

		private void DrawPlayground()
		{
			GUILayout.Label("pulse train → PlayWaveform");
			DrawSlider("pulses", ref _pulseCount, 1f, 8f, "0");
			DrawSlider("pulse ms", ref _pulseMs, 10f, 300f, "0");
			DrawSlider("gap ms", ref _gapMs, 10f, 400f, "0");
			DrawSlider("amplitude", ref _amplitude, 1f, 255f, "0");
			_repeat = GUILayout.Toggle(_repeat, " repeat (runs until Cancel)");

			var (timings, amplitudes) = BuildPulseTrain(
				Mathf.RoundToInt(_pulseCount), (long)_pulseMs, (long)_gapMs, Mathf.RoundToInt(_amplitude));
			GUILayout.Label($"timings [{string.Join(",", timings)}]\namps    [{string.Join(",", amplitudes)}]");

			GUILayout.Space(4);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Play", GUILayout.Height(30)))
			{
				var result = Haptics.PlayWaveform(timings, amplitudes, _repeat ? 0 : -1);
				_lastAction = $"PlayWaveform(…) → {result}";
			}
			if (GUILayout.Button("Cancel", GUILayout.Height(30)))
			{
				Haptics.Cancel();
				_lastAction = "Cancel()";
			}
			GUILayout.EndHorizontal();

			GUILayout.Label($"last: {_lastAction}");
		}

		/// <summary>
		/// Pulse train in the wire's off/on-alternating shape: no leading gap on the
		/// first pulse, `gapMs` before every later one. Static so the shape is
		/// unit-testable; the GUI supplies its slider values at the call site.
		/// </summary>
		internal static (long[] timings, int[] amplitudes) BuildPulseTrain(
			int pulses, long pulseMs, long gapMs, int amplitude)
		{
			var timings = new long[pulses * 2];
			var amplitudes = new int[pulses * 2];
			for (var i = 0; i < pulses; i++)
			{
				timings[i * 2] = i == 0 ? 0 : gapMs;
				timings[i * 2 + 1] = pulseMs;
				amplitudes[i * 2 + 1] = amplitude;
			}
			return (timings, amplitudes);
		}

		private void DrawPatternButton(HapticPattern pattern)
		{
			if (GUILayout.Button(pattern.ToString(), GUILayout.Height(30)))
			{
				var result = Haptics.Play(pattern, _intensity);
				_lastAction = $"Play({pattern}, {_intensity:0.00}) → {result}";
			}
		}

		private static void DrawSlider(string label, ref float value, float min, float max, string format)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"{label} {value.ToString(format)}", GUILayout.Width(96));
			value = GUILayout.HorizontalSlider(value, min, max);
			GUILayout.EndHorizontal();
		}

		private static string BuildReport()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"cap-haptics — bridge v{Haptics.BridgeVersion}, initialized={Haptics.IsInitialized}");

			var caps = Haptics.Capabilities;
			if (caps == null)
			{
				sb.AppendLine("No capability snapshot. Did Initialize() succeed?");
				return sb.ToString();
			}

			sb.AppendLine($"API level          : {caps.SdkInt}");
			sb.AppendLine($"has vibrator       : {caps.HasVibrator}");
			sb.AppendLine($"amplitude control  : {caps.HasAmplitudeControl}");
			sb.AppendLine($"actuator count     : {caps.VibratorCount}");
			sb.AppendLine($"device tier        : {caps.DeviceTier}");
			sb.AppendLine($"active tier        : {caps.ActiveTier}");
			sb.AppendLine($"view feedback      : {caps.ViewFeedbackAvailable}");
			sb.AppendLine($"system haptics     : {caps.SystemHapticsEnabled}");

			if (caps.SystemHapticsEnabled == SupportLevel.No)
				sb.AppendLine("!! System haptics are OFF — playback will be silently suppressed.");

			sb.AppendLine();
			sb.AppendLine("predefined effects (T2)");
			foreach (var effect in caps.Effects)
				sb.AppendLine($"  {effect.Name,-14} {effect.Support}");

			sb.AppendLine();
			sb.AppendLine("composition primitives (T3)");
			foreach (var primitive in caps.Primitives)
			{
				var duration = primitive.DurationMs >= 0 ? $"  {primitive.DurationMs}ms" : "";
				sb.AppendLine($"  {primitive.Name,-14} {primitive.Support}{duration}");
			}

			return sb.ToString();
		}
	}
}
