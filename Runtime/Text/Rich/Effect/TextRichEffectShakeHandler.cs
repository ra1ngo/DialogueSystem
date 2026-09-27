namespace DialogueSystem.Text
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class TextRichEffectShakeHandler
        : TextRichEffectHandler<TextRichEffectShake>
    {
        public override string Name =>
            "shake";

        protected override TextRichEffectShake ParseEffect(
            IReadOnlyDictionary<string, string> parameters
        )
        {
            return new TextRichEffectShake(
                GetFloat(parameters, "speed", 6f),
                GetFloat(parameters, "strength", 3f)
            );
        }

        protected override void ApplyEffect(
            TextElement.Glyph glyph,
            int glyphIndex,
            float time,
            TextRichEffectShake effect
        )
        {
            float x =
                Mathf.PerlinNoise(
                    glyphIndex * 13.37f,
                    time * effect.Speed
                )
                * 2f - 1f;

            float y =
                Mathf.PerlinNoise(
                    glyphIndex * 27.91f,
                    time * effect.Speed + 100f
                )
                * 2f - 1f;

            Vector2 offset =
                new Vector2(x, y)
                * effect.Strength;

            var vertices = glyph.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                vertex.position.x += offset.x;
                vertex.position.y += offset.y;

                vertices[i] = vertex;
            }
        }
    }
}
