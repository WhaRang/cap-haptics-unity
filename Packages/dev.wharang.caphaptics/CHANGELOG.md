# Changelog

All notable changes to this package are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com); versions follow [semver](https://semver.org).

## [0.11.0] - 2026-08-17

### Added
- **App-level off switch.** `Haptics.Enabled = false` makes every playback call return
  the new `HapticResult.Disabled` (code 7, appended to the wire enum on all three sides)
  without reaching the device, and cancels anything already playing; `= true` re-arms
  instantly. Not persisted by design — wire it to your settings system. `Cancel()` stays
  functional while disabled. Requires AARs rebuilt from android repo ≥ this version
  (enum manifest agreement).
- **Injectable log adaptor.** `Haptics.SetLogger(IHapticLogger)` routes the C# layer's
  log lines into your own pipeline (file, analytics, silent sink) instead of the Unity
  console; null restores the default. A throwing logger is caught and reported — the
  no-throw guarantee holds. Native-side logging (logcat / os_log, tag `CapHaptics`) is a
  separate channel and unaffected.

### Changed
- **Package renamed** `com.cap.haptics` → `dev.wharang.caphaptics` (displayName
  "Cap Haptics") for Asset Store UPM publishing under the verified `dev.wharang`
  namespace, and the **C# root namespace renamed** `Cap.Haptics.*` → `CapHaptics.*`
  to match the asmdef names — which also removes the trap where the bare name
  `Haptics` resolved to the `Cap.Haptics` namespace instead of the facade class.
  **Breaking for early adopters:** update the folder name / `manifest.json` reference
  and `using` directives (`Cap.Haptics.Client` → `CapHaptics.Client`, etc.). Serialized
  `HapticPatternAsset`s survive unchanged (GUID-bound, no SerializeReference); the
  native ABI is untouched.
- Type names normalized to the singular `Haptic` prefix — `HapticsDiagnosticsOverlay` →
  `HapticDiagnosticsOverlay`, `IHapticsLogger` → `IHapticLogger`, `HapticsLogLevel` →
  `HapticLogLevel` — matching `HapticPattern`, `HapticResult` and friends. `Haptics`
  survives only as the facade class itself.
- Enum XML docs rewritten platform-neutrally: the C# enums are the canonical wire
  vocabulary (not "mirrors of Kotlin"), with each platform's actual rendering stated —
  including that `ViewFeedback` is an Android-only channel.

## [0.10.0] - 2026-08-10

### Added
- **iOS backend.** The same semantic API now plays on iPhone: Core Haptics on tier 3
  (all 10 patterns, compositions with every primitive synthesized from CH events, and
  waveforms rendered as continuous-event envelopes), `UIFeedbackGenerator` on tier 2
  (native notification/impact/selection renderings, with waveforms and compositions
  degraded to merged impact beats), honest no-op on haptic-less hardware (iPad,
  simulator). Swift plugin sources ship in `Plugins/iOS/` and compile into the exported
  Xcode project — zero configuration. The forced-tier override, intensity dial,
  `HapticPatternAsset` (all three modes), repeat + cancel, and the diagnostics overlay
  work unchanged; the capabilities panel reports the iOS probe through the same JSON.
- Native bridge-version handshake for iOS, mirroring the AAR check: stale Swift sources
  fail `Initialize()` loudly.

### Notes
- iOS has no tier 1 — the ladder is 3 → 2 → 0; forcing tier 1 lands on 2.
- The generator tier obeys the System Haptics setting while Core Haptics does not; see
  the README's "I felt nothing" FAQ.

## [0.9.0] - 2026-08-09

### Changed
- **One asset = one rendering.** `HapticPatternAsset` now has a top-level `Mode` —
  Waveform / Composition / PredefinedEffect — replacing the "waveform + optional tier
  hints" shape. The mode is authoritative in code too: `Haptics.Play(asset)` always plays
  that rendering, and degradation on lower tiers happens natively through the same
  approximation machinery the built-in patterns use. The Inspector shows only the active
  mode's fields, and preview plays exactly what the asset is.
- **Segments unified with curves.** Each waveform segment is now either a static Buzz
  (duration + amplitude) or a Curve (duration + drawable envelope + probe count), plus a
  leading `delayMs` — so a click, a gap and a swell mix in one list. A custom drawer shows
  only the fields the segment type uses. Curve probes coarsen proportionally if the total
  would exceed the native 500-step cap.
- **adb preview logs are off by default** — toggle "Log adb commands" in the Inspector
  (persisted via EditorPrefs). Errors always log.
- Breaking for 0.8.0 assets: the top-level curve/segment fields were replaced; re-author
  existing test assets (one Curve segment reproduces an 0.8.0 curve asset).

## [0.8.0] - 2026-08-09

### Added
- **Curve authoring**: draw the strength envelope on an `AnimationCurve` (the new default
  waveform source) over a configurable duration; it samples into waveform steps with
  consecutive-equal merging, staying under the native 500-step cap. Segment-list mode
  remains for precise rhythms.
- **Tier-selectable preview**: the Inspector's preview can now play the T2 hint
  (`prebaked`) and T3 hint (`primitives`) over adb, not just the waveform — with an Auto
  mode that picks the richest rendering the asset offers. Shell limitations (no primitive
  scales, no prebaked intensity, no SDK tier logic) are stated in the Inspector.

### Changed
- Assets created with 0.7.0 deserialize into Curve mode (the new field's default) — set
  `Waveform Source` back to `Segments` on any existing asset that used the segment list.

## [0.7.0] - 2026-08-09

### Added
- **M3 — pattern assets**: `HapticPatternAsset` (Create → cap-haptics → Haptic Pattern) —
  a designer-authored waveform (segment list) with optional per-tier hints (T3 composition
  steps, T2 predefined effect); `Haptics.Play(asset, intensity)` consults the hints against
  the active tier, so the forced-tier override applies to assets too.
- Inspector "Preview on device (adb)" button: feel the waveform on a USB-attached phone from
  Edit mode — no build, no Play mode. The adb command and device reply are logged verbatim.
- `IHapticBackend` gained `PlayEffect` / `PlayComposition`, surfacing bridge methods that
  existed since v1 — no ABI change, no AAR rebuild.

## [0.6.0] - 2026-08-08

### Added
- **M1 — zero-setup Android install**: an editor build hook
  (`IPostGenerateGradleAndroidProject`) injects the kotlin-stdlib dependency into the
  exported Gradle project automatically. The manual Custom Main Gradle Template step is
  gone; projects that already declare kotlin-stdlib are detected and left untouched.

### Changed
- Runtime scripts reorganized into `Backend` / `Client` / `PatternTypes` folders with
  matching sub-namespaces (`Cap.Haptics.Client.Haptics`, `Cap.Haptics.PatternTypes.HapticPattern`).
  **Breaking for early adopters:** update `using Cap.Haptics;` accordingly.
- README install steps reduced from four to three.

## [0.5.0] - 2026-08-06

### Added
- **U1 — bridge**: `Haptics.Initialize()` with bridge-version guard; Android JNI backend and
  Editor log-only stub behind one seam.
- **U2 — capabilities**: `Haptics.Capabilities` parsed once from the native probe;
  `HapticsDiagnosticsOverlay` Caps tab with per-effect/per-primitive support, measured
  primitive durations and a system-haptics warning.
- **U3 — patterns**: `Haptics.Play(pattern, intensity)`, `SetForcedTier`, `Cancel`; full
  C# mirrors of the six wire enums, validated against the AAR's enum manifest at init;
  Patterns tab generated from the enum with tier override.
- **U4 — playground**: `Haptics.PlayWaveform(timings, amplitudes, repeatIndex)`; Playground
  tab building pulse trains from sliders, with repeat.
- **U5 — packaging**: README, sample, changelog; overlay respects the display safe area.

### Notes
- Requires bridge ABI **v2** (AARs included). Android minSdk 26.
- Consumers must declare `kotlin-stdlib` in a custom `mainTemplate.gradle` — see README.
