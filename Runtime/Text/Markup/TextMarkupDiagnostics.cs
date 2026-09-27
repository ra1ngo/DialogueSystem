namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class TextMarkupDiagnostics
    {
        [System.Diagnostics.Conditional(
            "UNITY_EDITOR"
        )]
        [System.Diagnostics.Conditional(
            "DEVELOPMENT_BUILD"
        )]
        public static void ValidateAndLog(
            string source,
            string context = null
        )
        {
            IReadOnlyList<TextToken> tokens =
                TextTokenParser.Parse(
                    source
                );


            IReadOnlyList<string> diagnostics =
                TextMarkupValidator.Validate(
                    tokens
                );


            for (
                int i = 0;
                i < diagnostics.Count;
                i++
            )
            {
                string prefix =
                    string.IsNullOrWhiteSpace(
                        context
                    )
                        ? "Text markup"
                        : $"Text markup ({context})";


                Debug.LogWarning(
                    $"{prefix}: " +
                    diagnostics[i]
                );
            }
        }
    }
}
