# Cap Haptics (`dev.wharang.caphaptics`)

Semantic haptics for Android and iOS from Unity. Ask for a **meaning** — `Success`,
`ImpactHeavy`, `Selection` — and the native SDK renders it through the best haptics API the
device actually supports, degrading gracefully on weaker hardware. Safe to call anywhere: in
the Editor and on other platforms every call is a log-only no-op, and nothing ever throws.

## Install and make it buzz in 5 minutes

1. **Get the package.** Copy this folder into your project's `Packages/` (embedded), or
   reference it from `Packages/manifest.json`:
   ```json
   "dev.wharang.caphaptics": "file:../relative/path/to/dev.wharang.caphaptics"
   ```
   The two native AARs ship inside the package — nothing else to install.
2. **Android:** set Minimum API Level to 26 (Player Settings → Other Settings). The `VIBRATE`
   permission merges in automatically from the AAR's manifest, and the package injects the
   kotlin-stdlib dependency into the exported Gradle project at build time — no Gradle
   templates, no manual steps. (Projects that already declare kotlin-stdlib through their
   own `mainTemplate.gradle` are detected and left untouched.)
   **iOS:** nothing to configure — the Swift plugin sources in `Plugins/iOS/` compile into
   the exported Xcode project automatically (minimum iOS version 13, Unity's floor anyway).
3. **Initialize once, then play:**
   ```csharp
   using Cap.Haptics.Client;
   using Cap.Haptics.PatternTypes;

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

## Authoring your own patterns

**Create → cap-haptics → Haptic Pattern** makes a `HapticPatternAsset`. One asset is one
rendering, chosen by its **Mode**:

- **Waveform** (default) — a segment list where each segment is a static *Buzz* (duration +
  strength) or a *Curve* (draw the strength envelope, pick how finely it samples), with an
  optional leading delay. A click, a gap and a swell mix in one list.
- **Composition** — a sequence of hardware-tuned primitives (T3).
- **Predefined Effect** — one OEM-tuned effect (T2).

Play it like the built-ins:

```csharp
Haptics.Play(myPatternAsset, intensity: 0.8f);
```

The mode always plays; on hardware below its tier the native library degrades it through
the same approximation machinery the built-in patterns use — an asset never silently
no-ops. The debug panel's tier override applies, so you can feel the degraded rendering too.

While tuning, the Inspector's **Play** button previews the asset on a USB-attached phone
straight from Edit mode, no build required — waveforms, compositions and effects alike.
(The adb channel plays primitives unscaled and effects as tuned; a running app is ground
truth for scales and degradation.)

## How the tiers map per platform

The same semantic API renders differently by hardware, probed once at init:

| Tier | Android | iOS |
|---|---|---|
| 3 | `VibrationEffect.Composition` primitives (API 30+, per-primitive support) | Core Haptics (`CHHapticEngine`) |
| 2 | `createPredefined` OEM effects (API 29+) | `UIFeedbackGenerator` (impact/notification/selection) |
| 1 | `createWaveform` (the API 26 floor) | — (no waveform API below Core Haptics) |
| 0 | no vibrator | iPad, simulator |

The debug panel's tier override works on both platforms, so every fallback rendering is
feelable on one device.

## What "no vibrator" and "suppressed" mean — "I felt nothing" FAQ

`HapticResult.Ok` means the platform accepted the effect — not that the user felt it: system
settings, OEM intensity sliders and battery saver can silence output invisibly. On Android
the one honest signal is `HapticResult.Suppressed` from the `LongPress` pattern's system
channel, which reports the user's actual haptics preference. The Caps tab warns when it can
tell.

**On iOS, check Settings → Sounds & Haptics → System Haptics.** The generator tier obeys
that switch while Core Haptics does not — so "tier 3 buzzes but tier 2 is silent" means the
setting is off, not a bug. The setting is not queryable (the Caps tab shows `UNKNOWN`
honestly), and `Ok` still comes back: iOS accepts the call and mutes the output.

## Versioning

The C# layer and the packaged native code share a versioned ABI. On any mismatch — stale
AAR or Swift sources, stale C#, drifted enum — `Initialize()` fails loudly at startup with a
message saying exactly what disagrees, instead of playing the wrong thing later. If you
rebuild the native side, ship it whole: `gradlew installUnityPlugin` copies both AARs from
the `cap-haptics-android` repo; `scripts/install-unity-plugin.sh` copies the Swift sources
from `cap-haptics-ios`.

The native SDK lives in the `cap-haptics-android` repository;