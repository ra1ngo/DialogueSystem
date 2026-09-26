namespace DialogueSystem
{
    using DialogueSystem.Text;

    using System;
    using UnityEngine;

    public sealed class DialogueController :
        IDialogueSystem,
        IDisposable
    {
        private readonly DialogueView dialogueView;

        private Dialogue dialogue;
        private int currentLineIndex;
        private bool isDialogueActive;

        private string[] currentPages;
        private int currentPageIndex;
        private DialoguePresentation currentPresentation;
        private readonly IDialogueLocalization localization;

        private readonly DialoguePresentationResolver presentationResolver;


        public DialogueController(
            DialogueView dialogueView,
            IDialogueLocalization localization,
            DialoguePresentationResolver presentationResolver
        )
        {
            this.dialogueView =
                dialogueView;

            this.localization =
                localization;

            this.presentationResolver =
                presentationResolver;


            dialogueView.TextEventTriggered +=
                OnTextEventTriggered;
        }


        public void Dispose()
        {
            dialogueView.TextEventTriggered -=
                OnTextEventTriggered;
        }


        private void OnTextEventTriggered(
            string eventId
        )
        {
            // Позже здесь будет нормальная обработка
            // text events.
            UnityEngine.Debug.Log(
                $"Text event: {eventId}"
            );
        }


        public void Submit()
        {
            if (!isDialogueActive)
                return;

            if (dialogueView.IsWaiting)
            {
                dialogueView.ContinueText();
                return;
            }

            if (dialogueView.IsTyping)
            {
                dialogueView.CompleteText();
                return;
            }

            if (HasNextPage())
            {
                ShowNextPage();
                return;
            }

            Next();
        }

        private bool HasNextPage()
        {
            return
                currentPages != null &&
                currentPageIndex + 1 <
                    currentPages.Length;
        }

        private void ShowNextPage()
        {
            if (!HasNextPage())
                return;

            currentPageIndex++;

            ShowCurrentPage();
        }


        public void Cancel()
        {
            if (!isDialogueActive)
                return;

            EndDialogue();
        }


        public void StartDialogue(
            Dialogue newDialogue
        )
        {
            if (newDialogue == null)
                return;

            dialogue =
                newDialogue;

            currentLineIndex =
                0;

            isDialogueActive =
                true;

            dialogueView.Show();
            ShowCurrentLine();
        }


        public void Next()
        {
            currentLineIndex++;

            if (
                dialogue == null ||
                dialogue.lines == null ||
                currentLineIndex >=
                    dialogue.lines.Length
            )
            {
                EndDialogue();
                return;
            }

            ShowCurrentLine();
        }


        public void EndDialogue()
        {
            isDialogueActive = false;

            dialogueView.Hide();

        }


        private void ShowCurrentLine()
        {
            if (
                dialogue == null ||
                dialogue.lines == null ||
                currentLineIndex < 0 ||
                currentLineIndex >=
                    dialogue.lines.Length
            )
            {
                EndDialogue();
                return;
            }

            DialogueLine line =
                dialogue.lines[
                    currentLineIndex
                ];

            currentPresentation =
                presentationResolver.Resolve(
                    line.presentation
                );

            dialogueView.ApplyPresentation(
                currentPresentation
            );



            string speaker =
                ResolveSpeaker(
                    line
                );

            dialogueView.SetSpeaker(
                speaker,
                currentPresentation.ShowSpeaker
            );

            string text =
                 ResolveText(
                     line
                 );


            TextMarkupDiagnostics.ValidateAndLog(
                text,
                $"Dialogue: {line.textKey}"
            );

            currentPages =
                DialoguePaginator.Paginate(
                    text,
                    dialogueView.CanFitText
                );

            currentPageIndex =
                0;

            ShowCurrentPage();
        }

        private void ShowCurrentPage()
        {
            if (
                currentPages == null ||
                currentPageIndex < 0 ||
                currentPageIndex >= currentPages.Length
            )
            {
                return;
            }

            string text =
                currentPages[
                    currentPageIndex
                ];

            switch (currentPresentation.Typing)
            {
                case DialogueTypingMode.Typewriter:

                    dialogueView.ShowText(
                        text
                    );

                    break;


                case DialogueTypingMode.Instant:

                    dialogueView.ShowTextInstant(
                        text
                    );

                    break;
            }
        }

        private string ResolveText(
            DialogueLine line
        )
        {
            return localization.Get(
                line.textKey
            );
        }


        private string ResolveSpeaker(
            DialogueLine line
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    line.speakerKey
                )
            )
            {
                return string.Empty;
            }

            return localization.Get(
                line.speakerKey
            );
        }
    }
}
