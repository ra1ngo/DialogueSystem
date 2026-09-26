namespace DialogueSystem.Text
{
    public sealed class TextRichEffectWave
        : TextRichEffect
    {
        public float Speed { get; }
        public float Strength { get; }
        public float Frequency { get; }

        public TextRichEffectWave(
            float speed,
            float strength,
            float frequency
        )
        {
            Speed = speed;
            Strength = strength;
            Frequency = frequency;
        }
    }
}
