namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class TextRichToken
    {
        public int Id { get; }

        public string Text { get; }

        public IReadOnlyList<TextRichFormat> Formats { get; }
        public IReadOnlyList<TextRichStyle> Styles { get; }
        public IReadOnlyList<TextRichEffect> Effects { get; }

        public bool HasEffects =>
            Effects.Count > 0;

        public string EffectId =>
            $"fx:{Id}";

        public TextRichToken(
            int id,
            string text,
            IEnumerable<TextRichFormat> formats,
            IEnumerable<TextRichStyle> styles,
            IEnumerable<TextRichEffect> effects
        )
        {
            Id = id;
            Text = text;

            Formats = formats.ToArray();
            Styles = styles.ToArray();
            Effects = effects.ToArray();
        }
    }
}
