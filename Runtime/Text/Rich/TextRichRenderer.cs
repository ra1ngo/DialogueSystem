namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System;

    public sealed class TextRichRenderer
    {
        private readonly TextElement textElement;

        private readonly TextRichEffectRegistry
            effectRegistry;

        private IReadOnlyList<TextRichToken>
            tokens;

        private readonly Dictionary<
            string,
            IReadOnlyList<TextRichEffect>
        > effectsById = new();

        private readonly Dictionary<
            string,
            int
        > localGlyphIndices = new();

        private float time;

        public TextRichRenderer(
            TextElement textElement,
            TextRichEffectRegistry effectRegistry
        )
        {
            this.textElement =
                textElement ??
                throw new ArgumentNullException(
                    nameof(textElement)
                );

            this.effectRegistry =
                effectRegistry ??
                throw new ArgumentNullException(
                    nameof(effectRegistry)
                );

            textElement.enableRichText = true;

            textElement.PostProcessTextVertices +=
                OnPostProcessTextVertices;
        }

        public void SetTokens(
            IReadOnlyList<TextRichToken> newTokens
        )
        {
            tokens = newTokens;

            effectsById.Clear();

            foreach (TextRichToken token in tokens)
            {
                if (!token.HasEffects)
                    continue;

                effectsById[token.EffectId] =
                    token.Effects;
            }
        }

        public void RenderFull()
        {
            RenderVisible(
                int.MaxValue
            );
        }


        public void RenderVisible(
            int visibleCharacters
        )
        {
            if (tokens == null)
            {
                textElement.text =
                    string.Empty;

                return;
            }

            textElement.text =
                BuildText(
                    visibleCharacters
                );
        }

        public void Clear()
        {
            textElement.text =
                string.Empty;
        }

        public void UpdateEffects(float newTime)
        {
            time = newTime;

            if (effectsById.Count == 0)
                return;

            textElement.MarkDirtyRepaint();
        }

        public void Dispose()
        {
            textElement.PostProcessTextVertices -=
                OnPostProcessTextVertices;

            effectsById.Clear();
            localGlyphIndices.Clear();
            tokens = null;
        }

        //private string BuildText(
        //    int visibleCharacters
        //)
        //{
        //    var builder =
        //        new StringBuilder();

        //    int remaining =
        //        visibleCharacters;

        //    foreach (TextRichToken token in tokens)
        //    {
        //        if (remaining <= 0)
        //            break;

        //        int take =
        //            Mathf.Min(
        //                remaining,
        //                token.Text.Length
        //            );

        //        string text =
        //            token.Text.Substring(
        //                0,
        //                take
        //            );

        //        AppendToken(
        //            builder,
        //            token,
        //            text
        //        );

        //        remaining -= take;
        //    }

        //    return builder.ToString();
        //}

        private string BuildText(
            int visibleCharacters
        )
        {
            var builder =
                new StringBuilder();

            int remaining =
                visibleCharacters;

            foreach (
                TextRichToken token
                in tokens
            )
            {
                if (remaining <= 0)
                    break;

                int take =
                    Mathf.Min(
                        remaining,
                        token.Text.Length
                    );

                AppendToken(
                    builder,
                    token,
                    take
                );

                remaining -=
                    take;
            }

            return builder.ToString();
        }

        //private static void AppendToken(
        //    StringBuilder builder,
        //    TextRichToken token,
        //    string text
        //)
        //{
        //    if (token.HasEffects)
        //    {
        //        builder.Append(
        //            $"<link=\"{token.EffectId}\">"
        //        );
        //    }

        //    foreach (
        //        TextRichStyle style
        //        in token.Styles
        //    )
        //    {
        //        builder.Append(
        //            $"<style=\"{style.Name}\">"
        //        );
        //    }

        //    foreach (
        //        TextRichFormat format
        //        in token.Formats
        //    )
        //    {
        //        builder.Append(
        //            UnityRichTextFormatConverter
        //                .GetOpenTag(format)
        //        );
        //    }

        //    builder.Append(text);

        //    for (
        //        int i = token.Formats.Count - 1;
        //        i >= 0;
        //        i--
        //    )
        //    {
        //        builder.Append(
        //            UnityRichTextFormatConverter
        //                .GetCloseTag(
        //                    token.Formats[i]
        //                )
        //        );
        //    }

        //    for (
        //        int i = token.Styles.Count - 1;
        //        i >= 0;
        //        i--
        //    )
        //    {
        //        builder.Append("</style>");
        //    }

        //    if (token.HasEffects)
        //    {
        //        builder.Append("</link>");
        //    }
        //}
        private static void AppendToken(
        StringBuilder builder,
        TextRichToken token,
        int visibleLength
    )
        {
            if (token.HasEffects)
            {
                builder.Append(
                    $"<link=\"{token.EffectId}\">"
                );
            }

            foreach (
                TextRichStyle style
                in token.Styles
            )
            {
                builder.Append(
                    $"<style=\"{style.Name}\">"
                );
            }

            foreach (
                TextRichFormat format
                in token.Formats
            )
            {
                builder.Append(
                    UnityRichTextFormatConverter
                        .GetOpenTag(format)
                );
            }

            builder.Append(
                token.Text,
                0,
                visibleLength
            );

            for (
                int i = token.Formats.Count - 1;
                i >= 0;
                i--
            )
            {
                builder.Append(
                    UnityRichTextFormatConverter
                        .GetCloseTag(
                            token.Formats[i]
                        )
                );
            }

            for (
                int i = token.Styles.Count - 1;
                i >= 0;
                i--
            )
            {
                builder.Append(
                    "</style>"
                );
            }

            if (token.HasEffects)
            {
                builder.Append(
                    "</link>"
                );
            }
        }

        private void OnPostProcessTextVertices(
            TextElement.GlyphsEnumerable glyphs
        )
        {
            localGlyphIndices.Clear();

            foreach (
                TextElement.Glyph glyph
                in glyphs
            )
            {
                string linkId =
                    glyph.linkID;

                if (
                    string.IsNullOrEmpty(linkId)
                    ||
                    !effectsById.TryGetValue(
                        linkId,
                        out IReadOnlyList<TextRichEffect> effects
                    )
                )
                {
                    continue;
                }

                int localIndex =
                    GetNextLocalGlyphIndex(
                        linkId
                    );

                foreach (
                    TextRichEffect effect
                    in effects
                )
                {
                    if (
                        !effectRegistry.TryGetByEffect(
                            effect,
                            out ITextRichEffectHandler handler
                        )
                    )
                    {
                        continue;
                    }

                    handler.Apply(
                        glyph,
                        localIndex,
                        time,
                        effect
                    );
                }
            }
        }

        private int GetNextLocalGlyphIndex(
            string effectId
        )
        {
            if (
                !localGlyphIndices.TryGetValue(
                    effectId,
                    out int index
                )
            )
            {
                localGlyphIndices[
                    effectId
                ] = 1;

                return 0;
            }

            localGlyphIndices[
                effectId
            ] = index + 1;

            return index;
        }
    }
}
