# Dialogue System

Extraction V1 baseline of a linear dialogue player and its reusable text subsystem.
Targets **Unity 6000.5.6f1** and the existing UI Toolkit implementation. No older Unity compatibility is claimed.

## Package

- UPM name: `com.ra1ngo.dialogue-system`, version `0.1.0`.
- One runtime assembly: `DialogueSystem.Runtime`.
- Dialogue API namespace: `DialogueSystem`.
- Shared text API namespace: `DialogueSystem.Text`.
- Required built-in module: `com.unity.modules.uielements` (`1.0.0`).
- No required Input System, VContainer, Unity Localization, Addressables or external orchestration framework.

Install through Unity Package Manager using this repository's Git URL (pin a reviewed tag/commit or branch), or select its `package.json` as a local package. Installing into the separate Sandbox is outside this iteration.

## Compose in the consuming application

1. Instantiate or place `Runtime/Dialogue/UI/DialogueUI.prefab`. It contains only a Transform, PanelRenderer and DialogueView, with package-local UXML, panel/text settings and typewriter timing references. It does not create a controller or persist across scenes automatically.
2. Provide an `IDialogueLocalization` implementation. Its `string Get(string key)` resolves both speaker and text keys. Table selection, locale readiness, missing-key behavior and locale changes belong to the application. No localization data is included.
3. Assign `Runtime/Dialogue/Settings/DialoguePresentations.asset` to the application's composition component and construct the controller:

```csharp
using DialogueSystem;

// view, localization and presentations are supplied by the application.
var resolver = new DialoguePresentationResolver(presentations);
var controller = new DialogueController(view, localization, resolver);
IDialogueSystem dialogue = controller;
```

4. Before the first `StartDialogue`, wait until `view.IsBootstrapped` is true and the localization provider is ready. For example, an application's coroutine can yield `new UnityEngine.WaitUntil(() => view.IsBootstrapped)`. The flag means that the required UI elements and a positive viewport size exist; it is not a dialogue-completion flag.
5. Pass the existing key-based data and forward physical input externally:

```csharp
dialogue.StartDialogue(new DialogueSystem.Dialogue
{
    lines = new[]
    {
        new DialogueLine
        {
            speakerKey = "speaker.name",
            textKey = "greeting",
            presentation = DialoguePresentationIds.Standard
        }
    }
});

// Call these from the application's own input callbacks, not all at once:
// dialogue.Submit();      // Continue wait, reveal, next page, or next line.
// dialogue.Cancel();      // Close only when active.
// dialogue.EndDialogue(); // Explicitly hide and stop playback.
```

6. Dispose the controller when its owner ends. As in the source, `Dispose()` only removes subscriptions; it does not close the UI. If the application needs to close it during teardown, it must call `EndDialogue()` explicitly before `Dispose()`.

The application owns player blocking, action maps, movement, scene lifetime and restoration of gameplay. The package supplies **no completion event/result or public active-session state** in V1; that limitation is intentionally retained, not replaced with inference from UI visibility or text events. Call `Submit()` for normal progression: the retained concrete `Next()` method advances a whole line directly and bypasses the current page/wait handling.

## Included behavior

Linear key-based lines; speaker visibility; standard/system presentations; automatic and explicit page breaks; whitespace normalization; rich formatting and named styles; wave, shake, rainbow and bounce effects; typewriter speed scopes, instant scopes, pauses, waits and text events.

The default `standard` presentation types text at the bottom of the screen and shows the speaker. `system` renders immediately at the top and hides the speaker. Unknown presentation IDs fall back to standard. UXML/USS and the default timing/presentation values retain the source configuration.

The view relies on the exact UXML element names and its serialized timing asset. The default panel uses the built-in Unity theme and does not include a custom font. Text effects use `TextElement.PostProcessTextVertices`; UI initialization uses `PanelRenderer.RegisterUIReloadCallback`. These are compatibility-sensitive APIs for the targeted Unity version.

## Extraction status

Source algorithms were preserved; game input and localization boundaries were detached. Extracted scripts/assets have fresh GUIDs, with serialized references remapped inside the package. This is not an automatic migration of existing game scenes or serialized type identities.

See `Documentation~/extraction-v1.md` for source comparison, validation and known behavior differences, and `Documentation~/deferred.md` for deliberately deferred issues. `audit.md` is the unchanged historical audit. No scenes, tests, SpeechBubble feature or game content are included in V1.
