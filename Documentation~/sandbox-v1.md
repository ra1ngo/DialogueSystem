# Sandbox Validation V1

Validated on 2026-09-27 using Unity **6000.5.6f1** (revision `0e0577a1a2ac`), executable `C:/Program Files/Unity/Hub/Editor/6000.5.6f1/Editor/Unity.exe`.

Package baseline: branch `codex/extraction-v1`, commit `6023b144581663a1b055b39ea595a5aa42d221b0`. Project: `C:/projects/DialogueSystemSandbox`. The Vano project was only read; it was never opened in Unity. The existing audit.md content is unchanged.

## Installation and assets

Sandbox `Packages/manifest.json` uses:

```json
"com.ra1ngo.dialogue-system": "file:../../DialogueSystem"
```

Unity resolves relative file dependencies from the Packages directory. The initially suggested `file:../DialogueSystem` looked for `DialogueSystemSandbox/DialogueSystem/package.json` and failed; the corrected path resolves the canonical adjacent repository. No package runtime source was copied into Assets.

Real Unity Package Manager resolution, asset import and Editor compilation passed. `DialogueSystem.Runtime` was loaded as its own assembly. UXML, USS, the prefab, PanelSettings, PanelTextSettings, TextStyleSheet, theme, DialoguePresentationSettings and TextTypewriterTimingSettings deserialized as the expected types. The prefab contains no missing scripts. Serialized panel/UXML/timing references resolve correctly; AssetDatabase dependency traversal found only package-local assets. The panel renders using the package theme/style configuration. There are no Vano asset references.

## Sandbox-only composition

Scene: `Assets/DialogueSystemSandbox/Scenes/DialogueBasic.unity`.

- `Runtime/SandboxLocalization.cs`: in-memory speaker/text keys, six demo lines and a numbered 100-word pagination probe.
- `Runtime/SandboxInputAdapter.cs`: Enter/Space to Submit, Escape to Cancel; no package input ownership.
- `Runtime/SandboxBootstrap.cs`: direct controller/resolver construction, wait for IsBootstrapped, start/restart demo (R), dispose on teardown.
- `Runtime/SandboxRuntimeValidation.cs`: sandbox-only runtime probes and panel image capture. In normal Play Mode these probes are disabled and the manual demo starts.
- `Editor/SandboxValidation.cs`: import/reference checks, scene creation, Play Mode orchestration and Windows development build.

No package Tests, Samples~, completion API, DI framework, localization adapter or gameplay system was introduced. Sandbox files/results remain local and were not committed or pushed by this package task. Existing Sandbox user changes were preserved; Unity also updated its own project/render-pipeline assets during import/build.

## Runtime and build result

Unity ran with graphics enabled in batch mode, entered real Play Mode, and passed **33 recorded checks**. Keyboard events were queued through the installed Input System and received by the Sandbox adapter; this was automated input, not a manual keyboard session. The batch probe routes keyboard input to the Game View while unfocused.

| Area | Exercised result |
|---|---|
| Startup | Positive UI readiness, initially hidden view, visible after StartDialogue, localized speaker |
| Submit ordering | Typing reveals to wait; wait continues; reveal completes current page; subsequent Submit advances page, then line, then closes |
| Cancel | Escape closes active playback and stops typing; inactive Escape remains harmless |
| Presentations | Standard bottom layout with speaker; system top layout without speaker and instant text; unknown ID falls back to standard |
| Pagination | Explicit page break and five automatic pages at the batch Game View size; balanced markup and measured fit for each; all 100 numbered words appear exactly once in order |
| Typewriter | Ordinary typing, timed pause/resumption, slow/fast speed scopes, inline instant scope, punctuation delay/resumption and Submit completion |
| Rich text | Bold, italic and named warning style reach UI Toolkit and are visible in captured output |
| Effects | Wave, shake, rainbow and bounce each changed actual processed glyph geometry/tint across frames; no glyph-processing errors |
| Text events | Event reaches DialogueView and the retained DialogueController log exactly once during reveal |
| Existing Instant semantics | Presentation Instant skips event/wait commands; no new semantics added |

The default batch Game View layout was 640x480. Standard geometry ended 40 units above the bottom; system began at y=40. Layout was verified geometrically rather than assuming the sign of resolvedStyle.bottom.

For visual review, the same UI panel was rendered temporarily to a 1280x720 RenderTexture and read back to PNG; the previous render target was restored without saving package assets. Standard/system placement, speaker visibility, formatting and named style were inspected. Two effects frames show different wave/shake/bounce positions and rainbow colors. Ordinary ScreenCapture file capture did not produce images in batch mode, hence the explicit panel readback.

Unity **StandaloneWindows64 Development Build succeeded**, with **0 errors and 0 warnings** in the final BuildReport. The built executable was not launched separately; runtime validation was in Editor Play Mode.

Evidence remains under Sandbox `ValidationResults/`: `unity.log`, `assets.txt`, `runtime-checks.txt`, empty final `errors.txt`, `result.txt` (`0: runtime passed; build Succeeded`), `build.txt`, `source-comparison.txt`, `standard.png`, `system.png`, `effects-a.png`, `effects-b.png`, and `Build/DialogueSandbox.exe`.

## Independent Vano comparison and package changes

Fresh comparison covered all 49 source C# files: controller, paginator, view, presentation, token/parser, whitespace/markup, typewriter/commands/cadence, rich renderer/formats and all effects. After removing namespace/import wrapping, 46 files match; the remaining three differ only in the approved input/localization boundary, Submit/Cancel interface methods and public readiness property. No unexpected behavioral differences were found. Source snapshot hashes and the audit hash still match Extraction V1.

**No extraction/runtime/serialized-reference defect was found. No package runtime C# or UI asset was changed after baseline.** Validation adjustments were confined to Sandbox: local dependency path, background keyboard routing, geometry-based assertions, render-target capture, and teardown that only disposes rather than touching an already-disabled UI.

Package changes: canonical BSD Zero Clause license with copyright `2026 ra1ngo`; manifest SPDX `0BSD`; requested ChatGPT development acknowledgement; this record and README validation link. Unity-generated metadata for LICENSE.md and the existing audit.md is retained so these package text assets have stable identities; audit.md itself is untouched.

## Warnings and limits

Final captured Play Mode errors: none. Final build errors/warnings: zero. Editor startup/shutdown also logged environment diagnostics about licensing access-token/client validation (the installed license still authorized the run), D3D12 info-queue availability, WindowsStandalone native-extension discovery and virtual-key mapping in the background session; they did not prevent import, rendering or the successful player build.

An early Sandbox teardown called EndDialogue after UI shutdown and hit a UI Toolkit style-access exception. The unchanged source View has the same lifetime sensitivity; the harness now follows the requested dispose-only teardown. Scene-transition/UI lifetime redesign remains deferred.

Existing limitations are retained: no completion signal, replacement on repeated StartDialogue, no locale refresh of prepared pages, Instant presentation command skipping, text events only logging, UI reload/resize behavior, mutable default effect registry and current null-safety. See deferred.md. No broader compatibility or exhaustive runtime coverage is claimed.
