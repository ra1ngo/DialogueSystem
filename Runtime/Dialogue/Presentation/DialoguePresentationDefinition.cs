namespace DialogueSystem
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class DialoguePresentationDefinition
    {
        [SerializeField]
        private string id =
            DialoguePresentationIds.Standard;

        [SerializeField]
        private string styleClass;

        [SerializeField]
        private bool instant;

        [SerializeField]
        private bool showSpeaker = true;


        public string Id =>
            id;

        public string StyleClass =>
            styleClass;

        public bool Instant =>
            instant;

        public bool ShowSpeaker =>
            showSpeaker;


        public DialoguePresentation
            CreatePresentation()
        {
            return new DialoguePresentation(
                styleClass,
                instant
                    ? DialogueTypingMode.Instant
                    : DialogueTypingMode.Typewriter,
                showSpeaker
            );
        }
    }
}
