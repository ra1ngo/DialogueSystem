namespace DialogueSystem.Text
{
    public abstract class TextTypewriterToken
    {
    }

    public sealed class TextTypewriterTextToken
        : TextTypewriterToken
    {
        public string Text { get; }

        public TextTypewriterTextToken(
            string text
        )
        {
            Text = text;
        }
    }
}
