namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class TextRichFormat
    {
        public string Name { get; }

        public IReadOnlyDictionary<string, string>
            Parameters
        { get; }

        public TextRichFormat(
            string name,
            IReadOnlyDictionary<string, string> parameters
        )
        {
            Name = name;

            Parameters =
                parameters != null
                    ? new Dictionary<string, string>(
                        parameters
                    )
                    : new Dictionary<string, string>();
        }
    }
}
