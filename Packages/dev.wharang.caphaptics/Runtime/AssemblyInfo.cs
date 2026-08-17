using System.Runtime.CompilerServices;

// The editmode tests exercise internal seams (EnumManifestValidator, the backends,
// the overlay's pulse-train builder) — internal stays the right visibility for
// consumers; tests are the one privileged reader.
[assembly: InternalsVisibleTo("CapHaptics.EditModeTests")]
