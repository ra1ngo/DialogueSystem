namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using System.Globalization;

    public static class TextTypewriterTokenParser
    {
        public static IReadOnlyList<TextTypewriterToken>
            Parse(
                IReadOnlyList<TextToken> tokens
            )
        {
            var result =
                new List<TextTypewriterToken>();

            foreach (TextToken token in tokens)
            {
                if (token.Type == TextTokenType.Text)
                {
                    result.Add(
                        new TextTypewriterTextToken(
                            token.Text
                        )
                    );

                    continue;
                }

                ParseTag(
                    token,
                    result
                );
            }

            return result;
        }

        private static void ParseTag(
            TextToken token,
            List<TextTypewriterToken> result
        )
        {
            switch (token.Name)
            {
                case "speed":
                    ParseSpeed(
                        token,
                        result
                    );
                    break;

                case "instant":
                    result.Add(
                        token.IsClosing
                            ? new PopInstantTextCommand()
                            : new PushInstantTextCommand()
                    );
                    break;

                case "pause":
                    if (
                        !token.IsClosing
                        &&
                        TryParseFloat(
                            token.Value,
                            out float duration
                        )
                    )
                    {
                        result.Add(
                            new PauseTextCommand(
                                duration
                            )
                        );
                    }

                    break;

                case "wait":
                    if (!token.IsClosing)
                    {
                        result.Add(
                            new WaitTextCommand()
                        );
                    }

                    break;

                case "event":
                    if (
                        !token.IsClosing
                        &&
                        !string.IsNullOrWhiteSpace(
                            token.Value
                        )
                    )
                    {
                        result.Add(
                            new EventTextCommand(
                                token.Value
                            )
                        );
                    }

                    break;
            }
        }

        private static void ParseSpeed(
            TextToken token,
            List<TextTypewriterToken> result
        )
        {
            if (token.IsClosing)
            {
                result.Add(
                    new PopSpeedTextCommand()
                );

                return;
            }

            if (
                TryParseFloat(
                    token.Value,
                    out float speed
                )
            )
            {
                result.Add(
                    new PushSpeedTextCommand(
                        speed
                    )
                );
            }
        }

        private static bool TryParseFloat(
            string value,
            out float result
        )
        {
            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result
            );
        }
    }
}
