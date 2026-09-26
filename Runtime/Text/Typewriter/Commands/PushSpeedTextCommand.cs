namespace DialogueSystem.Text
{
    public sealed class PushSpeedTextCommand
        : TextTypewriterCommand
    {
        public float Speed { get; }

        public PushSpeedTextCommand(
            float speed
        )
        {
            Speed = speed;
        }
    }
}
