namespace DialogueSystem.Text
{
    using System;
    using System.Collections.Generic;

    public sealed class TextRichEffectRegistry
    {
        public static TextRichEffectRegistry Default
        {
            get;
        } = CreateDefault();

        private readonly Dictionary<
            string,
            ITextRichEffectHandler
        > handlersByName = new();

        private readonly Dictionary<
            Type,
            ITextRichEffectHandler
        > handlersByType = new();

        public void Register(
            ITextRichEffectHandler handler
        )
        {
            handlersByName[handler.Name] =
                handler;

            handlersByType[handler.EffectType] =
                handler;
        }

        public bool TryGetByName(
            string name,
            out ITextRichEffectHandler handler
        )
        {
            return handlersByName.TryGetValue(
                name,
                out handler
            );
        }

        public bool TryGetByEffect(
            TextRichEffect effect,
            out ITextRichEffectHandler handler
        )
        {
            return handlersByType.TryGetValue(
                effect.GetType(),
                out handler
            );
        }

        private static TextRichEffectRegistry
            CreateDefault()
        {
            var registry =
                new TextRichEffectRegistry();

            registry.Register(
                new TextRichEffectWaveHandler()
            );

            registry.Register(
                new TextRichEffectShakeHandler()
            );

            registry.Register(
                new TextRichEffectRainbowHandler()
            );

            registry.Register(
                new TextRichEffectBounceHandler()
            );

            return registry;
        }
    }
}
