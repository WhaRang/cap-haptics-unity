// The interface lives in UnityEditor.Android, which only exists with Android Build Support
// installed and the Android build target active — exactly the situations where the injector
// is needed. Everywhere else the class simply doesn't compile, which is correct.
#if UNITY_ANDROID
using System;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace Cap.Haptics.Editor
{
	/// <summary>
	/// M1 — zero-setup install: injects the kotlin-stdlib dependency into the Gradle project
	/// Unity exports, so consumers never edit a Gradle template by hand.
	///
	/// Why this is needed at all: the cap-haptics AARs are Kotlin, and Unity consumes AARs
	/// through a flatDir repository, which carries no transitive-dependency metadata. Without
	/// the stdlib, the SDK's enums reference <c>kotlin.enums.EnumEntries</c>, absent from the
	/// old partial stdlib androidx drags in — and init dies with a <c>NoClassDefFoundError</c>
	/// naming our class while the real missing one is three frames down. That failure arrives
	/// from inside JNI, on-device, nowhere near the actual mistake; an install step whose
	/// failure mode looks like that must not be manual.
	///
	/// Idempotent: projects that already declare kotlin-stdlib — via their own
	/// mainTemplate.gradle or another plugin's injector — are left untouched.
	/// </summary>
	public sealed class KotlinStdlibInjector : IPostGenerateGradleAndroidProject
	{
		/// <summary>
		/// The single place the version lives. Sync with the embedded Kotlin of the AGP the
		/// AARs are built with (AGP 9.3.1 → Kotlin 2.2.10) whenever they are rebuilt — the
		/// android repo's PLAN.md §10 M1 records this coupling. Gradle resolves version
		/// conflicts upward, so a consumer already on a newer stdlib wins harmlessly.
		/// </summary>
		private const string KotlinStdlibVersion = "2.2.10";

		private const string DependencyNeedle = "org.jetbrains.kotlin:kotlin-stdlib";

		private static readonly string DependencyLine =
			$"    implementation 'org.jetbrains.kotlin:kotlin-stdlib:{KotlinStdlibVersion}' // injected by dev.wharang.caphaptics";

		public int callbackOrder => 0;

		/// <param name="path">The exported <c>unityLibrary</c> module directory (Unity
		/// 2019.3+ contract). Handled defensively in case a future Unity hands the root.</param>
		public void OnPostGenerateGradleAndroidProject(string path)
		{
			try
			{
				var gradleFile = FindUnityLibraryGradle(path);
				if (gradleFile == null)
				{
					Debug.LogError(
						"[cap-haptics] Could not find unityLibrary/build.gradle under " +
						$"'{path}' — kotlin-stdlib was NOT injected. The Android build may " +
						"fail at runtime with NoClassDefFoundError; declare " +
						$"'{DependencyNeedle}:{KotlinStdlibVersion}' via a custom mainTemplate.gradle instead.");
					return;
				}

				var content = File.ReadAllText(gradleFile);
				if (content.Contains(DependencyNeedle))
				{
					Debug.Log($"[cap-haptics] kotlin-stdlib already declared in {gradleFile} — leaving it alone.");
					return;
				}

				var marker = "dependencies {";
				var index = content.IndexOf(marker, StringComparison.Ordinal);
				if (index < 0)
				{
					Debug.LogError(
						$"[cap-haptics] No dependencies block in {gradleFile} — kotlin-stdlib " +
						"was NOT injected. Declare it via a custom mainTemplate.gradle instead.");
					return;
				}

				var insertAt = index + marker.Length;
				content = content.Substring(0, insertAt)
					+ Environment.NewLine + DependencyLine
					+ content.Substring(insertAt);

				File.WriteAllText(gradleFile, content);
				Debug.Log($"[cap-haptics] Injected kotlin-stdlib {KotlinStdlibVersion} into {gradleFile}.");
			}
			catch (Exception e)
			{
				// Never fail the export from here: a readable error beats a broken build
				// pipeline, and the fallback (manual template) still exists.
				Debug.LogError($"[cap-haptics] kotlin-stdlib injection failed: {e.Message}");
			}
		}

		private static string? FindUnityLibraryGradle(string path)
		{
			// Unity 2019.3+ passes the unityLibrary module itself.
			var direct = Path.Combine(path, "build.gradle");
			if (File.Exists(direct) && Path.GetFileName(path) == "unityLibrary")
				return direct;

			// Defensive: a root-directory path (or an unexpected layout).
			var nested = Path.Combine(path, "unityLibrary", "build.gradle");
			if (File.Exists(nested))
				return nested;

			// Last resort: whatever build.gradle the path points at.
			return File.Exists(direct) ? direct : null;
		}
	}
}
#endif
