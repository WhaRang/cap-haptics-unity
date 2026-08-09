using Cap.Haptics;
using Cap.Haptics.Client;
using UnityEngine;

namespace _Main.Scripts
{
	/// <summary>
	/// U1 smoke test: initializes cap-haptics at startup and logs the bridge version, with
	/// no scene wiring required. In the Editor this exercises the log-only stub; on the
	/// device it proves AAR packaging, manifest merge and JNI resolution in one line of
	/// output (<c>adb logcat -s Unity:V CapHaptics:V</c>).
	///
	/// Once the haptics panel exists (U2/U3) this bootstrap becomes the single place the
	/// SDK is initialized for the project.
	/// </summary>
	public static class CapHapticsBootstrap
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Initialize()
		{
			var ok = Haptics.Initialize(verboseLogging: true);
			Debug.Log($"[cap-haptics] Bootstrap: initialized={ok}, bridgeVersion={Haptics.BridgeVersion}");

			// U2: the capability panel, mirroring the native harness's diagnostics screen.
			HapticsDiagnosticsOverlay.Attach();
		}
	}
}
