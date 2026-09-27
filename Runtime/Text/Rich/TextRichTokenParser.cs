namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using System;

    public sealed class TextRichTokenParser
    {
        private readonly TextRichEffectRegistry
            effectRegistry;

        public TextRichTokenParser(
            TextRichEffectRegistry effectRegistry
        )
        {
            this.effectRegistry =
                effectRegistry ??
                throw new ArgumentNullException(
                    nameof(effectRegistry)
                );
        }

        public IReadOnlyList<TextRichToken> Parse(
            IReadOnlyList<TextToken> tokens
        )
        {
            var result =
                new List<TextRichToken>();

            var formats =
                new List<TextRichFormat>();

            var styles =
                new List<TextRichStyle>();

            var effects =
                new List<TextRichEffect>();

            int nextId = 0;

            foreach (TextToken token in tokens)
            {
                if (token.Type == TextTokenType.Text)
                {
                    result.Add(
                        new TextRichToken(
                            nextId++,
                            token.Text,
                            formats,
                            styles,
                            effects
                        )
                    );

                    continue;
                }

                if (token.IsClosing)
                {
                    CloseTag(
                        token,
                        formats,
                        styles,
                        effects
                    );

                    continue;
                }

                OpenTag(
                    token,
                    formats,
                    styles,
                    effects
                );
            }

            return result;
        }

        private void OpenTag(
            TextToken token,
            List<TextRichFormat> formats,
            List<TextRichStyle> styles,
            List<TextRichEffect> effects
        )
        {
            switch (token.Name)
            {
                case "format":
                    if (
                        !string.IsNullOrWhiteSpace(
                            token.Value
                        )
                    )
                    {
                        formats.Add(
                            new TextRichFormat(
                                token.Value,
                                token.Parameters
                            )
                        );
                    }

                    break;

                case "style":
                    if (
                        !string.IsNullOrWhiteSpace(
                            token.Value
                        )
                    )
                    {
                        styles.Add(
                            new TextRichStyle(
                                token.Value
                            )
                        );
                    }

                    break;

                case "effect":
                    if (
                        string.IsNullOrWhiteSpace(
                            token.Value
                        )
                    )
                    {
                        break;
                    }

                    if (
                        effectRegistry.TryGetByName(
                            token.Value,
                            out ITextRichEffectHandler handler
                        )
                    )
                    {
                        effects.Add(
                            handler.Parse(
                                token.Parameters
                            )
                        );
                    }

                    break;
            }
        }

        private static void CloseTag(
            TextToken token,
            List<TextRichFormat> formats,
            List<TextRichStyle> styles,
            List<TextRichEffect> effects
        )
        {
            switch (token.Name)
            {
                case "format":
                    RemoveLast(formats);
                    break;

                case "style":
                    RemoveLast(styles);
                    break;

                case "effect":
                    RemoveLast(effects);
                    break;
            }
        }

        private static void RemoveLast<T>(
            List<T> list
        )
        {
            if (list.Count == 0)
                return;

            list.RemoveAt(
                list.Count - 1
            );
        }
    }
}
