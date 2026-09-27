namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using System.Globalization;

    public static class TextMarkupValidator
    {
        private readonly struct ScopeEntry
        {
            public string Name { get; }
            public int TokenIndex { get; }

            public ScopeEntry(
                string name,
                int tokenIndex
            )
            {
                Name = name;
                TokenIndex = tokenIndex;
            }
        }


        public static IReadOnlyList<string> Validate(
            IReadOnlyList<TextToken> tokens
        )
        {
            List<string> diagnostics =
                new();

            Stack<ScopeEntry> scopes =
                new();


            for (
                int i = 0;
                i < tokens.Count;
                i++
            )
            {
                TextToken token =
                    tokens[i];


                if (token.Type != TextTokenType.Tag)
                    continue;


                if (!IsKnownTag(token.Name))
                {
                    diagnostics.Add(
                        $"Unknown tag [{token.Name}] " +
                        $"at token {i}."
                    );

                    continue;
                }


                if (token.IsClosing)
                {
                    ValidateClosingTag(
                        token,
                        i,
                        scopes,
                        diagnostics
                    );

                    continue;
                }


                ValidateOpeningTag(
                    token,
                    i,
                    diagnostics
                );


                if (IsScopeTag(token.Name))
                {
                    scopes.Push(
                        new ScopeEntry(
                            token.Name,
                            i
                        )
                    );
                }
            }


            while (scopes.Count > 0)
            {
                ScopeEntry scope =
                    scopes.Pop();

                diagnostics.Add(
                    $"Tag [{scope.Name}] opened at " +
                    $"token {scope.TokenIndex} " +
                    $"was not closed."
                );
            }


            return diagnostics;
        }


        private static void ValidateClosingTag(
            TextToken token,
            int tokenIndex,
            Stack<ScopeEntry> scopes,
            List<string> diagnostics
        )
        {
            if (IsOneShotTag(token.Name))
            {
                diagnostics.Add(
                    $"Tag [{token.Name}] cannot be closed. " +
                    $"Found [/{token.Name}] at token " +
                    $"{tokenIndex}."
                );

                return;
            }


            if (scopes.Count == 0)
            {
                diagnostics.Add(
                    $"Closing tag [/{token.Name}] " +
                    $"at token {tokenIndex} has no " +
                    $"matching opening tag."
                );

                return;
            }


            ScopeEntry current =
                scopes.Peek();


            if (current.Name == token.Name)
            {
                scopes.Pop();

                return;
            }


            bool matchingScopeExists =
                false;


            foreach (ScopeEntry scope in scopes)
            {
                if (scope.Name != token.Name)
                    continue;

                matchingScopeExists =
                    true;

                break;
            }


            if (!matchingScopeExists)
            {
                diagnostics.Add(
                    $"Closing tag [/{token.Name}] " +
                    $"at token {tokenIndex} has no " +
                    $"matching opening tag."
                );

                return;
            }


            diagnostics.Add(
                $"Invalid tag nesting at token " +
                $"{tokenIndex}: expected " +
                $"[/{current.Name}], but found " +
                $"[/{token.Name}]."
            );


            /*
             * Recover the stack so one malformed tag
             * doesn't generate a cascade of misleading
             * diagnostics.
             */
            while (scopes.Count > 0)
            {
                ScopeEntry scope =
                    scopes.Pop();

                if (scope.Name == token.Name)
                    break;
            }
        }


        private static void ValidateOpeningTag(
            TextToken token,
            int tokenIndex,
            List<string> diagnostics
        )
        {
            switch (token.Name)
            {
                case "effect":
                case "style":
                case "format":
                    RequireValue(
                        token,
                        tokenIndex,
                        diagnostics
                    );
                    break;


                case "speed":
                    ValidateSpeed(
                        token,
                        tokenIndex,
                        diagnostics
                    );
                    break;


                case "instant":
                    RequireNoValue(
                        token,
                        tokenIndex,
                        diagnostics
                    );

                    RequireNoParameters(
                        token,
                        tokenIndex,
                        diagnostics
                    );
                    break;


                case "pause":
                    ValidatePause(
                        token,
                        tokenIndex,
                        diagnostics
                    );
                    break;


                case "wait":
                case "page":
                    RequireNoValue(
                        token,
                        tokenIndex,
                        diagnostics
                    );

                    RequireNoParameters(
                        token,
                        tokenIndex,
                        diagnostics
                    );
                    break;


                case "event":
                    RequireValue(
                        token,
                        tokenIndex,
                        diagnostics
                    );

                    RequireNoParameters(
                        token,
                        tokenIndex,
                        diagnostics
                    );
                    break;
            }
        }


        private static void ValidateSpeed(
            TextToken token,
            int tokenIndex,
            List<string> diagnostics
        )
        {
            if (
                !RequireValue(
                    token,
                    tokenIndex,
                    diagnostics
                )
            )
            {
                return;
            }


            RequireNoParameters(
                token,
                tokenIndex,
                diagnostics
            );


            if (
                !float.TryParse(
                    token.Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float speed
                )
            )
            {
                diagnostics.Add(
                    $"Invalid speed '{token.Value}' " +
                    $"at token {tokenIndex}. " +
                    $"Expected a number."
                );

                return;
            }


            if (speed <= 0f)
            {
                diagnostics.Add(
                    $"Speed must be greater than 0 " +
                    $"at token {tokenIndex}. " +
                    $"Received {speed}."
                );
            }
        }


        private static void ValidatePause(
            TextToken token,
            int tokenIndex,
            List<string> diagnostics
        )
        {
            if (
                !RequireValue(
                    token,
                    tokenIndex,
                    diagnostics
                )
            )
            {
                return;
            }


            RequireNoParameters(
                token,
                tokenIndex,
                diagnostics
            );


            if (
                !float.TryParse(
                    token.Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float duration
                )
            )
            {
                diagnostics.Add(
                    $"Invalid pause duration " +
                    $"'{token.Value}' at token " +
                    $"{tokenIndex}. Expected a number."
                );

                return;
            }


            if (duration < 0f)
            {
                diagnostics.Add(
                    $"Pause duration cannot be negative " +
                    $"at token {tokenIndex}. " +
                    $"Received {duration}."
                );
            }
        }


        private static bool RequireValue(
            TextToken token,
            int tokenIndex,
            List<string> diagnostics
        )
        {
            if (
                !string.IsNullOrWhiteSpace(
                    token.Value
                )
            )
            {
                return true;
            }


            diagnostics.Add(
                $"Tag [{token.Name}] at token " +
                $"{tokenIndex} requires a value."
            );

            return false;
        }


        private static void RequireNoValue(
            TextToken token,
            int tokenIndex,
            List<string> diagnostics
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    token.Value
                )
            )
            {
                return;
            }


            diagnostics.Add(
                $"Tag [{token.Name}] at token " +
                $"{tokenIndex} does not accept a value."
            );
        }


        private static void RequireNoParameters(
            TextToken token,
            int tokenIndex,
            List<string> diagnostics
        )
        {
            if (
                token.Parameters == null ||
                token.Parameters.Count == 0
            )
            {
                return;
            }


            diagnostics.Add(
                $"Tag [{token.Name}] at token " +
                $"{tokenIndex} does not accept " +
                $"additional parameters."
            );
        }


        private static bool IsKnownTag(
            string name
        )
        {
            return
                IsScopeTag(name) ||
                IsOneShotTag(name);
        }


        private static bool IsScopeTag(
            string name
        )
        {
            return
                name == "effect" ||
                name == "style" ||
                name == "format" ||
                name == "speed" ||
                name == "instant";
        }


        private static bool IsOneShotTag(
            string name
        )
        {
            return
                name == "pause" ||
                name == "wait" ||
                name == "event" ||
                name == "page";
        }
    }
}
