namespace DialogueSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "DialoguePresentations",
        menuName = "Dialogue/Presentation Settings"
    )]
    public sealed class DialoguePresentationSettings :
        ScriptableObject
    {
        [SerializeField]
        private DialoguePresentationDefinition[]
            presentations =
                Array.Empty<
                    DialoguePresentationDefinition
                >();


        public IReadOnlyList<
            DialoguePresentationDefinition
        > Presentations =>
            presentations;
    }
}
