namespace DialogueSystem.Text
{
    public sealed class EventTextCommand
        : TextTypewriterCommand
    {
        public string EventId { get; }

        public EventTextCommand(
            string eventId
        )
        {
            EventId = eventId;
        }
    }
}
