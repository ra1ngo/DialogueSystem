# Extraction V1 record

## Source and scope

Reference: the current files under `C:/projects/vano/vano/Assets`, read during extraction. The source project and `DialogueSystemSandbox` were not written to or opened in Unity. `audit.md` remains unchanged (SHA-256 `97aa29570169b7bb4580b9eaee7fbe0c47677c2b94b8d220dd06eef57cb6e03f`).

| Source area | Package area | Comparison |
|---|---|---|
| `UI/Dialogue` (including Presentation) | `Runtime/Dialogue` | All 12 C# files and UXML/USS included; three C# boundary adaptations listed below |
| `UI/Text` | `Runtime/Text` | All 37 C# files included; bodies unchanged |
| `Game/Config/UI/DialoguePresentations.asset` | `Runtime/Dialogue/Settings` | Default presentations retained |
| `Text/Settings/DefaultTypewriterTiming.asset` | `Runtime/Text/Settings` | Serialized values retained, including the existing obsolete field |
| `UI Toolkit/PanelSettings.asset`, `DialogueTextSettings.asset`, `DialogueTextStyles.asset`, `UnityThemes/UnityDefaultRuntimeTheme.tss` | `Runtime/Dialogue/UI` | Panel/theme/style configuration retained |
| UI object inside `Infrastructure/DI/AppRoot.prefab` | `Runtime/Dialogue/UI/DialogueUI.prefab` | Only Transform, PanelRenderer and DialogueView retained; standalone parent, name and local origin |

There are 49 extracted C# files plus the new `IDialogueLocalization`. A single asmdef replaces reliance on Assembly-CSharp. Dialogue uses namespace `DialogueSystem`; Text uses `DialogueSystem.Text` without a reverse dependency on Dialogue. Text remains available for future consumers without bringing in SpeechBubble now.

## Exact adaptations

- `DialogueController`: remove input-controller/mode fields, constructor parameters, subscriptions and mode-switch calls; expose former OnSubmit/OnCancel bodies as public Submit/Cancel; resolve keys through IDialogueLocalization without a hard-coded table.
- `IDialogueSystem`: add Submit and Cancel, retaining StartDialogue and EndDialogue. No completion/session/result API.
- `DialogueView`: change only IsBootstrapped visibility from internal to public, so an application in another assembly can wait for the same readiness condition previously used by AppBootstrap.
- Add namespaces/imports; preserve source bodies, comments and legacy fragments. Normalize copied text files to UTF-8/LF, decoding source CP1251 files without dropping characters.
- Regenerate GUIDs intentionally for all copied files. Remap serialized references and update custom assembly/type identifiers. Built-in Unity resource references remain unchanged. New folders/prefab/interface/asmdef have their own metadata.

## Validation

- Compared all 49 extracted C# files with the live source, removing only namespace/import wrapping for comparison. 46 match exactly after that normalization; the three differences are the explicit adaptations above. Pagination, markup, commands, cadence, rich renderer/effects and presentation logic are preserved.
- Compared all eight copied non-C# files after GUID and custom assembly/type-name remapping. Compared the minimal prefab against the retained source UI components.
- Scanned runtime sources for excluded gameplay/input/localization/DI/orchestration dependencies. No compile-time references to them remain.
- Checked unique metadata GUIDs, package-local/built-in GUID targets, prefab-local file IDs, UXML stylesheet paths and the built-in theme reference.
- Checked package/asmdef JSON, metadata coverage and unchanged hashes of every extracted source file, its metadata, the source prefab and audit.md.
- Compiled the runtime C# with the Roslyn compiler and .NET Standard 2.1/UnityEngine reference libraries shipped in Unity 6000.5.6f1. No references to Vano assemblies, Input System, VContainer, Localization or Addressables were used. Diagnostics-enabled compilation was also checked. Only three existing CS0649 warnings for Unity-serialized fields remain.
- Compiled a separate consumer assembly against the resulting DLL to verify public construction, localization implementation, readiness access and all four IDialogueSystem operations across an assembly boundary. The consumer was compile-only, not executed or shipped as a sample/test.
- This is standalone C# compilation against Unity libraries, **not Unity Editor compilation/import, prefab deserialization or a PlayMode test**. Unity asset import and visual/runtime behavior must be validated in the next Sandbox iteration.

## Known behavior differences from Vano

1. Physical input is no longer subscribed automatically. The host calls Submit/Cancel. Given the same calls, the original waiting/reveal/page/line/cancel decision order is preserved.
2. Starting/ending dialogue no longer changes action maps, stops movement or restores gameplay input. The consuming application owns those effects.
3. Localization provider/table/initialization and missing-key behavior are no longer supplied by Vano. The host supplies Get(key). Speaker/text keys and the empty-speaker shortcut are unchanged; rendered text depends on that provider returning equivalent strings.
4. No game bootstrap, DI root, automatic intro or DontDestroyOnLoad is included. The host must create the controller, supply settings, wait for IsBootstrapped and own lifetime. The UI prefab is standalone rather than a child of AppRoot and contains no WorldUIRoot/SpeechBubble layer.
5. IsBootstrapped is publicly readable across the assembly boundary; its state transitions are unchanged. Namespace/assembly/GUID changes require explicit integration rather than reusing old serialized identities.

No other algorithmic behavior changes were introduced. Existing limitations remain: replacement on repeated start, no completion notification, Dispose only unsubscribes, text events only log, Instant presentation skips typewriter commands, UI reload does not restore playback, static default effect registry, current null/oversized-word behavior. See deferred.md.
