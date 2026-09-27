namespace DialogueSystem.Text
{
    public sealed class TextRichEffectRainbow
        : TextRichEffect
    {
        public float Speed { get; }
        public float Frequency { get; }
        public float Saturation { get; }
        public float Brightness { get; }

        public TextRichEffectRainbow(
            float speed,
            float frequency,
            float saturation,
            float brightness
        )
        {
            Speed = speed;
            Frequency = frequency;
            Saturation = saturation;
            Brightness = brightness;
        }
    }
}
