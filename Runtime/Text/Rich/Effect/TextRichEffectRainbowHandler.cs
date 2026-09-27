namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class TextRichEffectRainbowHandler
        : TextRichEffectHandler<TextRichEffectRainbow>
    {
        public override string Name =>
            "rainbow";

        protected override TextRichEffectRainbow ParseEffect(
            IReadOnlyDictionary<string, string> parameters
        )
        {
            return new TextRichEffectRainbow(
                GetFloat(parameters, "speed", 0.3f),
                GetFloat(parameters, "frequency", 0.08f),
                GetFloat(parameters, "saturation", 1f),
                GetFloat(parameters, "brightness", 1f)
            );
        }

        protected override void ApplyEffect(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            TextRichEffectRainbow effect
        )
        {
            float hue =
                Mathf.Repeat(
                    time * effect.Speed +
                    glyphIndex * effect.Frequency,
                    1f
                );

            Color32 color =
                Color.HSVToRGB(
                    hue,
                    effect.Saturation,
                    effect.Brightness
                );

            var vertices = glyph.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                Color32 tint = vertex.tint;

                tint.r = color.r;
                tint.g = color.g;
                tint.b = color.b;

                vertex.tint = tint;

                vertices[i] = vertex;
            }
        }
    }
}
