namespace DialogueSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class DialoguePresentationResolver
    {
        private static readonly DialoguePresentation
            BuiltInStandard =
                new DialoguePresentation(
                    null,
                    DialogueTypingMode.Typewriter,
                    true
                );


        private readonly Dictionary<
            string,
            DialoguePresentation
        > presentations =
            new();


        private readonly DialoguePresentation
            standard;


        public DialoguePresentationResolver(
            DialoguePresentationSettings settings
        )
        {
            if (settings == null)
            {
                throw new ArgumentNullException(
                    nameof(settings)
                );
            }


            foreach (
                DialoguePresentationDefinition definition
                in settings.Presentations
            )
            {
                if (definition == null)
                    continue;


                string id =
                    definition.Id?.Trim();


                if (
                    string.IsNullOrWhiteSpace(id)
                )
                {
                    continue;
                }


                /*
                 * Presentation без style считаем
                 * некорректным и не добавляем.
                 *
                 * Resolve() тогда естественно
                 * вернёт standard.
                 */
                if (
                    string.IsNullOrWhiteSpace(
                        definition.StyleClass
                    )
                )
                {
                    continue;
                }


                presentations[id] =
                    definition.CreatePresentation();
            }


            if (
                presentations.TryGetValue(
                    DialoguePresentationIds.Standard,
                    out DialoguePresentation
                        configuredStandard
                )
            )
            {
                standard =
                    configuredStandard;
            }
            else
            {
                standard =
                    BuiltInStandard;
            }
        }


        public DialoguePresentation Resolve(
            string id
        )
        {
            if (
                string.IsNullOrWhiteSpace(id)
            )
            {
                return standard;
            }


            string normalizedId =
                id.Trim();


            if (
                presentations.TryGetValue(
                    normalizedId,
                    out DialoguePresentation
                        presentation
                )
            )
            {
                return presentation;
            }


            return standard;
        }
    }
}
