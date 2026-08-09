using Cap.Haptics.Client;
using UnityEngine;

namespace Cap.Haptics.Samples
{
	/// <summary>
	/// The whole SDK in one file: initialize once at startup, attach the debug panel.
	/// No scene wiring needed — importing this sample into any project gives you the
	/// pattern grid, the tier override and the waveform playground on next Play.
	///
	/// In your own game you would keep the <see cref="Haptics.Initialize"/> call, drop the
	/// overlay, and call <see cref="Haptics.Play"/> from wherever meaning happens:
	/// <code>
	/// Haptics.Play(HapticPattern.Success);
	/// Haptics.Play(HapticPattern.ImpactLight, intensity: 0.6f);
	/// </code>
	/// </summary>
	public static class HapticsDemo
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Boot()
		{
			if (!Haptics.Initialize(verboseLogging: true))
			{
				// Initialize never throws; false means the log already says exactly why —
				// version mismatch, enum drift, or a native failure.
				Debug.LogWarning("[cap-haptics] Sample: initialization failed, see errors above.");
				return;
			}

			HapticsDiagnosticsOverlay.Attach();
		}
	}
}
