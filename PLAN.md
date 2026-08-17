# cap-haptics — Project Plan

A Unity → Kotlin haptics SDK. Unity hosts a button panel; a Kotlin Android library
does the real work, choosing the best available vibration API for the device and
degrading gracefully on older Android versions.

The point of the project is **SDK development practice**: designing a stable public
surface, a clean native boundary, capability detection, and graceful degradation —
not just "make the phone buzz".

---

## Current state — snapshot for continuation (2026-08-09)

*This section is the context handoff between machines/sessions. Update it when a milestone
closes.*

**Done:** Phase 1 in full (A0–A8 Android, U0–U5 Unity, template cleanup). Phase 2: **M1 ✅**
(zero-setup install — kotlin-stdlib auto-injected at Gradle export), **M3 ✅** (pattern
assets with adb edit-mode preview, verified on-device). **M2 deferred by decision**: git
init + CI for the android/unity/**ios** repos will happen as one batch once
`cap-haptics-ios` exists and hosts the M5 plugin. **M5 ✅ (2026-08-10)**: the full §11 plan
ran I0–I7 in two days; every phase verified on the iPhone, including the I5 checklist
(assets ×3 modes on both tiers, repeat+cancel, interruption recovery, intensity dial) and
the I6 hardening pass (boundary no-throw audit, 8k-round fuzz, 34 `swift test` green).
Package bumped to **0.10.0**; README covers iOS install (zero-step) and the "I felt
nothing" FAQ. Two iOS lessons recorded in §11.7 and the ios repo README: (1) a running
haptics-only `CHHapticEngine` owns the actuator, so it is *released* before generator
playback; (2) **generators obey the System Haptics toggle, Core Haptics does not** — tier 2
silent while tier 3 buzzes is that setting, not a bug; `systemHapticsEnabled` stays
`UNKNOWN` (not queryable). **M2 mostly ✅ (2026-08-10):** all three repos turned out to already be on GitHub
(`WhaRang/cap-haptics-*`) with clean trees — the un-versioned-workspace premise was stale,
and PLAN.md is versioned inside the unity repo, so the copy-into-android idea is dropped
(a duplicate would drift). Done: `v0.10.0` tags on all three repos; CI workflows committed
(android: `gradlew build` on temurin-21 — if AGP 9.3 demands a newer JDK the run will say
so; ios: `swift test` + device-SDK typecheck on macos-15). CI verified green on GitHub. C# editmode
tests written (2026-08-10): `Tests/Editor/` in the package (+`testables` in the project
manifest, `InternalsVisibleTo` from Runtime) — capabilities JSON (full blob, stub shape,
empty/garbage → null, missing-field defaults), EnumManifestValidator (agreement,
Kotlin-naming normalization, id/name drift, missing section, appended-entry tolerance,
multi-problem reporting), and the overlay's pulse-train builder (extracted to an internal
static for testability). **M2 ✅ closed 2026-08-17**: suite green in the Editor test
runner — 16/16 (8 EnumManifestValidator, 5 HapticCapabilities, 3 PulseTrain) via a
headless `-runTests -testPlatform EditMode` run. **M4 deferred by decision (2026-08-17)**:
shipping without the device-matrix run for now — consequence: the listing/FAQ must say
low-end Android tier selection is verified by forced-tier + JVM tests, not on hardware.
**M6 format decision made (2026-08-17): UPM delivery**, not `.unitypackage` — see §10 M6.
Package metadata refreshed for iOS (description/keywords in `package.json`).
**Open:** M6 (Asset Store). **Workspace note:** repos moved to
`~/dev/Alex/` (2026-08-10) — iCloud sync of `~/Documents` was corrupting Unity builds with
"name 2" file duplicates; never keep Unity projects in synced folders.

**Versions:** UPM package **0.10.0** · bridge ABI **v2** (shared constant — Android AAR
`BridgeVersion.CURRENT` and iOS `BridgeVersion.current` both checked against
`Haptics.ExpectedBridgeVersion` at init) · AAR modules compileSdk 36 (the Unity-AGP
ceiling, §2) · kotlin-stdlib **2.2.10** injected by `Editor/KotlinStdlibInjector.cs`
· Unity **6000.3.9f1** · test devices Samsung S26 Ultra (API 36, full T3) + iPhone (M5).

**Unity package layout** (a post-U5 reorg — supersedes the sketch in §6; renamed from
`com.cap.haptics` on 2026-08-17 — see the namespace note below):

```
Packages/dev.wharang.caphaptics/
├── Runtime/
│   ├── Backend/        IHapticBackend (the platform seam) · AndroidHapticBackend
│   │                   (JNI, compiled UNITY_ANDROID && !UNITY_EDITOR) ·
│   │                   EditorHapticBackend (log-only stub, honest answers)
│   ├── Client/         Haptics (static facade) · HapticCapabilities ·
│   │                   EnumManifestValidator · HapticResult · SupportLevel ·
│   │                   HapticsDiagnosticsOverlay (3-tab debug panel)
│   └── PatternTypes/   enum mirrors (HapticPattern/Primitive/PredefinedEffect/
│                       ViewFeedback; HapticTier still in root namespace) ·
│                       HapticPatternAsset — mode-based: one asset is ONE rendering
│                       (Waveform segments [Buzz|Curve + delay] / Composition /
│                       PredefinedEffect); native degradation below the mode's tier
├── Editor/             KotlinStdlibInjector · HapticPatternAssetEditor (adb preview,
│                       logs off by default) · SegmentDrawer
├── Plugins/Android/    haptics-core.aar + haptics-unity.aar
│                       (refreshed by `gradlew installUnityPlugin` in the android repo)
├── Plugins/iOS/        CapHaptics/ Swift sources (M5 — compiled into the Xcode project
│                       by Unity; synced from the ios repo's Sources/CapHaptics/)
├── Tests/Editor/       C# editmode tests (M2): capabilities JSON, EnumManifestValidator,
│                       overlay pulse-train builder
└── Samples~/HapticsDemo/
```

**Namespace (2026-08-17):** Asset Store UPM enrollment granted publisher namespace
**`dev.wharang`** (domain `wharang.dev` bought at GoDaddy, verified via DNS TXT record).
The UPM package was renamed `com.cap.haptics` → **`dev.wharang.caphaptics`** (displayName
"Cap Haptics") — folder `git mv`, `package.json`, manifest `testables`, lock file, injector
marker comment, root/package/ios READMEs, the android repo's `installUnityPlugin` path and
the ios repo's `install-unity-plugin.sh`. The **Kotlin/Java namespaces and the JNI bridge
class (`com.cap.haptics.unity.HapticsBridge`) deliberately keep `com.cap.haptics`** — they
are compiled into the shipped AARs, invisible to consumers, and renaming them would mean an
AAR rebuild plus ABI churn for zero user value. Ditto the iOS `os_log` subsystem string.

**0.11.0 (2026-08-17):** app-level off switch — `Haptics.Enabled` gates all playback with
the new `HapticResult.Disabled` (wire code 7, appended on all three sides — the first real
exercise of the append-only ABI rule; **AARs must be rebuilt** via `gradlew
installUnityPlugin` or Android init fails the manifest check, loudly and by design);
switching off cancels running playback; not persisted (consumer settings own that).
Also: injectable log adaptor — `Haptics.SetLogger(IHapticsLogger)`
routes the C# layer's log lines into the consumer's pipeline (a throwing logger is caught,
so the no-throw guarantee holds; native logcat/os_log untouched; +3 editmode tests) — and
an enum-doc sweep: the C# enums are documented as the canonical wire vocabulary rather
than "mirrors of Kotlin", with each platform's real rendering stated (`ViewFeedback` named
as Android-only). The **C# root namespace renamed `Cap.Haptics.*` → `CapHaptics.*`**
(matches the asmdef names and kills the bare-`Haptics`-binds-to-namespace lookup trap;
assets safe — GUID-bound, no SerializeReference; native ABI untouched). Two feature ideas
parked in the backlog: PlayerConnection live device preview, in-build runtime pattern
editor.

**Version control:** all three repos live on GitHub (`WhaRang/cap-haptics-android`,
`-unity`, `-ios`), tagged `v0.10.0`, CI green (M2). The iOS plugin ships as **Swift
sources** inside the Unity package (not a prebuilt `.a`/`.xcframework` — Unity compiles
them into the exported Xcode project), synced from the ios repo. `PLAN.md` is versioned
inside the unity repo and is the single source of project context.

**Machine notes:** §7's `JAVA_HOME`/adb paths describe the original Windows box. iOS work
needs macOS with Unity 6000.3.9f1 (+ iOS Build Support) and Xcode; none of the Android
toolchain is required there. The Inspector's adb preview drives Android devices only — it
is not an iPhone preview.

---

## 1. Goals

**Primary**
- Learn the full loop: C# API → JNI boundary → Kotlin library → Android platform API.
- Build a *semantic* haptics API (`Success`, `ImpactHeavy`, `Selection`) rather than a
  raw one (`vibrate(200ms)`), and make each semantic pattern render sensibly on every
  supported Android version.
- Practice real SDK concerns: versioning, packaging (AAR + UPM), stable ABI across the
  JNI boundary, capability probing, error handling, no-crash guarantees.
- **Phase 2 (added 2026-08-08):** turn the SDK into a distributable product — zero-setup
  install, designer-authorable patterns, iOS parity, and an Asset Store listing. See §10.

**Secondary**
- A diagnostics screen that reports exactly which haptic tier the current device landed on.
- A custom-waveform playground so patterns can be authored from Unity without rebuilding
  the AAR.

**Explicit non-goals (for v1)**
- iOS implementation (design for it, stub it out). *v1 shipped exactly so; the non-goal is
  lifted in Phase 2 — see §10 M5.*
- Audio-coupled haptics / game-engine-driven continuous haptics.
- Gamepad or controller rumble.
- Pre-API-26 support (see §2).

---

## 2. Locked decisions

| Decision | Value | Consequence |
|---|---|---|
| Unity | **6000.3.9f1** | Unity 6.3; UPM package layout, current Gradle templates |
| Unity architecture | **Arch ECS** | C# facade must be a plain static service callable from systems — no MonoBehaviour singleton dependency |
| minSdk | **26** | Three tiers, not four. `VibrationEffect` is always available; no deprecated `vibrate(long)` path |
| compileSdk | **36 for the AAR modules, 37 for `:app`** | *Revised from 37 during U1:* AGP stamps a library's compileSdk into its AAR metadata as the minimum a consumer must compile against, and Unity 6000.3 ships AGP 8.10 whose ceiling is android-36 — so `:haptics-core`/`:haptics-unity` must stay ≤36 to be consumable. `:app` keeps 37 (its androidx deps demand it; it is never consumed by Unity) |
| AGP / Gradle / JDK | **9.3.1 / 9.5.0 / JBR 25.0.2** | Verified building clean 2026-08-05 |
| Kotlin plugin | **not needed** | AGP 9 built-in Kotlin support is active — verified by `:app:compileDebugUnitTestKotlin` succeeding with no plugin declared |
| Test device | **Samsung Galaxy S26 Ultra**, newest Android | Will land on the top tier with everything supported → **every fallback path is untestable on hardware** |
| Modules | `:haptics-core`, `:haptics-unity`, `:app` | See §5.1 |
| Package | `com.cap.haptics` | Current namespace is `com.example.hapticsandroid` — rename in A0 |
| Repos | **two** (android, unity) | `PLAN.md` lives in the un-versioned root; consider copying it into the android repo |

**The two constraints that shape everything below:**

1. **minSdk 26 removes the legacy tier.** Deliberate — the deprecated `vibrate(long)` path
   was the least interesting code in the project. Version branching still matters at the
   26 → 29 → 30 → 31 boundaries.
2. **One device, top tier.** The S26 Ultra will report full primitive support, so the
   interesting code — the fallbacks — can never be reached naturally. This makes two things
   *mandatory* rather than optional, and they're scheduled early:
   - a **forced-tier override** (A2) so you can feel every tier on the one device you own;
   - **pure, injectable tier-selection logic** (A2) so simulated capability sets can be
     unit-tested on the JVM with no device at all.

**To confirm on-device before A4:** `adb shell getprop ro.build.version.sdk`. This decides
whether API 36 envelope effects are in reach as a stretch tier.

---

## 3. Architecture

Six layers, each with one job. The rule that keeps this an SDK and not a script:
**each layer only knows about the layer directly beneath it.**

```
┌─ Unity (C#) ─────────────────────────────────────────────────────┐
│ L0  Public API        Haptics.Play(HapticPattern.Success)        │
│ L1  Platform router   Android / Editor stub / iOS stub           │
│ L2  JNI bridge        AndroidJavaObject, primitives only         │
└──────────────────────────┬───────────────────────────────────────┘
                           │  JNI  (the ABI — keep it tiny and stable)
┌──────────────────────────┴───────────────────────────────────────┐
│ L3  Kotlin facade      :haptics-unity — JNI-safe, no-throw       │
├──────────────────────────────────────────────────────────────────┤
│ L4  Capability probe   what can this device actually do?         │
│ L5  Backend strategy   pick one of 3 tiers at init  :haptics-core│
│ L6  Pattern registry   semantic pattern → per-tier rendering     │
└───────────────── Android Vibrator / VibratorManager ─────────────┘
```

### 3.1 The three backend tiers (L5)

Tier selection happens **once at init**, from the capability probe — not per call.

| Tier | Gate | Mechanism | Feel |
|------|------|-----------|------|
| **T3 — Composed** | API 30+ *and* `arePrimitivesSupported` reports the primitives we need | `VibrationEffect.startComposition()` with `PRIMITIVE_CLICK / TICK / QUICK_RISE / …` | Crisp, LRA-quality, hardware-tuned |
| **T2 — Predefined** | API 29+ | `VibrationEffect.createPredefined(EFFECT_CLICK / TICK / DOUBLE_CLICK / HEAVY_CLICK)` | Decent, OEM-tuned constants |
| **T1 — Waveform** | API 26+ (the floor) | `VibrationEffect.createWaveform(timings, amplitudes, -1)`; amplitude only if `hasAmplitudeControl()` | Buzzy but shaped |
| ~~T0 — Legacy~~ | *unreachable at minSdk 26* | — | — |

A device can be API 31 but have **no** primitive support (cheap ERM motor) — it must land
on T2 or T1. This is exactly why the gate is *capability*, not *version alone*. Version is
necessary but not sufficient.

There is also a **View-feedback channel** (`View.performHapticFeedback`), a parallel channel
rather than a tier. It obeys the user's haptic settings, is OEM-tuned per gesture, and —
uniquely — **reports when it was suppressed**, which the `Vibrator` path cannot.

*A6 narrowed its use.* It ended up routing only `LongPress`, not `Selection` or `Confirm`:
it has no intensity control and no tier story, so using it means surrendering the tuned
compositions for patterns a game invents its own meaning for. It earns its place only where
the user's expectation of the gesture outweighs ours. It is therefore a separate
`viewFeedbackFor(pattern)` lookup consulted before the tier rendering, rather than a variant
of `Rendering` — and only at full intensity, since it cannot be scaled.

### 3.2 Degradation matrix (L6) — the core design artifact

Every semantic pattern declares a rendering for **all three tiers**. Filling this table in
*is* the design work; the code is mechanical afterwards. Values below are a starting draft —
expect to tune them by feel on-device in A5.

| Pattern | T3 (composed) | T2 (predefined) | T1 (waveform) |
|---|---|---|---|
| `Selection` | `TICK` @0.4 | `EFFECT_TICK` | one-shot 10 ms @ amp 60 |
| `ImpactLight` | `CLICK` @0.4 | `EFFECT_TICK` | 15 ms @ 90 |
| `ImpactMedium` | `CLICK` @0.7 | `EFFECT_CLICK` | 25 ms @ 160 |
| `ImpactHeavy` | `CLICK` @1.0 | `EFFECT_HEAVY_CLICK` | 40 ms @ 255 |
| `Success` | `QUICK_RISE` @0.5 → `CLICK` @1.0 (+60 ms) | *(falls to T1 waveform)* | `[0,30,60,50]` amps `[0,120,0,220]` |
| `Warning` | `CLICK` @0.8 → `CLICK` @0.5 (+120 ms) | `EFFECT_DOUBLE_CLICK` | `[0,40,80,40]` @ `[0,200,0,200]` |
| `Error` | `CLICK` ×3 @0.9, 90 ms apart | *(falls to T1 waveform)* | 3 pulses 50/70 ms @255 |
| `RampUp` | `SLOW_RISE` @1.0 | *(falls to T1 waveform)* | 400 ms waveform, amp 20→255 in 16 steps |
| `Heartbeat` | `THUD` @0.8 → `THUD` @0.5 | *(falls to T1 waveform)* | `[0,60,90,40]` @ `[0,255,0,140]` |
| `LongPress` | `CLICK` @0.6 → View-feedback in A6 | `EFFECT_HEAVY_CLICK` | 45 ms @ 220 |

**T2 cannot sequence** *(found in A5)*. The first draft of this table had `EFFECT_CLICK ×2` and
similar in the T2 column, but the predefined API plays one effect per `vibrate` call — two
beats would mean scheduling a second call from a Handler, and system load between them
smears the rhythm. So multi-beat patterns render as a single waveform even at T2, and only
single-beat patterns use the tuned effects. T2's advantage over T1 is OEM tuning of
individual impacts; it has nothing to offer a rhythm. `EFFECT_DOUBLE_CLICK` is the one
native two-beat effect, so `Warning` keeps it.

Rules baked into the table:
- **Never silently no-op.** Every pattern degrades to *something*.
- **Respect the perceptible floor** *(learned by feel in A5)*. Below roughly a third of full
  strength most motors produce nothing a hand reliably detects. A "light" pattern authored
  near that floor is not light, it is broken — lightness has to come from duration and rhythm
  as much as amplitude. Intensity scaling maps onto `[floor, 1]` through a compressive curve
  rather than multiplying linearly toward zero, so the dial fades instead of falling off a
  cliff. Consequence: **intensity 0 is the weakest perceptible setting, not silence.**
- **Amplitude is optional even on T1.** If `hasAmplitudeControl()` is false, collapse the
  amplitude array to `DEFAULT_AMPLITUDE` — the timings still carry the rhythm.
- **T3 is per-primitive, not all-or-nothing.** `THUD` may be unsupported while `CLICK` is
  fine; fall back per-primitive with a substitution map, not per-pattern.

### 3.3 The JNI boundary (L2 ↔ L3) — the real ABI

This boundary is the thing to design most carefully: hardest to change later, easiest to
get subtly wrong.

**Rules for the Kotlin facade:**
- Public methods take/return **JNI-friendly types only**: `int`, `long`, `float`, `boolean`,
  `String`, `IntArray`, `LongArray`, `FloatArray`. No Kotlin data classes, no `List<T>`,
  no nullable primitives.
- **No overloads.** `AndroidJavaObject.Call` resolves by name + runtime arg types; overloads
  cause maddening `NoSuchMethodError`s. Use distinct names: `playPattern`, `playWaveform`.
- **No default arguments** without `@JvmOverloads` — Kotlin defaults compile to a synthetic
  `$default` method JNI can't see.
- **No exceptions cross the boundary.** Every public method wraps its body in `try/catch`,
  logs, and returns an error code. A thrown Java exception from a Unity `Call` is a native
  crash risk and an awful debugging experience.
- **Singleton with an explicit accessor**, not `object` field access:
  `HapticsBridge.getInstance()` annotated `@JvmStatic`.
- **Init is explicit and idempotent**: `initialize(Activity): Boolean`.

**Surface (first draft — will shrink):**
```kotlin
class HapticsBridge {
    fun initialize(activity: Activity): Boolean
    fun getBridgeVersion(): Int              // ABI guard — added in A8, checked from C#
    fun getCapabilitiesJson(): String        // one JSON blob, parsed once in C#
    fun getActiveTier(): Int                 // 1..3
    fun setForcedTier(tier: Int): Int        // debug: -1 = auto
    fun playPattern(patternId: Int, intensityScale: Float): Int
    fun playWaveform(timingsMs: LongArray, amplitudes: IntArray, repeatIndex: Int): Int
    fun performViewFeedback(constantId: Int): Int
    fun cancel()
    fun getLastError(): String
}
```

Design notes:
- **`getCapabilitiesJson` returns JSON; everything else returns primitives.** JSON is fine
  for a once-per-session diagnostic blob; it is *not* fine on a per-call hot path (string
  marshalling + GC per vibration).
- `patternId` is an `int` matching a C# enum. Two enums must stay in sync across languages —
  a genuine SDK problem; §8 covers the options.
- `intensityScale` (0..1) is applied *inside* Kotlin so scaling semantics stay consistent
  per tier.

**Threading:** Unity's main thread is *not* the Android UI thread. `Vibrator` calls are safe
off-UI-thread, but `View.performHapticFeedback` is not — the View-feedback channel must
marshal via `activity.runOnUiThread {}`.

### 3.4 Callbacks (Kotlin → C#)

Not needed for v1 playback, but worth building once as an SDK exercise:

- `UnityPlayer.UnitySendMessage(go, method, string)` — simple, string-only, requires a
  GameObject by name. Fragile, and a poor fit for an ECS project.
- **`AndroidJavaProxy`** implementing a Kotlin `interface HapticsListener` — type-safe, no
  GameObject coupling. **Preferred.** Caveat: callbacks arrive on the *calling* thread, so
  the C# side must marshal back to Unity's main thread before touching game state.

---

## 4. Android API reference

Working list. **Verify each API level against official docs before coding that tier** —
several are easy to misremember by one release, and getting it wrong means a
`NoSuchMethodError` on exactly the devices you can't test. A1's capability dump will
confirm most of this empirically.

| API | Android | What lands |
|---|---|---|
| 26 | 8.0 | **our floor** — `VibrationEffect`, `createOneShot`, `createWaveform`, `DEFAULT_AMPLITUDE`, `hasAmplitudeControl()` |
| 29 | 10 | `VibrationEffect.createPredefined()` — `EFFECT_CLICK / TICK / DOUBLE_CLICK / HEAVY_CLICK` |
| 30 | 11 | `VibrationEffect.Composition` + primitives `CLICK/TICK/QUICK_RISE/SLOW_RISE/QUICK_FALL`; `areEffectsSupported()`, `arePrimitivesSupported()`; `HapticFeedbackConstants.CONFIRM/REJECT/GESTURE_START/GESTURE_END` |
| 31 | 12 | `VibratorManager`, `CombinedVibration`, multi-actuator; primitives `LOW_TICK / THUD / SPIN`; `getPrimitiveDurations()` *(confirmed by lint in A1)* |
| 33 | 13 | `VibrationAttributes` (replaces the `AudioAttributes` usage hack) |
| 34 | 14 | more `HapticFeedbackConstants` (`SEGMENT_TICK`, `DRAG_START`, `TOGGLE_ON/OFF`) |
| 36 | 16 | envelope / vendor effects — **stretch goal only** |

Notable gotcha: on **API 29** you can *create* predefined effects but there's no query API
(`areEffectsSupported` arrives at 30). So T2 on API 29 is "fire and hope" — the platform
substitutes a generic fallback internally. Encode that as `UNKNOWN` in the capability model
rather than pretending to know.

**Permission:** `android.permission.VIBRATE` — normal (install-time), no runtime request.
Declare it in **`:haptics-core`'s own manifest** so manifest-merger pulls it into both `:app`
and the Unity build automatically. A small thing that makes the SDK feel professional:
consumers shouldn't have to know.

**System settings can silence you.** Touch-feedback off, vibration-intensity sliders (Samsung
has several), DND, and battery saver all suppress output. `hasVibrator()` returning true does
not mean the user will feel anything. The diagnostics screen must say so, or you will spend an
evening debugging a working SDK.

---

## 5. Android implementation plan

**This is the current focus.** Unity comes after A8.

### 5.1 Gradle module layout

```
cap-haptics-android/
├── settings.gradle.kts             include(":haptics-core", ":haptics-unity", ":app")
├── gradle/libs.versions.toml       + android-library plugin alias
├── haptics-core/                   com.android.library  ·  com.cap.haptics.core
├── haptics-unity/                  com.android.library  ·  com.cap.haptics.unity
└── app/                            com.android.application · com.cap.haptics.demo
```

**Why three modules.** `:haptics-core` never mentions Unity, so it stays a real Android SDK
that any app could consume — and all the interesting logic lives there. `:haptics-unity` is a
pure adapter: JNI-safe types, JSON serialization, no-throw wrappers, nothing else. `:app` is a
native harness that lets you iterate on how patterns *feel* without Unity in the loop, which is
the single biggest speed win in this project — the Unity build cycle is slow and you'll be
tuning waveforms dozens of times.

**AAR shipping.** AARs don't bundle transitive dependencies, so **both** `haptics-core.aar` and
`haptics-unity.aar` get copied into Unity's `Assets/Plugins/Android/`. Ship two AARs rather
than reaching for a fat-AAR plugin — simpler and less fragile.

### 5.2 `:haptics-core` package structure

Phase column = when the file first appears.

```
com.cap.haptics.core/
├── Haptics.kt                    public entry point for native consumers        A2
│                                 (moved up from A7: A2 needs a public surface
│                                  for the harness rather than leaking internals)
├── HapticsConfig.kt              init options: debug logging, forced tier       A2
│
├── model/
│   ├── HapticPattern.kt          semantic enum + stable int id                  A5
│   ├── HapticTier.kt             T1 / T2 / T3                                   A1
│   ├── SupportLevel.kt           YES / NO / UNKNOWN tri-state                   A1
│   ├── HapticCapabilities.kt     plain data class — sdkInt, hasVibrator,        A1
│   │                             hasAmplitudeControl, effects, primitives,
│   │                             actuatorCount
│   ├── PredefinedEffect.kt       T2 effect enum (+ minApi)                      A1
│   ├── HapticPrimitive.kt        T3 primitive enum (+ minApi, CORE set)         A1
│   ├── TierSelector.kt           pure fn: capabilities → tier                   A1
│   ├── CompositionStep.kt        primitive + scale + delay (+ validation)       A4
│   ├── ViewFeedback.kt           HapticFeedbackConstants enum (+ minApi)        A6
│   ├── HapticResult.kt           int result codes + names                       A2
│   └── Waveform.kt               timings/amplitudes value type + validation     A2
│
├── capability/
│   ├── VibratorProvider.kt       VibratorManager (31+) vs Vibrator (<31)        A1
│   ├── PlatformIds.kt            model enums → platform constants (internal)    A1
│   └── CapabilityProbe.kt        platform → HapticCapabilities                  A1
│
├── backend/
│   ├── HapticBackend.kt          interface: play(Rendering), playWaveform,      A2
│   │                             cancel
│   ├── WaveformBackend.kt        T1                                             A2
│   ├── PredefinedBackend.kt      T2                                             A3
│   ├── ComposedBackend.kt        T3                                             A4
│   ├── NoOpBackend.kt            no vibrator / disabled                         A2
│   └── BackendFactory.kt         pure fn: capabilities + forcedTier → backend   A2
│
├── pattern/
│   ├── EffectApproximation.kt    T2 effect → T1 waveform (first matrix slice)   A3
│   ├── CompositionApproximation.kt  T3 composition → T1 waveform                A4
│   ├── Rendering.kt              sealed: Composed / Effect / Wave              A5
│   │                             (no ViewFeedback variant — that channel is a
│   │                              separate lookup, see A6 note below)
│   ├── PatternRegistry.kt        the §3.2 matrix, in code                       A5
│   ├── PrimitiveSubstitution.kt  unsupported primitive → nearest supported      A4
│   └── IntensityScaler.kt        apply 0..1 scale per rendering type            A5
│
├── feedback/
│   └── ViewFeedbackChannel.kt    performHapticFeedback path (needs a View)      A6
│
└── util/
    ├── HLog.kt                   tagged, gated logging                          A1
    └── SystemHapticsSettings.kt  best-effort read of system vibration settings  A6
```

**The design constraint that makes this testable without devices:** `CapabilityProbe` is the
*only* class that touches the platform to read capabilities. Everything downstream —
`BackendFactory.select()`, `PatternRegistry`, `IntensityScaler`, `PrimitiveSubstitution`,
`Waveform` validation — is a pure function over a `HapticCapabilities` data class. That means
you can unit-test "what happens on an API 26 device with no amplitude control" on the JVM, with
no emulator and no phone. Given you have exactly one device that will always pick T3, this is
not a nicety — it's how the fallback logic gets verified at all.

### 5.3 `:haptics-unity` package structure

```
com.cap.haptics.unity/
├── HapticsBridge.kt        the §3.3 facade — no-throw, primitives only          A8
├── BridgeVersion.kt        ABI version constant                                 A8
├── CapabilitiesJson.kt     HapticCapabilities → JSON string                     A8
├── EnumManifest.kt         every marshalled enum + wire id, for C# to validate  A8
├── Json.kt                 tiny hand-rolled writer (keeps serialisation         A8
│                           JVM-testable; org.json is a stub off-device)
└── UnityCallback.kt        AndroidJavaProxy-facing interface + dispatcher       deferred
```

`UnityCallback` was **not built**: nothing in the current design needs Kotlin→C# calls, and
an unused callback channel is API surface to maintain for free. Revisit if async capability
probing or playback-complete events turn out to be wanted.

`consumer-rules.pro` ships inside the AAR. R8 cannot see that JNI reaches the bridge by name,
so without it a minified consumer build strips the class — failing only in release, only at
runtime.

### 5.4 `:app` — native test harness

Grows one screen per phase, mirroring what Unity will eventually show:
- **A0** — launcher activity, "hello" text (there is currently no activity at all).
- **A1** — capabilities dump: SDK int, vibrator present, amplitude control, per-effect and
  per-primitive support, chosen tier.
- **A2** — raw controls: duration/amplitude sliders, one-shot and waveform buttons, tier
  override spinner.
- **A5** — the full pattern grid + a tier switcher, so you can feel `Success` on T3, then
  force T2, then T1, back to back. This is where the degradation matrix gets tuned.

### 5.5 Phases

**Status: A0–A8 all complete (2026-08-06).** The table stays as the historical record.

| # | Phase | Creates | Done when |
|---|---|---|---|
| **A0** | Module skeleton & rename | 3 modules wired in `settings.gradle.kts`; namespaces → `com.cap.haptics.*`; `rootProject.name` → `cap-haptics`; `android-library` alias in the version catalog; `VIBRATE` in core's manifest; `MainActivity` + launcher in `:app` | `gradlew build` green; `:app` installs and launches on the S26 |
| **A1** | **Capability probe** | `VibratorProvider`, `CapabilityProbe`, `HapticCapabilities`, `HapticTier`, `SupportLevel`, `HLog`; `:app` diagnostics screen | Real capability values on screen — this converts §4's "verify at implementation time" caveats into facts |
| **A2** | Backend interface + T1 + tier selection | `HapticBackend`, `WaveformBackend`, `NoOpBackend`, `BackendFactory`, `HapticResult`, `Waveform`, `HapticsConfig` (incl. **forced-tier override**) | One-shot and waveform both buzz; forcing a tier visibly changes behaviour; JVM unit tests cover `BackendFactory.select()` across simulated capability sets |
| **A3** | T2 predefined | `PredefinedBackend` | All four predefined effects fire; forcing T2 on the S26 feels different from T1 |
| **A4** | T3 composed | `ComposedBackend`, `PrimitiveSubstitution` | Composition plays; an artificially-unsupported primitive substitutes correctly (unit-tested) |
| **A5** | **Pattern registry** | `HapticPattern`, `Rendering`, `PatternRegistry`, `IntensityScaler`; `:app` full grid + tier switcher | Every pattern fires on all three tiers via the override; matrix values tuned by feel |
| **A6** | View-feedback channel | `ViewFeedbackChannel`, `SystemHapticsSettings` | `LongPress` routes correctly; diagnostics warns when system haptics are off |
| **A7** | Public facade & hardening | `Haptics`, KDoc, no-throw audit, result codes, unit-test pass | Fuzzing the public API with garbage input (negative timings, absurd arrays, null-ish state, calls before init) never crashes |
| **A8** | Unity bridge + packaging | `:haptics-unity` module, `HapticsBridge`, `BridgeVersion`, `CapabilitiesJson`; Gradle task copying both AARs into the Unity project | Both AARs land in `Assets/Plugins/Android/` from one Gradle command |

A1 is deliberately first: it's cheap, it's the highest-information step in the project, and its
output determines the concrete values in A3–A5.

A5 is where the project stops being a tech demo and becomes an SDK.

---

## 6. Unity implementation plan

**Status: U0–U5 all complete, template cleanup included (2026-08-06).** What Phase 2 builds
on top of this is in §10.

| # | Phase | Deliverable |
|---|---|---|
| **U0** | UPM package skeleton at `Packages/com.cap.haptics/`, AARs in place | — |
| **U1** | **Hello bridge** — call `getBridgeVersion()`, log it | Proves toolchain, manifest merge, AAR packaging and JNI resolution at once, while the code is trivial enough that failures are readable |
| **U2** | Capability panel — parse `getCapabilitiesJson()` | — |
| **U3** | Pattern grid auto-generated from the C# enum + tier override | — |
| **U4** | Playground — sliders → `playWaveform` | — |
| **U5** | Editor stub, `Samples~/`, README, semver | — |

**Package layout:**
```
Packages/com.cap.haptics/
├── package.json
├── Runtime/
│   ├── Haptics.cs                  L0 — plain static service, ECS-friendly
│   ├── HapticPattern.cs            the enum — mirrored to Kotlin
│   ├── IHapticBackend.cs           L1
│   ├── AndroidHapticBackend.cs     L2 JNI
│   ├── EditorHapticBackend.cs      L1 stub — logs instead of vibrating
│   └── HapticCapabilities.cs       C# mirror of the JSON blob
├── Plugins/Android/
│   ├── haptics-core.aar
│   └── haptics-unity.aar
└── Samples~/HapticsDemo/
```

**Arch ECS note:** keep `Haptics` a plain static service with no `MonoBehaviour` or scene
dependency, so systems can call it directly. The JNI objects are cached statics initialised
once; nothing per-entity.

**Editor stub matters.** Every call must be safe and log-only in the Editor, so pressing Play
doesn't throw. SDKs that only work on-device are painful SDKs.

---

## 7. Build & integration workflow

**CLI builds need `JAVA_HOME`** — it isn't set system-wide on this machine (Android Studio
uses its own bundled JBR, so the IDE is unaffected):
```powershell
$env:JAVA_HOME = 'C:\Program Files\Android\Android Studio\jbr'
.\gradlew.bat build
```

Two Unity integration modes, both worth setting up:

- **A — AAR drop (main loop).** `gradlew :haptics-unity:assembleRelease` → Gradle task copies
  both AARs into the Unity package → Unity Build & Run. *Automate the copy in A8; doing it by
  hand a hundred times is how stale-AAR bugs happen.*
- **B — Gradle export (debugging).** Unity → Build → *Export Project* → open in Android Studio →
  real Kotlin breakpoints and the profiler. Reach for this the first time something misbehaves
  inside Kotlin.

**kotlin-stdlib and Gradle templates:** Unity consumes the AARs via `flatDir`, which strips
transitive-dependency metadata, so **kotlin-stdlib must be declared explicitly** — without it
the Kotlin-2.x-compiled enums reference `kotlin.enums.EnumEntries`, absent from the old partial
stdlib androidx drags in, and `Haptics` fails class-load with a `NoClassDefFoundError` that
names our class while the real missing one is three frames down (found in U1). *Since M1 the
package injects the dependency itself* via `IPostGenerateGradleAndroidProject`
(`Editor/KotlinStdlibInjector.cs`) — no template needed; a custom `mainTemplate.gradle` is only
for exotic setups, and the injector detects and defers to an existing declaration. The version
constant lives in the injector; keep it in sync with AGP's embedded Kotlin when upgrading AGP.

**Debugging:** `adb logcat -s CapHaptics:V Unity:V`. Everything the Kotlin side does gets a
tagged log line under a debug-gated verbose flag.

---

## 8. SDK-craft topics to hit deliberately

The transferable lessons — worth naming so they don't get skipped as "polish".

- **Enum sync across the language boundary.** *Decided in A8: option (c).* The bridge exposes
  `getEnumManifestJson()` — every marshalled enum with its wire id — and the C# side validates
  its own enums against it at init. Codegen (option b) is tidier on paper but means parsing
  Kotlin source in a build script, which breaks the first time someone reformats an enum.
  Runtime validation catches identical drift with none of that machinery, and catches it *on
  the device against the AAR actually installed*, which is where a mismatch really bites.
- **Wire ids are append-only.** All six marshalled enums carry an explicit `id`/`level`/`code`
  rather than relying on `ordinal`, so reordering an enum cannot silently change the ABI.
- **Versioning the ABI.** `getBridgeVersion()` from U1 onward. When C# expects v3 and the AAR is
  v2, fail loudly at init instead of mysteriously later.
- **Capability probing over version sniffing.** `Build.VERSION.SDK_INT` is a *precondition*,
  never the decision.
- **Pure logic, testable without hardware.** §5.2's constraint. The reason the fallback tiers
  are verifiable at all.
- **Let the test suite guard the layering.** `android.util.Log` is a throwing stub under JVM
  unit tests, so any `android.*` creeping into `model/` fails the build immediately. A2 hit
  this for real when `Waveform` imported `HLog`. `testOptions.unitTests.isReturnDefaultValues`
  would have silenced it — deliberately *not* enabled, because the failure is the feature.
- **No-throw public surface + error codes.** Never let a Java exception unwind into Unity.
  A7 put the outermost `try/catch` at the *facade*, not just in the backends — the backends
  catch what they can predict, the facade catches what nobody did.
- **Test the wire format itself** *(A7)*. `id`/`level`/`code` uniqueness and round-tripping
  are asserted, because a duplicate integer makes `fromId` ambiguous and the ABI unfixable
  once an AAR is in someone's Unity project.
- **Validate/construct agreement.** `create()` returns null *exactly* when `validate()` has a
  complaint, fuzzed over thousands of adversarial arrays. If the two ever disagree, something
  the platform rejects reaches it anyway.
- **Editor/no-op backend.** The SDK must be safe to call anywhere.
- **README that answers "how do I install this and make it buzz in 5 minutes".**

---

## 9. Risks

| Risk | Mitigation |
|---|---|
| One device, always top tier → fallbacks never exercised | Forced-tier override (A2) + pure unit-testable selection logic (§5.2). Both scheduled early, not as polish |
| Silent no-op (system haptics disabled) mistaken for a bug | Diagnostics screen calls it out (A6); log every playback attempt |
| Stale AAR after a Kotlin change | Gradle copy task (A8); log bridge version + build timestamp at init |
| JNI `NoSuchMethodError` from overloads/defaults/obfuscation | Rules in §3.3; keep R8 off for the bridge or add explicit `-keep` rules |
| Emulator has no vibration motor | Every phase's acceptance criterion is on the physical device |
| API-level facts misremembered | A1's empirical dump first; then verify §4 against official docs before coding each tier; guard every call with a version check *and* a capability check |
| Samsung-specific quirks (vibration intensity sliders, One UI overrides) | Diagnostics surfaces what it can; accept that some Samsung settings aren't publicly queryable |
| *(Phase 2)* Store rejection over the manual kotlin-stdlib Gradle step | M1 removes the step entirely via `IPostGenerateGradleAndroidProject` injection |
| *(Phase 2)* kotlin-stdlib version drifts from AGP's embedded Kotlin on AGP upgrades | Version lives in exactly one constant (M1); §7 note says to sync it when bumping AGP |
| *(Phase 2)* T1/T2 selection never ran on hardware that genuinely chooses it | M4 device-matrix run on a cheap ERM phone and an API 26–29 device |
| *(Phase 2)* iOS haptics semantics ≠ Android's (Core Haptics is not a Vibrator) | M5 keeps the semantic API identical and maps per platform; the §3.2 matrix gains an iOS column rather than the API gaining iOS concepts |

---

## 10. Phase 2 — from project to product

Phase 1 proved the loop: C# API → JNI → Kotlin → tier selection → motor, with a versioned,
manifest-validated ABI. Phase 2 turns that into something other people can install without
reading this document. Milestones are ordered by value ÷ effort; each is shippable alone.

**The distribution goal:** a package on the Unity Asset Store (OpenUPM as a possible early
channel — see backlog). The single principle behind most of what follows: **an SDK's install
experience is part of its API.** Every manual step is a support ticket; every undocumented
platform gap is a refund.

### M1 — Zero-setup Android install *(first: it's a store requirement in disguise)*

**Status: complete 2026-08-09.** `Editor/KotlinStdlibInjector.cs`; demo project's Gradle
templates removed, README step deleted. Verified on-device: exported `unityLibrary/build.gradle`
carries the injected line, Editor.log shows the injection, and init reaches T3 with no template
in the project. (Note: the injection log prints to the Editor console at export time — it never
appears in logcat, which is where it was first looked for.)

The kotlin-stdlib line in a custom `mainTemplate.gradle` (§7) works, but it is a manual,
easy-to-miss consumer step whose failure mode is the U1 `NoClassDefFoundError` — from inside
JNI, nowhere near the actual mistake. Reviewers test "import → build → run"; this step is
the likeliest rejection.

- An editor script in the package (`Editor/` + `CapHaptics.Editor.asmdef`) implements
  `IPostGenerateGradleAndroidProject` and injects
  `implementation 'org.jetbrains.kotlin:kotlin-stdlib:<version>'` into the exported Gradle
  project's `unityLibrary/build.gradle` — no template, no checkbox, idempotent (skip if the
  dependency is already declared, so projects with their own template keep working).
- The version lives in **one constant**, commented to sync with AGP's embedded Kotlin
  (currently 2.2.10 from AGP 9.3.1) whenever the AARs are rebuilt with a newer AGP.
- Remove the manual step from the package README (step 3) and demote the §7 template note
  to "only needed for consumers on exotic setups".

**Done when:** a fresh Unity project with the package imported builds to device with zero
manual Gradle edits, and the demo project itself no longer carries `mainTemplate.gradle`.

### M2 — Repo hygiene & CI

- `git init` both repos; copy PLAN.md into the android repo (the §2 note, finally acted on);
  tag the v0.5.0 state in each.
- GitHub Actions on the android repo: `gradlew build` — lint plus the ~90 JVM unit tests
  that exist precisely because they need no device (§5.2's design constraint pays off here).
- C# editmode tests (Unity Test Framework — already in the manifest) for the one untested
  layer: capabilities JSON parsing (well-formed, empty, garbage), `EnumManifestValidator`
  (agreement, id drift, name drift, missing section, extra AAR entries tolerated), and the
  overlay's pulse-train builder.

**Done when:** CI is green on push to the android repo; C# tests pass in the Editor test
runner and are runnable in CI later without redesign.

### M3 — Pattern assets (authorable haptics)

**Status: implemented 2026-08-09** (`Runtime/PatternTypes/HapticPatternAsset.cs`,
`Editor/HapticPatternAssetEditor.cs`, `Haptics.Play(asset, intensity)` overload; backend
gained `PlayEffect`/`PlayComposition` over the existing bridge — no ABI change). Design
delta from the sketch below: per-tier hints became a T3 composition-step list and an
optional T2 predefined effect, mirroring §3.2's matrix shape; the waveform is authorable
two ways — an `AnimationCurve` envelope ("draw what you feel", the default; sampled with
equal-run merging under the native 500-step cap) or an explicit segment list for precise
rhythms. Edit-mode preview drives the raw vibrator via `adb shell cmd vibrator_manager`
and is tier-selectable (`waveform` / `prebaked` / `primitives` shell effects) — with the
shell's limits stated in the Inspector: no primitive scales, no prebaked intensity, no SDK
tier logic; the in-app debug panel stays ground truth. Done-when pending: author loop
verified by the user on-device.

Phase 1's patterns are compiled into the AAR — extending the vocabulary means Kotlin. That
was right for learning the degradation matrix; it is wrong for a product whose users are
Unity developers and their designers.

- A `HapticPatternAsset : ScriptableObject`: waveform envelope (timings + amplitudes,
  drawn/edited in the Inspector) plus optional per-tier rendering hints.
- `Haptics.Play(HapticPatternAsset, intensity)` overload, rendered through the existing
  `playWaveform` bridge method — **no ABI change, no AAR rebuild**.
- A "Preview on device" Inspector button (plays over an attached device while the Editor
  runs — the fast tuning loop the native harness gave us, handed to designers).
- The built-in enum patterns stay: they are the tuned, tier-aware defaults; assets are the
  extension point.

**Done when:** a designer can author, tweak and feel a new pattern without touching Kotlin,
C#, or a rebuild.

### M4 — Device-matrix validation

The forced-tier override simulates the *code path*, not the hardware (§ "Testing without
the hardware"). T1/T2 have never been chosen naturally.

- A Firebase Test Lab (or BrowserStack) smoke run: the native `:app` harness and the Unity
  demo on at least (a) a cheap ERM-motor device, (b) an API 26–29 device.
- Asserts: init succeeds, the *expected* tier is selected for each device's capabilities,
  full pattern sweep completes with `OK`/`SUPPRESSED` only, no crash, no ANR.

**Done when:** the matrix results are recorded here, and any tier-selection surprise found
on real low-end hardware is fixed and unit-tested.

### M5 — iOS backend

Fills the L1 slot reserved since §3. The tier model maps better than expected:

| cap-haptics tier | iOS mechanism | Gate |
|---|---|---|
| T3-equivalent | `CHHapticEngine` (Core Haptics) | `supportsHaptics` |
| T2-equivalent | `UIFeedbackGenerator` (impact/notification/selection) | iPhone 7+ |
| Floor | well-behaved no-op | everything else (incl. iPad) |

- Swift (or ObjC) plugin + a C# `IosHapticBackend` behind the existing `IHapticBackend`
  seam; same probe-once-then-select shape; same no-throw result-code surface.
- The §3.2 degradation matrix gains an iOS column — the semantic API does not change at all.
  That invariance is the whole test of whether the Phase 1 architecture was actually right.

**Implementation notes for the iOS session** *(written 2026-08-09, before starting)*:

- The contract is `Runtime/Backend/IHapticBackend`, eleven members: `GetBridgeVersion`,
  `Initialize(verbose)`, `GetCapabilitiesJson`, `GetEnumManifestJson`, `PlayPattern`,
  `SetForcedTier`, `PlayEffect`, `PlayComposition`, `PlayWaveform`, `Cancel`, `Dispose`.
  `EditorHapticBackend` is the reference for minimal honest answers;
  `AndroidHapticBackend` for the guard-everything, never-throw style.
- Branch point: `Haptics.Initialize` picks the backend under
  `#if UNITY_ANDROID && !UNITY_EDITOR` — add an `#elif UNITY_IOS && !UNITY_EDITOR` arm.
- There is no AAR on iOS, so: `GetBridgeVersion` returns `ExpectedBridgeVersion`;
  `GetEnumManifestJson` generates from the local C# enums (the Editor stub already shows
  how); the native plugin should instead carry **its own** version constant checked at
  init — same failure philosophy, different artifact.
- `GetCapabilitiesJson` must emit the same JSON shape (reference:
  `CapabilitiesJson.kt` in the android repo, and the C# parser
  `Client/HapticCapabilities.cs`): `sdkInt` carries the iOS major version, tier integers
  reuse `HapticTier` levels, `systemHapticsEnabled` stays the tri-state
  `"YES"/"NO"/"UNKNOWN"` string.
- Mapping: `PlayPattern`/`PlayComposition` → `CHHapticEngine` (transient/continuous event
  patterns); `PlayEffect` → `UIFeedbackGenerator`; `PlayWaveform` → Core Haptics
  continuous events driven by the amplitude envelope. Degradation: no Core Haptics →
  generators; neither → well-behaved no-op with capabilities that say so.
- `HapticPatternAsset` must keep working unchanged — `Play(asset)` already routes by mode
  through the backend seam, so a correct `IosHapticBackend` gets assets for free.
- Threading: `UIFeedbackGenerator`/`CHHapticEngine` calls belong on the main thread;
  Unity's main thread is not it — marshal inside the plugin, mirroring how the Kotlin
  side handles `performHapticFeedback`.

**Done when:** `Haptics.Play(Success)` feels correct on an iPhone, the capability panel
reports the iOS tier honestly, and the Editor stub is untouched.

*The full iOS implementation plan — architecture, tier mapping, degradation column,
P/Invoke ABI rules, and the I0–I7 phase schedule — is **§11**.*

### M6 — Asset Store submission

- **Demo scene**: a real canvas with buttons wired to `Haptics.Play` — intended-usage look.
  The debug overlay stays what it is: a diagnostics tool, not the shop window.
- **Docs**: a hosted page or PDF grown from the package README (install, 5-minute buzz,
  capability model, result codes, FAQ for "I felt nothing").
- **Publisher assets**: account, key images in Unity's required sizes, 3–5 screenshots
  (device photos of panel + demo + code snippet), support email.
- **Third-party notices**: kotlin-stdlib is Apache-2.0 and resolved by the consumer's
  Gradle, not redistributed in the package — stated explicitly.
- **Compatibility statement**: tested on 6000.3; decide whether to validate 6000.0 / 2022
  LTS or declare Unity 6.x-only. (M1's editor script is the main portability risk surface.)
- **Review criteria**: zero console errors *and warnings* on fresh import; honest listing —
  Editor is a log-only stub, iOS status stated plainly whichever side of M5 this lands on.
- **Format: decided 2026-08-17 — UPM delivery.** UPM publishing on the Asset Store is now
  generally available (no longer early-access), and the package is already a clean UPM
  package (asmdefs, `Samples~`, `Tests/Editor`, per-platform plugin folders) — the
  `.unitypackage` path would mean restructuring into `Assets/` and giving up the
  Samples-UI / immutable-package / one-click-update semantics for nothing. Consequences:
  - Enroll as a UPM publisher on the Publisher Portal (identity verification via Persona;
    DNS domain verification optional for individuals).
  - A publisher **namespace** must be claimed, and the package technical name must match
    it — **verify `com.cap` is claimable before tagging 1.0.0**; if not, the rename to a
    claimable namespace is a breaking change that must land before first release, never
    after.
  - Validate with the Asset Store Publishing Tools in-Editor before upload; buyers install
    via Package Manager (package stays under `Packages/`, immutable).

**Done when:** the submission is uploaded and review feedback is triaged back into this
section.

### Backlog (unscheduled)

- **PlayerConnection live device preview** (v1.1 candidate): an "audition on connected
  device" button in `HapticPatternAssetEditor`, sending the asset over Unity's
  PlayerConnection (USB-capable on iOS — the adb-preview equivalent the iPhone lacks) to a
  small runtime listener in a dev build, playing through the real SDK tiers on both
  platforms.
- **In-build runtime pattern editor**: designers author haptics inside a dev build and
  export back as `HapticPatternAsset` JSON. The market-strong big sibling of the
  PlayerConnection preview — revisit only if the preview proves insufficient.
- Global intensity multiplier (`Haptics.GlobalIntensity`, 0..1, folded into every call
  the way the mute switch gates them). The mute half shipped in 0.11.0 as
  `Haptics.Enabled` — *not* persisted, reversing the old note here: the game's settings
  system owns persistence, the SDK holding its own PlayerPrefs key would mean two
  sources of truth.
- Per-pattern cooldowns — games spam haptics; the SDK should defend the motor.
- API 36 envelope-effects tier (the §4 stretch goal).
- Real screenshots for the repo README placeholders.
- OpenUPM listing as an early, low-ceremony distribution channel before (or alongside) the
  Asset Store.

---

## 11. iOS implementation plan (M5)

**Status: I0–I7 all complete, verified on-device (2026-08-10).** The table stays as the
historical record. Delivery-format note: §11.1's source-drop decision held — no
xcframework was ever needed. Written 2026-08-09 on the macOS machine, before any iOS code
existed. The governing invariant from §10 M5: **the semantic API does not change at all** —
`Haptics.Play(HapticPattern.Success)` and `Haptics.Play(asset)` work identically at every
call site; only the backend behind `IHapticBackend` is new. Game code gains at most an
`#elif UNITY_IOS` awareness in exactly one place (`Haptics.Initialize`), which lives in the
package, not the game.

### 11.1 Locked decisions (iOS)

| Decision | Value | Consequence |
|---|---|---|
| Native language | **Swift**, with a thin `@_cdecl` C surface | Modern API access; Unity sees only C symbols |
| Interop | **P/Invoke `[DllImport("__Internal")]`** | iOS is statically linked — no dylib name, no JNI-style reflection; symbols resolve at link time, so a missing native file fails the Xcode build, not runtime |
| Plugin delivery | **Swift source files in `Plugins/iOS/`** (not a prebuilt `.xcframework`) — *revises the §"Current state" note* | Unity compiles them into the exported Xcode project; no build pipeline, no Swift-ABI/stdlib-linking issues, diffable in the Unity package. `cap-haptics-ios` still hosts the canonical sources + the SwiftUI harness app; a copy script replaces the AAR-copy Gradle task |
| Deployment floor | **iOS 13** | `CHHapticEngine` (13.0) and generator `intensity:` overloads (13.0) both in reach; Unity 6's own minimum is ≥13 anyway |
| Tier numbering | **Reuse `HapticTier` wire levels** | iOS lands on 3 (Core Haptics), 2 (generators), or 0 (no-op). **There is no iOS T1** — no arbitrary-waveform API below Core Haptics — so `Waveform=1` is simply never reported |
| Bridge versioning | Native `capHapticsGetBridgeVersion()` returns its **own** constant, checked against `ExpectedBridgeVersion` | Same failure philosophy as the AAR; guards a stale `Plugins/iOS` copy exactly like a stale AAR |
| Enum manifest | `GetEnumManifestJson` generated **from the local C# enums** (the Editor stub's generator, reused) | There is no second language runtime to drift from — the wire ids never leave C#-land except as ints into Swift's mapping tables, which I-phase tests cover instead |
| Test device | The user's iPhone (Core Haptics capable — any iPhone 8+) | Like the S26: top tier always chosen naturally → **forced-tier override is again mandatory, not optional** (I4) |

### 11.2 Tier mapping and capability probe

Probe once at init, select once — same shape as §3.1.

| Tier (wire) | iOS mechanism | Gate | Feel |
|---|---|---|---|
| **3 — Composed** | `CHHapticEngine`: transient + continuous events, per-event intensity/sharpness | `CHHapticEngine.capabilitiesForHardware().supportsHaptics` | Full parity with Android T3 — and strictly more expressive (continuous envelopes) |
| **2 — Predefined** | `UIImpactFeedbackGenerator` (light/medium/heavy/soft/rigid), `UINotificationFeedbackGenerator` (success/warning/error), `UISelectionFeedbackGenerator` | iPhone (`userInterfaceIdiom == .phone`) and **not** `supportsHaptics` | OEM-tuned single beats; **cannot sequence reliably** (same lesson as Android T2, §3.2) and no queryable support — encode as `UNKNOWN`, like Android API 29 |
| **0 — None** | well-behaved no-op | everything else: iPad, simulator, iPod touch | Capabilities say so honestly |

In practice tier 2 is nearly empty (iPhone 7/8/X without Core Haptics are iOS-13-capable but
ancient); it exists so degradation is designed, not accidental — and the forced-tier override
makes it feelable on any iPhone.

**Capabilities JSON** — same shape, same keys, parsed by the existing
`HapticCapabilities.FromJson` with zero changes (reference: `CapabilitiesJson.kt`,
`EditorHapticBackend.GetCapabilitiesJson`):
- `sdkInt` — iOS major version (e.g. 19). The diagnostics overlay just prints it.
- `hasVibrator` — true if either tier 3 or tier 2 is available.
- `hasAmplitudeControl` — true iff Core Haptics (generators have fixed tuning; the
  `intensity:` parameter on impact generators is coarse, not an amplitude channel).
- `vibratorCount` — 1 or 0. `viewFeedbackAvailable` — false (no such channel on iOS).
- `systemHapticsEnabled` — `"UNKNOWN"`: the Settings → Sounds & Haptics switch is not
  queryable. The engine's stopped-handler reason (`.systemPolicy`) upgrades this to a
  logged hint at playback time, not at probe time.
- `effects[]` — the `PredefinedEffect` wire ids with support `YES` on any haptic iPhone
  (generators exist), `UNKNOWN` on the pre-probe-API devices, `NO` otherwise.
- `primitives[]` — the `HapticPrimitive` wire ids: `YES` across the board when
  `supportsHaptics` (every primitive is synthesized from CH events — there is no
  per-primitive hardware lottery on iOS), `NO` otherwise; `durationMs` from our own
  synthesis table.

### 11.3 The P/Invoke boundary — the iOS ABI

Different mechanics from JNI, same discipline (§3.3). The C surface, one function per
`IHapticBackend` member plus the string-memory helper:

```c
int32_t capHapticsGetBridgeVersion(void);
bool    capHapticsInitialize(bool verbose);
// Returned char* is strdup'd; C# marshals then frees via capHapticsFreeString.
const char* capHapticsGetCapabilitiesJson(void);
int32_t capHapticsPlayPattern(int32_t patternId, float intensity);
int32_t capHapticsSetForcedTier(int32_t tierLevel);        // negative = auto; returns tier in effect
int32_t capHapticsPlayEffect(int32_t effectId);
int32_t capHapticsPlayComposition(const int32_t* primitiveIds, const float* scales,
                                  const int32_t* delaysMs, int32_t count);
int32_t capHapticsPlayWaveform(const int64_t* timingsMs, const int32_t* amplitudes,
                               int32_t timingsCount, int32_t amplitudesCount, int32_t repeatIndex);
void    capHapticsCancel(void);
void    capHapticsFreeString(const char* s);
```

Rules:
- **Arrays cross as pointer + explicit count.** C# passes the managed arrays directly
  (the marshaller pins for the call); Swift copies into buffers **before** returning,
  never retains the pointers.
- **No exceptions/`fatalError` may escape.** Every `@_cdecl` body catches, logs under the
  verbose flag (`os_log` subsystem `com.cap.haptics` — the `adb logcat -s CapHaptics:V`
  equivalent is Console.app / `xcrun devicectl` log streaming), and returns a
  `HapticResult` code. Same no-throw audit as A7.
- **Threading:** all `UIFeedbackGenerator` and `CHHapticEngine` work hops to the main
  queue **inside the plugin** (`DispatchQueue.main.async`), mirroring how Kotlin marshals
  `performHapticFeedback`. Playback calls return immediately (fire-and-forget, like
  Android); only init blocks (`DispatchQueue.main.sync` if needed) because its return
  value is the probe result.
- `GetEnumManifestJson` never crosses the boundary — C#-generated (§11.1).
- **`IosHapticBackend.cs`** is compiled under `UNITY_IOS && !UNITY_EDITOR`, wraps every
  extern in try/catch like `AndroidHapticBackend`, and `Haptics.Initialize` gains the
  `#elif UNITY_IOS && !UNITY_EDITOR` arm — the only C# branch-point change.

### 11.4 Degradation matrix — the iOS column (the design artifact, again)

Core Haptics events use intensity ∩ **sharpness** (≈ frequency): sharp+short ≈ Android's
crisp primitives, soft+long ≈ THUD. Starting draft; tune by feel in I3, exactly like A5.
Perceptible-floor rule from §3.2 carries over (CH intensity below ~0.3 vanishes on the
Taptic Engine too); intensity scaling maps onto `[floor, 1]` inside Swift.

| Pattern | Tier 3 (Core Haptics) | Tier 2 (generators) |
|---|---|---|
| `Selection` | transient i0.4 s0.6 | `UISelectionFeedbackGenerator.selectionChanged()` |
| `ImpactLight` | transient i0.4 s0.5 | impact(.light) |
| `ImpactMedium` | transient i0.7 s0.5 | impact(.medium) |
| `ImpactHeavy` | transient i1.0 s0.6 | impact(.heavy) |
| `Success` | continuous rise 60 ms i0.5→ transient i1.0 s0.7 | notification(.success) |
| `Warning` | transient i0.8 + transient i0.5 @+120 ms | notification(.warning) |
| `Error` | transient i0.9 ×3, 90 ms apart | notification(.error) |
| `RampUp` | continuous 400 ms, intensity curve 0.2→1.0 | impact(.soft) → impact(.rigid) @+300 ms (best effort) |
| `Heartbeat` | transient i0.8 s0.25 + transient i0.5 s0.25 @+90 ms | impact(.heavy) ×2, 90 ms apart (best effort) |
| `LongPress` | transient i0.6 s0.4 | impact(.medium) |

Notice tier 2 is *richer* than Android T2 for the notification patterns — `Success`,
`Warning`, `Error` have native OEM-tuned renderings instead of falling to a waveform. The
matrix rewards designing per-platform instead of translating.

**Primitive synthesis table (I3):** `PlayComposition` maps each `HapticPrimitive` wire id to
a CH event recipe — `CLICK`→transient s0.7; `TICK`→transient i×0.6 s1.0; `LOW_TICK`→transient
s0.2; `THUD`→transient s0.1 (or 40 ms continuous soft); `QUICK_RISE`/`SLOW_RISE`/`QUICK_FALL`→
continuous with an intensity ramp (150/500/100 ms); `SPIN`→continuous 200 ms with an
intensity+sharpness wobble. Step `delayMs` becomes relative event times in **one**
`CHHapticPattern` — a single engine play call, no scheduling smear (the Android T2 lesson).

**`PlayWaveform` on tier 3:** off/on segments → continuous events at the segment's relative
time, `amplitude/255 → intensity` (floor-mapped), fixed s≈0.5; empty amplitudes → i0.8.
`repeatIndex ≥ 0` loops via a repeating `CHHapticPatternPlayer` schedule until `Cancel`.
On tier 2: collapse each on-segment to the nearest impact style by amplitude, schedule with
`DispatchQueue.asyncAfter` — best effort, documented as such. **`HapticPatternAsset` then
works unchanged, for free** — `Play(asset)` already routes by mode through the seam.

**Engine lifecycle (the iOS-specific risk):** `CHHapticEngine` stops on backgrounding,
audio-session interruptions, and system policy. Set `resetHandler` (recreate + restart) and
`stoppedHandler` (record reason); lazily `start()` before play if stopped. Getting this wrong
is the iOS equivalent of the stale-AAR bug: works in the first minute, dies after the first
phone call.

### 11.5 `cap-haptics-ios` repo layout

```
cap-haptics-ios/
├── Sources/CapHaptics/            the canonical plugin sources (all Swift)
│   ├── CapHapticsBridge.swift     @_cdecl surface — no-throw, C types only      I1
│   ├── BridgeVersion.swift        native ABI constant                           I1
│   ├── CapabilityProbe.swift      probe → capabilities struct                   I2
│   ├── CapabilitiesJson.swift     struct → JSON string (hand-rolled, like Json.kt) I2
│   ├── TierSelector.swift         pure fn: capabilities + forcedTier → tier     I2
│   ├── backend/
│   │   ├── HapticBackend.swift    protocol: the internal seam                   I3
│   │   ├── CoreHapticsBackend.swift  tier 3 — engine lifecycle + event synthesis I3
│   │   ├── GeneratorBackend.swift    tier 2 — the three generator families      I4
│   │   └── NoOpBackend.swift                                                    I2
│   ├── pattern/
│   │   ├── PatternRegistry.swift  the §11.4 matrix                              I3
│   │   ├── PrimitiveSynthesis.swift  primitive wire id → CH event recipe        I3
│   │   └── IntensityScaler.swift  floor-mapped scaling (§3.2 rule)              I3
│   └── util/HLog.swift            os_log wrapper, verbose-gated                 I1
├── HarnessApp/                    SwiftUI test app — :app's role: pattern grid,
│                                  tier switcher, capability dump; feel-iteration
│                                  without Unity in the loop                     I2+
├── Tests/CapHapticsTests/         XCTest: TierSelector over simulated caps,
│                                  JSON shape, synthesis-table ids ↔ C# wire ids I2+
└── scripts/install-unity-plugin.sh  copies Sources/CapHaptics → Unity package
                                     Plugins/iOS/ (the gradlew installUnityPlugin twin)
```

Same testability constraint as §5.2: `CapabilityProbe` is the only file touching the
platform for probing; `TierSelector`, the registry, synthesis and scaling are pure over a
capabilities struct — XCTest-able on any Mac, no iPhone required.

**Unity package side:** `Plugins/iOS/` gains the Swift files (marked iOS-only in the
`.meta` importer settings); `Runtime/Backend/IosHapticBackend.cs` compiles under
`UNITY_IOS`; no `.asmdef` change needed.

### 11.6 Phases

| # | Phase | Creates | Done when |
|---|---|---|---|
| **I0** | Toolchain proof | Xcode + Unity iOS Build Support verified; empty Unity iOS build runs on the iPhone; repo skeleton committed | The slowest, most environment-fragile step passes before any real code exists |
| **I1** | **Hello bridge** (U1's twin) | `CapHapticsBridge.swift` with `getBridgeVersion` only + `HLog`; copy script; `IosHapticBackend` stub; `#elif UNITY_IOS` arm in `Haptics.Initialize` | Unity app on iPhone logs the native bridge version via P/Invoke — proves Swift-in-Unity compilation, `@_cdecl` symbol resolution, and the version handshake while everything is trivial |
| **I2** | Capability probe + selection | `CapabilityProbe`, `CapabilitiesJson`, `TierSelector`, `NoOpBackend`; harness app with capability dump; XCTests for selector + JSON | Diagnostics overlay in the Unity build shows honest iOS capabilities parsed by the **unchanged** C# parser; simulator reports tier 0 gracefully |
| **I3** | Core Haptics backend | `CoreHapticsBackend` + engine lifecycle, `PatternRegistry`, `PrimitiveSynthesis`, `IntensityScaler`; `playPattern`/`playComposition`/`playWaveform` on tier 3; harness pattern grid | All 10 patterns feel right on the iPhone (the A5-style tuning session, in the harness); backgrounding + a phone call don't kill playback for the rest of the session |
| **I4** | Generator backend + forced tier | `GeneratorBackend`, `setForcedTier` (clamped, like Android), tier switcher in harness | Forcing tier 2 on the Core-Haptics iPhone audibly/haptically changes rendering; every pattern still fires; waveform best-effort works |
| **I5** | Full Unity integration | Complete `IosHapticBackend`, C#-side enum manifest wiring, `Cancel`, asset routing verified | Unity demo scene: pattern grid, playground sliders, **`HapticPatternAsset`s** all work on the iPhone; capability panel + tier override work end to end |
| **I6** | Hardening | No-throw audit of every `@_cdecl` body; fuzz the boundary from C# (negative counts, huge arrays, calls before init, double init, cancel-while-playing); interruption-recovery test | Nothing crashes; result codes are honest; A7's bar, on iOS |
| **I7** | Docs + closeout | README section (iOS install = nothing to do — files ship in the package), §11.4 matrix updated with tuned values, snapshot section updated, M5 marked done | §10 M5's done-when holds: `Play(Success)` feels correct, capability panel honest, Editor stub untouched |

I1 before any haptics code for the same reason U1 existed: the interop toolchain is the
highest-risk, lowest-information-per-failure part — prove it while failures are readable.

### 11.7 iOS-specific risks

| Risk | Mitigation |
|---|---|
| Swift-in-Unity compilation quirks (bridging, symbol visibility) | I1 proves the whole chain with one trivial function; `@_cdecl` + C types only |
| `CHHapticEngine` silently stopped (background, interruption, policy) | reset/stopped handlers + lazy restart-before-play (I3); harness has a "call me then retry" test step |
| One iPhone, always tier 3 → generator path never chosen naturally | Forced-tier override (I4) + pure `TierSelector` XCTests — the §2 story, verbatim |
| System Haptics toggle off → silent no-op mistaken for a bug | Not queryable; diagnostics states `UNKNOWN` + README FAQ; stopped-reason logged when the engine refuses |
| Simulator has no haptics at all | Tier 0 path is a first-class citizen from I2; every feel-criterion is on-device |
| Stale plugin copy in Unity package | Native bridge-version handshake (I1) — the stale-AAR defense, ported |
| Generator timing smear for multi-beat best-effort renderings | Accepted and documented; tier 2 is a fallback for hardware that barely exists — correctness over beauty |
