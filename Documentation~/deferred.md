# Deferred after Extraction V1

- Completion/result API and integration with external orchestration; optional EcaSystems adapter only in a later iteration.
- SpeechBubble extraction using the shared Text subsystem.
- Optional Unity Localization adapter, localization automation/table generation and locale-refresh behavior.
- Dialogue/view ownership across scenes; disposing an active controller does not hide its view.
- Repeated StartDialogue currently replaces playback without a queue or completion notification.
- UI reload hides/recreates the view without restoring controller state; viewport resize does not repaginate an existing line.
- `[event]` currently logs through DialogueController; no completion meaning or gameplay dispatch is assigned to it.
- Instant presentation bypasses typewriter commands, including `[wait]` and `[event]`; inline `[instant]` has different semantics. Preserve until explicitly revisited.
- Shared mutable `TextRichEffectRegistry.Default` and extension ownership.
- Null-safety, malformed data and missing keys; no broad defensive-code changes in this extraction.
- Automated tests for pagination/markup/typewriter and controller lifecycle.
- Sandbox package import, sample scenes, physical input/localization adapters and visual verification.
- Unity version compatibility beyond 6000.5.6f1, especially PanelRenderer and glyph-processing APIs.
- Existing pagination edge cases, including oversized words and one-shot commands near page boundaries, remain unchanged.
- Existing commented legacy code and obsolete serialized timing fields remain unchanged.
