namespace DialogueSystem
{
    public interface IDialogueSystem
    {
        void StartDialogue(Dialogue dialogue);
        void Submit();
        void Cancel();
        void EndDialogue();
    }
}
