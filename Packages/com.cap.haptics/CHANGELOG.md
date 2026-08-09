# Changelog

All notable changes to this package are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com); versions follow [semver](https://semver.org).

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
