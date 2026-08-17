#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using CapHaptics.Client;
using HapticResult = CapHaptics.Client.HapticResult;

namespace CapHaptics.Backend
{
	/// <summary>
	/// L2 — the P/Invoke bridge to the Swift plugin in <c>Plugins/iOS/CapHaptics</c>.
	///
	/// Unlike JNI there is no runtime lookup: iOS builds are statically linked, so the
	/// <c>__Internal</c> externs resolve at Xcode link time — a missing native file is a
	/// linker error in the exported project, not a runtime surprise. The Swift side is
	/// shaped like the Kotlin bridge: C types only, no exception ever escapes, result
	/// codes instead of throws (PLAN.md §11.3).
	///
	/// The full contract is native except the enum manifest, permanently C#-generated
	/// via the <see cref="EditorHapticBackend"/>: there is no second enum declaration
	/// on iOS to drift from (PLAN.md §11.1).
	/// </summary>
	internal sealed class IosHapticBackend : IHapticBackend
	{
		[DllImport("__Internal")]
		private static extern int capHapticsGetBridgeVersion();

		// Swift's Bool is a 1-byte C _Bool; C#'s default bool marshalling is a 4-byte
		// BOOL — pin both directions to 1 byte or the return value reads register garbage.
		[DllImport("__Internal")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static extern bool capHapticsInitialize([MarshalAs(UnmanagedType.I1)] bool verbose);

		[DllImport("__Internal")]
		private static extern IntPtr capHapticsGetCapabilitiesJson();

		[DllImport("__Internal")]
		private static extern void capHapticsFreeString(IntPtr s);

		[DllImport("__Internal")]
		private static extern int capHapticsSetForcedTier(int tierLevel);

		[DllImport("__Internal")]
		private static extern int capHapticsPlayPattern(int patternId, float intensity);

		[DllImport("__Internal")]
		private static extern int capHapticsPlayComposition(int[] primitiveIds, float[] scales, int[] delaysMs, int count);

		[DllImport("__Internal")]
		private static extern int capHapticsPlayWaveform(long[] timingsMs, int[] amplitudes, int timingsCount, int amplitudesCount, int repeatIndex);

		[DllImport("__Internal")]
		private static extern int capHapticsPlayEffect(int effectId);

		[DllImport("__Internal")]
		private static extern void capHapticsCancel();

		private readonly EditorHapticBackend _stub = new EditorHapticBackend();

		public int GetBridgeVersion()
		{
			try
			{
				return capHapticsGetBridgeVersion();
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsGetBridgeVersion failed — are the Swift sources in Plugins/iOS? {e.Message}");
				return -1;
			}
		}

		public bool Initialize(bool verboseLogging)
		{
			try
			{
				return capHapticsInitialize(verboseLogging);
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsInitialize failed: {e.Message}");
				return false;
			}
		}

		public string GetCapabilitiesJson()
		{
			var ptr = IntPtr.Zero;
			try
			{
				ptr = capHapticsGetCapabilitiesJson();
				return ptr == IntPtr.Zero ? "" : Marshal.PtrToStringUTF8(ptr) ?? "";
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsGetCapabilitiesJson failed: {e.Message}");
				return "";
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					capHapticsFreeString(ptr);
			}
		}

		public string GetEnumManifestJson() => _stub.GetEnumManifestJson();

		public int PlayPattern(int patternId, float intensity)
		{
			try
			{
				return capHapticsPlayPattern(patternId, intensity);
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsPlayPattern failed: {e.Message}");
				return (int)HapticResult.PlatformError;
			}
		}

		public int SetForcedTier(int tierLevel)
		{
			try
			{
				return capHapticsSetForcedTier(tierLevel);
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsSetForcedTier failed: {e.Message}");
				return 0;
			}
		}

		public int PlayWaveform(long[] timingsMs, int[] amplitudes, int repeatIndex)
		{
			if (timingsMs == null || amplitudes == null)
				return (int)HapticResult.InvalidArgument;
			try
			{
				return capHapticsPlayWaveform(timingsMs, amplitudes, timingsMs.Length, amplitudes.Length, repeatIndex);
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsPlayWaveform failed: {e.Message}");
				return (int)HapticResult.PlatformError;
			}
		}

		public int PlayEffect(int effectId)
		{
			try
			{
				return capHapticsPlayEffect(effectId);
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsPlayEffect failed: {e.Message}");
				return (int)HapticResult.PlatformError;
			}
		}

		public int PlayComposition(int[] primitiveIds, float[] scales, int[] delaysMs)
		{
			if (primitiveIds == null || scales == null || delaysMs == null ||
				primitiveIds.Length == 0 ||
				scales.Length != primitiveIds.Length || delaysMs.Length != primitiveIds.Length)
				return (int)HapticResult.InvalidArgument;
			try
			{
				return capHapticsPlayComposition(primitiveIds, scales, delaysMs, primitiveIds.Length);
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsPlayComposition failed: {e.Message}");
				return (int)HapticResult.PlatformError;
			}
		}

		public void Cancel()
		{
			try
			{
				capHapticsCancel();
			}
			catch (Exception e)
			{
				HapticsLog.Error($"[cap-haptics] capHapticsCancel failed: {e.Message}");
			}
		}

		public void Dispose() => _stub.Dispose();
	}
}
#endif
