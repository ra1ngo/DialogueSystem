namespace DialogueSystem.Text
{
    public sealed class TextRichEffectBounce
        : TextRichEffect
    {
        public float Speed { get; }
        public float Height { get; }
        public float Frequency { get; }

        public TextRichEffectBounce(
            float speed,
            float height,
            float frequency
        )
        {
            Speed = speed;
            Height = height;
            Frequency = frequency;
        }
    }
}
