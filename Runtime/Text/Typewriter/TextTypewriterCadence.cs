namespace DialogueSystem.Text
{
    using System;
    public sealed class TextTypewriterCadence
    {
        private readonly TextTypewriterTimingSettings
            settings;


        public TextTypewriterCadence(
            TextTypewriterTimingSettings settings
        )
        {
            this.settings =
                settings ??
                throw new ArgumentNullException(
                    nameof(settings)
                );
        }


        public float GetDelayMultiplier(
            string text,
            int characterIndex
        )
        {
            if (
                string.IsNullOrEmpty(text) ||
                characterIndex < 0 ||
                characterIndex >= text.Length
            )
            {
                return 1f;
            }


            char character =
                text[characterIndex];


            if (
                character == '\n' ||
                character == '\r'
            )
            {
                return settings
                    .LineBreakMultiplier;
            }


            if (
                character == ' ' ||
                character == '\t'
            )
            {
                return settings
                    .WhitespaceMultiplier;
            }


            if (
                Contains(
                    settings.ClauseCharacters,
                    character
                )
            )
            {
                return settings
                    .ClauseMultiplier;
            }


            if (
                Contains(
                    settings.DashCharacters,
                    character
                )
            )
            {
                return settings
                    .DashMultiplier;
            }


            if (
                Contains(
                    settings.SentenceCharacters,
                    character
                )
            )
            {
                /*
                 * "...", "?!", "!!!"
                 *
                 * Большую паузу получит
                 * только последний знак.
                 */
                if (
                    HasSentencePunctuationAfter(
                        text,
                        characterIndex
                    )
                )
                {
                    return 1f;
                }


                return settings
                    .SentenceMultiplier;
            }


            return 1f;
        }


        private bool HasSentencePunctuationAfter(
            string text,
            int characterIndex
        )
        {
            int nextIndex =
                characterIndex + 1;


            if (nextIndex >= text.Length)
                return false;


            return Contains(
                settings.SentenceCharacters,
                text[nextIndex]
            );
        }


        private static bool Contains(
            string characters,
            char character
        )
        {
            return
                !string.IsNullOrEmpty(characters) &&
                characters.IndexOf(character) >= 0;
        }
    }
}
