# Changelog

All notable changes to this package are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com); versions follow [semver](https://semver.org).

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
