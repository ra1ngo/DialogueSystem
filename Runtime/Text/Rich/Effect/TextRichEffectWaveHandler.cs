namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class TextRichEffectWaveHandler
        : TextRichEffectHandler<TextRichEffectWave>
    {
        public override string Name =>
            "wave";

        protected override TextRichEffectWave ParseEffect(
            IReadOnlyDictionary<string, string> parameters
        )
        {
            return new TextRichEffectWave(
                GetFloat(parameters, "speed", 6f),
                GetFloat(parameters, "strength", 3f),
                GetFloat(parameters, "frequency", 0.6f)
            );
        }

        protected override void ApplyEffect(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            TextRichEffectWave effect
        )
        {
            float offsetY =
                Mathf.Sin(
                    time * effect.Speed +
                    glyphIndex * effect.Frequency
                )
                * effect.Strength;

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
