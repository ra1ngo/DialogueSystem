namespace DialogueSystem
{
    using System;

    [Serializable]
    public class DialogueLine
    {
        public string speakerKey;
        public string textKey;

        public string presentation =
            DialoguePresentationIds.Standard;
    }
}
