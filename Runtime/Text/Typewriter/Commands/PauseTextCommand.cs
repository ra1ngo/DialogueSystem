namespace DialogueSystem.Text
{
    public sealed class PauseTextCommand
        : TextTypewriterCommand
    {
        public float Duration { get; }

        public PauseTextCommand(
            float duration
        )
        {
            Duration = duration;
        }
    }
}
