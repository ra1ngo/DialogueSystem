namespace DialogueSystem.Text
{
    using System.Collections.Generic;

    public static class UnityRichTextFormatConverter
    {
        private static readonly HashSet<string>
            SupportedFormats = new()
            {
                "b",
                "i",
                "u",
                "s",

                "allcaps",
                "lowercase",
                "uppercase",
                "smallcaps",

                "alpha",
                "color",
                "font",
                "font-weight",
                "gradient",
                "mark",
                "mspace",
                "nobr",
                "noparse",
                "size",
                "sub",
                "sup",
                "voffset"
            };

        public static string GetOpenTag(
            TextRichFormat format
        )
        {
            if (
                !SupportedFormats.Contains(
                    format.Name
                )
            )
            {
                return string.Empty;
            }

            if (
                format.Parameters.TryGetValue(
                    "value",
                    out string value
                )
                &&
                !string.IsNullOrWhiteSpace(value)
            )
            {
                return
                    $"<{format.Name}={value}>";
            }

            return $"<{format.Name}>";
        }

        public static string GetCloseTag(
            TextRichFormat format
        )
        {
            if (
                !SupportedFormats.Contains(
                    format.Name
                )
            )
            {
                return string.Empty;
            }

            return $"</{format.Name}>";
        }
    }
}
