namespace DialogueSystem.Text
{
    using System.Collections.Generic;

    public enum TextTokenType
    {
        Text,
        Tag
    }

    public sealed class TextToken
    {
        public TextTokenType Type { get; }

        public string Text { get; }

        public string Name { get; }
        public string Value { get; }

        public bool IsClosing { get; }

        public IReadOnlyDictionary<string, string> Parameters { get; }

        private TextToken(
            TextTokenType type,
            string text,
            string name,
            string value,
            bool isClosing,
            IReadOnlyDictionary<string, string> parameters
        )
        {
            Type = type;
            Text = text;
            Name = name;
            Value = value;
            IsClosing = isClosing;
            Parameters = parameters;
        }

        public static TextToken CreateText(string text)
        {
            return new TextToken(
                TextTokenType.Text,
                text,
                null,
                null,
                false,
                null
            );
        }

        public static TextToken CreateTag(
            string name,
            string value,
            bool isClosing,
            IReadOnlyDictionary<string, string> parameters
        )
        {
            return new TextToken(
                TextTokenType.Tag,
                null,
                name,
                value,
                isClosing,
                parameters
            );
        }
    }
}
