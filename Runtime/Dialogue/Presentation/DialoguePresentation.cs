namespace DialogueSystem
{
    public sealed class DialoguePresentation
    {
        public string StyleClass { get; }

        public DialogueTypingMode Typing { get; }

        public bool ShowSpeaker { get; }


        public DialoguePresentation(
            string styleClass,
            DialogueTypingMode typing,
            bool showSpeaker
        )
        {
            StyleClass =
                styleClass;

            Typing =
                typing;

            ShowSpeaker =
                showSpeaker;
        }
    }
}
