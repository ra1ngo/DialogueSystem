namespace DialogueSystem
{
    /// <summary>Resolves dialogue text and speaker keys using the application's localization provider.</summary>
    public interface IDialogueLocalization
    {
        string Get(string key);
    }
}
