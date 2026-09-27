namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class TextRichEffectBounceHandler
        : TextRichEffectHandler<TextRichEffectBounce>
    {
        public override string Name =>
            "bounce";

        protected override TextRichEffectBounce ParseEffect(
            IReadOnlyDictionary<string, string> parameters
        )
        {
            return new TextRichEffectBounce(
                GetFloat(parameters, "speed", 6f),
                GetFloat(parameters, "height", 4f),
                GetFloat(parameters, "frequency", 0.6f)
            );
        }

        protected override void ApplyEffect(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            TextRichEffectBounce effect
        )
        {
            float wave =
                Mathf.Sin(
                    time * effect.Speed +
                    glyphIndex * effect.Frequency
                );

            float offsetY =
                Mathf.Max(0f, wave)
                * effect.Height;

            var vertices = glyph.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                vertex.position.y += offsetY;

                vertices[i] = vertex;
            }
        }
    }
}
