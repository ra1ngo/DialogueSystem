namespace DialogueSystem.Text
{
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "TextTypewriterTimingSettings",
        menuName = "Text/Typewriter Timing Settings"
    )]
    public sealed class TextTypewriterTimingSettings :
        ScriptableObject
    {
        [Header("Whitespace")]

        [Min(0f)]
        [SerializeField]
        private float whitespaceMultiplier = 0f;

        [Min(0f)]
        [SerializeField]
        private float lineBreakMultiplier = 3f;


        [Header("Clause punctuation")]

        [SerializeField]
        private string clauseCharacters =
            ",;:";

        [Min(0f)]
        [SerializeField]
        private float clauseMultiplier = 2.5f;


        [Header("Dashes")]

        [SerializeField]
        private string dashCharacters =
            "-–—";

        [Min(0f)]
        [SerializeField]
        private float dashMultiplier = 1.5f;


        [Header("Sentence punctuation")]

        [SerializeField]
        private string sentenceCharacters =
            ".!?…";

        [Min(0f)]
        [SerializeField]
        private float sentenceMultiplier = 5f;


        public float WhitespaceMultiplier =>
            whitespaceMultiplier;

        public float LineBreakMultiplier =>
            lineBreakMultiplier;

        public string ClauseCharacters =>
            clauseCharacters;

        public float ClauseMultiplier =>
            clauseMultiplier;

        public string DashCharacters =>
            dashCharacters;

        public float DashMultiplier =>
            dashMultiplier;

        public string SentenceCharacters =>
            sentenceCharacters;

        public float SentenceMultiplier =>
            sentenceMultiplier;
    }
}
