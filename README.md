# Cap Haptics (`dev.wharang.caphaptics`)

Semantic haptics for Android from Unity. Ask for a **meaning** — `Success`, `ImpactHeavy`,
`Selection` — and the native SDK renders it through the best vibration API the device
actually supports, degrading gracefully on weaker hardware. Safe to call anywhere: in the
Editor and on non-Android platforms every call is a log-only no-op, and nothing ever throws.

## Install and make it buzz in 5 minutes

1. **Get the package.** Copy this folder into your project's `Packages/` (embedded), or
   reference it from `Packages/manifest.json`:
   ```json
   "dev.wharang.caphaptics": "file:../relative/path/to/dev.wharang.caphaptics"
   ```
   The two native AARs ship inside the package — nothing else to install.
2. **Set Android Minimum API Level to 26** (Player Settings → Other Settings). The `VIBRATE`
   permission merges in automatically from the AAR's manifest.
3. **Declare kotlin-stdlib.** The AARs are Kotlin, and Unity's flatDir packaging drops
   transitive dependencies, so enable *Custom Main Gradle Template* (Player Settings →
   Publishing Settings) and add one line under `dependencies`:
   ```gradle
   implementation 'org.jetbrains.kotlin:kotlin-stdlib:2.2.10'
   ```
4. **Initialize once, then play:**
   ```csharp
   using Cap.Haptics;

   Haptics.Initialize();                                  // once, at startup
   Haptics.Play(HapticPattern.Success);                   // meaning in, buzz out
   Haptics.Play(HapticPattern.ImpactLight, intensity: 0.6f);
   ```
   Every call returns a `HapticResult` instead of throwing. `Haptics` is a plain static
   class — no MonoBehaviour, no scene object, call it from anywhere (ECS systems included).

## The debug panel

```csharp
HapticsDiagnosticsOverlay.Attach();   // after Initialize()
```

Three tabs, no scene wiring: **Caps** shows what the device reported (per-effect and
per-primitive support, chosen tier, whether system haptics are switched off); **Patterns**
is a grid of every pattern plus a tier override, so you can feel the fallback renderings on
a device that would never choose them; **Playground** builds pulse-train waveforms from
sliders and sends them through `Haptics.PlayWaveform`.

Or import the *Haptics Demo* sample from the Package Manager window, which does both calls
for you.

## What "no vibrator" and "suppressed" mean

`HapticResult.Ok` means the platform accepted the effect — not that the user felt it: system
settings, OEM intensity sliders and battery saver can silence output invisibly. The one
honest signal is `HapticResult.Suppressed` from the `LongPress` pattern's system channel,
which reports the user's actual haptics preference. The Caps tab warns when it can tell.

## Versioning

The C# layer and the packaged AARs share a versioned ABI. On any mismatch — stale AAR, stale
C#, drifted enum — `Initialize()` fails loudly at startup with a message saying exactly what
disagrees, instead of playing the wrong thing later. If you rebuild the native side, ship
both AARs together (`gradlew installUnityPlugin` does).

The native SDK lives in the `cap-haptics-android` repository;
