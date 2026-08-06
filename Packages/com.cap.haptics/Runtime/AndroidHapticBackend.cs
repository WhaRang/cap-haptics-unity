#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Android;

namespace Cap.Haptics
{
	/// <summary>
	/// L2 — the JNI bridge to <c>com.cap.haptics.unity.HapticsBridge</c>.
	///
	/// Everything crossing this boundary is primitives, Strings and primitive arrays —
	/// the Kotlin side is shaped so that <see cref="AndroidJavaObject.Call"/> can always
	/// resolve by name alone (no overloads, no default arguments; see the bridge's KDoc).
	///
	/// Nothing here throws past the constructor: the native surface returns result codes
	/// rather than raising, and this class catches whatever JNI itself produces. A haptic
	/// effect is never worth crashing over.
	/// </summary>
	internal sealed class AndroidHapticBackend : IHapticBackend
	{
		private const string BridgeClass = "com.cap.haptics.unity.HapticsBridge";

		private readonly AndroidJavaObject _bridge;

		/// <summary>May throw — <see cref="Haptics"/> constructs inside its own guard.</summary>
		public AndroidHapticBackend()
		{
			using var bridgeClass = new AndroidJavaClass(BridgeClass);
			_bridge = bridgeClass.CallStatic<AndroidJavaObject>("getInstance");
		}

		public int GetBridgeVersion()
		{
			try
			{
				return _bridge.Call<int>("getBridgeVersion");
			}
			catch (Exception e)
			{
				// Most likely a stale or missing AAR: the method itself is unreachable.
				Debug.LogError($"[cap-haptics] getBridgeVersion failed — are both AARs in Plugins/Android? {e.Message}");
				return -1;
			}
		}

		public bool Initialize(bool verboseLogging)
		{
			try
			{
				// The bridge wants the Activity because the system view-feedback channel
				// needs a View to hang haptics off. Unity's main thread is not the Android
				// UI thread; the Kotlin side marshals where the platform requires it.
				//
				// No `using`: currentActivity is owned by the Unity runtime, and disposing
				// it throws "The object is owned by Unity runtime" (found in U1 on-device).
				var activity = AndroidApplication.currentActivity;
				if (_bridge.Call<bool>("initialize", activity, verboseLogging))
					return true;

				// False has two different meanings; getLastError/isInitialized tell them
				// apart. A bridge-level throw records detail; an initialized-but-false
				// answer means the probe reported no usable vibrator.
				var detail = _bridge.Call<string>("getLastError");
				var nativeInitialized = _bridge.Call<bool>("isInitialized");
				Debug.LogError(
					"[cap-haptics] Native initialize returned false. " +
					$"nativeInitialized={nativeInitialized} (true = SDK is up but probe found no vibrator), " +
					$"lastError='{detail}'");
				return false;
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] initialize failed: {e.Message}");
				return false;
			}
		}

		public string GetCapabilitiesJson()
		{
			try
			{
				return _bridge.Call<string>("getCapabilitiesJson");
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] getCapabilitiesJson failed: {e.Message}");
				return "";
			}
		}

		public string GetEnumManifestJson()
		{
			try
			{
				return _bridge.Call<string>("getEnumManifestJson");
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] getEnumManifestJson failed: {e.Message}");
				return "";
			}
		}

		public int PlayPattern(int patternId, float intensity)
		{
			try
			{
				return _bridge.Call<int>("playPattern", patternId, intensity);
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] playPattern failed: {e.Message}");
				return (int)HapticResult.PlatformError;
			}
		}

		public int SetForcedTier(int tierLevel)
		{
			try
			{
				return _bridge.Call<int>("setForcedTier", tierLevel);
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] setForcedTier failed: {e.Message}");
				return (int)HapticTier.None;
			}
		}

		public int PlayWaveform(long[] timingsMs, int[] amplitudes, int repeatIndex)
		{
			try
			{
				return _bridge.Call<int>("playWaveform", timingsMs, amplitudes, repeatIndex);
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] playWaveform failed: {e.Message}");
				return (int)HapticResult.PlatformError;
			}
		}

		public void Cancel()
		{
			try
			{
				_bridge.Call("cancel");
			}
			catch (Exception e)
			{
				Debug.LogError($"[cap-haptics] cancel failed: {e.Message}");
			}
		}

		public void Dispose()
		{
			_bridge.Dispose();
		}
	}
}
#endif
