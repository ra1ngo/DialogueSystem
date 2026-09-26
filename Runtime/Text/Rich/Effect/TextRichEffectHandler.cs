namespace DialogueSystem.Text
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine.UIElements;

    public abstract class TextRichEffectHandler<T>
        : ITextRichEffectHandler
        where T : TextRichEffect
    {
        public abstract string Name { get; }

        public Type EffectType =>
            typeof(T);

        public TextRichEffect Parse(
            IReadOnlyDictionary<string, string> parameters
        )
        {
            return ParseEffect(parameters);
        }

        public void Apply(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            TextRichEffect effect
        )
        {
            ApplyEffect(
                glyph,
                glyphIndex,
                time,
                (T)effect
            );
        }

        protected abstract T ParseEffect(
            IReadOnlyDictionary<string, string> parameters
        );

        protected abstract void ApplyEffect(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            T effect
        );

        protected static float GetFloat(
            IReadOnlyDictionary<string, string> parameters,
            string name,
            float defaultValue
        )
        {
            if (
                !parameters.TryGetValue(
                    name,
                    out string value
                )
            )
            {
                return defaultValue;
            }

            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float result
            )
                ? result
                : defaultValue;
        }
    }
}
