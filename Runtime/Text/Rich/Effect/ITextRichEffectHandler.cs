namespace DialogueSystem.Text
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    public interface ITextRichEffectHandler
    {
        string Name { get; }

        Type EffectType { get; }

        TextRichEffect Parse(
            IReadOnlyDictionary<string, string> parameters
        );

        void Apply(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            TextRichEffect effect
        );
    }
}
