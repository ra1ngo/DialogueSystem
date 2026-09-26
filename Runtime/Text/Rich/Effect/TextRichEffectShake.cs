namespace DialogueSystem.Text
{
    public sealed class TextRichEffectShake
        : TextRichEffect
    {
        public float Speed { get; }
        public float Strength { get; }

        public TextRichEffectShake(
            float speed,
            float strength
        )
        {
            Speed = speed;
            Strength = strength;
        }
    }
}
