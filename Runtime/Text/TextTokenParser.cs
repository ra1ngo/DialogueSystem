namespace DialogueSystem.Text
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class TextTokenParser
    {
        public static IReadOnlyList<TextToken> Parse(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return Array.Empty<TextToken>();
            }

            var tokens = new List<TextToken>();
            var textBuffer = new StringBuilder();

            int index = 0;

            while (index < source.Length)
            {
                if (source[index] != '[')
                {
                    textBuffer.Append(source[index]);
                    index++;
                    continue;
                }

                int closingBracket =
                    source.IndexOf(']', index);

                if (closingBracket < 0)
                {
                    textBuffer.Append(source[index]);
                    index++;
                    continue;
                }

                string tagContent =
                    source.Substring(
                        index + 1,
                        closingBracket - index - 1
                    )
                    .Trim();

                if (string.IsNullOrEmpty(tagContent))
                {
                    textBuffer.Append(source[index]);
                    index++;
                    continue;
                }

                FlushText(
                    tokens,
                    textBuffer
                );

                tokens.Add(
                    ParseTag(tagContent)
                );

                index = closingBracket + 1;
            }

            FlushText(
                tokens,
                textBuffer
            );

            return tokens;
        }

        private static TextToken ParseTag(
            string content
        )
        {
            bool isClosing =
                content.StartsWith("/");

            if (isClosing)
            {
                string closingTagName =
                    content.Substring(1)
                        .Trim()
                        .ToLowerInvariant();

                return TextToken.CreateTag(
                    closingTagName,
                    null,
                    true,
                    new Dictionary<string, string>()
                );
            }

            string[] parts =
                content.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries
                );

            string mainPart =
                parts[0];

            string tagName;
            string tagValue = null;

            int equalsIndex =
                mainPart.IndexOf('=');

            if (equalsIndex >= 0)
            {
                tagName =
                    mainPart.Substring(
                        0,
                        equalsIndex
                    );

                tagValue =
                    mainPart.Substring(
                        equalsIndex + 1
                    );
            }
            else
            {
                tagName =
                    mainPart;
            }

            tagName =
                tagName.ToLowerInvariant();

            var parameters =
                new Dictionary<string, string>();

            for (int i = 1; i < parts.Length; i++)
            {
                string part =
                    parts[i];

                int parameterEquals =
                    part.IndexOf('=');

                if (parameterEquals <= 0)
                    continue;

                string parameterName =
                    part.Substring(
                        0,
                        parameterEquals
                    );

                string parameterValue =
                    part.Substring(
                        parameterEquals + 1
                    );

                parameters[
                    parameterName.ToLowerInvariant()
                ] = parameterValue;
            }

            return TextToken.CreateTag(
                tagName,
                tagValue,
                false,
                parameters
            );
        }

        private static void FlushText(
            List<TextToken> tokens,
            StringBuilder buffer
        )
        {
            if (buffer.Length == 0)
                return;

            tokens.Add(
                TextToken.CreateText(
                    buffer.ToString()
                )
            );

            buffer.Clear();
        }
    }
}
