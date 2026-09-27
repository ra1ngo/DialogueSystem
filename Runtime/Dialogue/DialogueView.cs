namespace DialogueSystem
{
    using DialogueSystem.Text;

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(PanelRenderer))]
    public class DialogueView : MonoBehaviour
    {
        [SerializeField]
        private float charactersPerSecond = 30f;

        private PanelRenderer panelRenderer;

        private VisualElement dialogueRoot;
        private Label dialogueSpeaker;
        private Label dialogueText;
        private Label dialogueContinue;

        private TextRichRenderer richRenderer;
        private TextTypewriter typewriter;

        //private readonly TextRichTokenParser
        //    richTokenParser =
        //        new(
        //            TextRichEffectRegistry.Default
        //        );
        private TextRichTokenParser
            richTokenParser;

        private string currentPresentationClass;

        public bool IsBootstrapped { get; private set; }

        public event Action<string>
            TextEventTriggered;


        public bool IsTyping =>
            typewriter != null &&
            typewriter.IsTyping;

        public bool IsWaiting =>
            typewriter != null &&
            typewriter.IsWaiting;

        private VisualElement textViewport;
        private Label measureText;
        private TextRichRenderer measureRenderer;

        [SerializeField]
        private TextTypewriterTimingSettings typewriterTimingSettings;

        private readonly TextRichEffectRegistry
        effectRegistry =
            TextRichEffectRegistry.Default;

        private void Awake()
        {
            if (typewriterTimingSettings == null)
            {
                throw new InvalidOperationException(
                    "DialogueView requires " +
                    "TextTypewriterTimingSettings."
                );
            }

            richTokenParser =
                new TextRichTokenParser(
                    effectRegistry
                );
        }

        private void OnEnable()
        {
            panelRenderer =
                GetComponent<PanelRenderer>();

            panelRenderer.RegisterUIReloadCallback(
                OnUIReload
            );
        }

        private void OnDisable()
        {
            UnregisterGeometryCallback();

            IsBootstrapped = false;

            if (panelRenderer != null)
            {
                panelRenderer.UnregisterUIReloadCallback(
                    OnUIReload
                );
            }

            DisposeTextSystems();
        }

        private void Update()
        {
            richRenderer?.UpdateEffects(
                Time.unscaledTime
            );
        }

        private void OnUIReload(
        PanelRenderer renderer,
        VisualElement root
    )
        {
            IsBootstrapped = false;


            UnregisterGeometryCallback();

            DisposeTextSystems();


            QueryElements(
                root
            );

            CreateTextSystems();


            Hide();


            textViewport.RegisterCallback<
                GeometryChangedEvent
            >(
                OnTextViewportGeometryChanged
            );


            TryMarkBootstrapped(
                textViewport.contentRect.width,
                textViewport.contentRect.height
            );
        }

        private void QueryElements(
            VisualElement root
        )
        {
            dialogueRoot =
                RequireElement<VisualElement>(
                    root,
                    "dialogue-root"
                );

            dialogueSpeaker =
                RequireElement<Label>(
                    root,
                    "dialogue-speaker"
                );

            dialogueText =
                RequireElement<Label>(
                    root,
                    "dialogue-text"
                );

            dialogueContinue =
                RequireElement<Label>(
                    root,
                    "dialogue-continue"
                );

            textViewport =
                RequireElement<VisualElement>(
                    root,
                    "dialogue-text-viewport"
                );

            measureText =
                RequireElement<Label>(
                    root,
                    "dialogue-measure-text"
                );
        }

        private static T RequireElement<T>(
            VisualElement root,
            string name
        )
        where T : VisualElement
        {
            T element =
                root.Q<T>(
                    name
                );

            if (element != null)
                return element;

            throw new InvalidOperationException(
                $"DialogueView requires " +
                $"'{name}' ({typeof(T).Name}) " +
                $"in its visual tree."
            );
        }

        private void UnregisterGeometryCallback()
        {
            if (textViewport == null)
                return;

            textViewport.UnregisterCallback<
                GeometryChangedEvent
            >(
                OnTextViewportGeometryChanged
            );
        }

        private void CreateTextSystems()
        {
            richRenderer =
                new TextRichRenderer(
                    dialogueText,
                    effectRegistry
                );

            measureRenderer =
                new TextRichRenderer(
                    measureText,
                    effectRegistry
                );


            TextTypewriterCadence cadence =
                new TextTypewriterCadence(
                    typewriterTimingSettings
                );


            typewriter =
                new TextTypewriter(
                    this,
                    richRenderer,
                    charactersPerSecond,
                    cadence
                );


            typewriter.EventTriggered +=
                OnTextEventTriggered;

            typewriter.StateChanged +=
                OnTypewriterStateChanged;
        }

        private void OnTextViewportGeometryChanged(
            GeometryChangedEvent evt
        )
        {
            TryMarkBootstrapped(
                evt.newRect.width,
                evt.newRect.height
            );
        }

        private void TryMarkBootstrapped(
            float width,
            float height
        )
        {
            if (IsBootstrapped)
                return;

            if (
                !IsValidSize(width) ||
                !IsValidSize(height)
            )
            {
                return;
            }

            IsBootstrapped = true;

            UnregisterGeometryCallback();
        }

        public bool CanFitText(
            string source
        )
        {
            if (
                textViewport == null ||
                dialogueText == null ||
                measureText == null ||
                measureRenderer == null
            )
            {
                return false;
            }

            IReadOnlyList<TextToken> tokens =
                TextTokenParser.Parse(
                    source
                );

            IReadOnlyList<TextRichToken> richTokens =
                richTokenParser.Parse(
                    tokens
                );

            measureRenderer.SetTokens(
                richTokens
            );

            measureRenderer.RenderFull();


            float width =
                textViewport.contentRect.width;

            float maxHeight =
                textViewport.contentRect.height;


            //if (
            //    float.IsNaN(width) ||
            //    float.IsNaN(maxHeight) ||
            //    float.IsInfinity(width) ||
            //    float.IsInfinity(maxHeight) ||
            //    width <= 0f ||
            //    maxHeight <= 0f
            //)
            //{
            //    return false;
            //}
            if (
                !IsValidSize(width) ||
                !IsValidSize(maxHeight)
            )
            {
                return false;
            }


            Vector2 measuredSize =
                measureText.MeasureTextSize(
                    measureText.text,
                    width,
                    VisualElement.MeasureMode.Exactly,
                    0f,
                    VisualElement.MeasureMode.Undefined
                );

            return measuredSize.y <=
                   maxHeight;
        }


        public void Show()
        {
            if (dialogueRoot == null)
                return;

            dialogueRoot.style.visibility = Visibility.Visible;
        }

        public void Hide()
        {
            typewriter?.Stop();

            SetContinueVisible(
                false
            );

            if (dialogueRoot == null)
                return;

            dialogueRoot.style.visibility = Visibility.Hidden;
        }

        public void ApplyPresentation(
            DialoguePresentation presentation
        )
        {
            if (
                dialogueRoot == null ||
                presentation == null
            )
            {
                return;
            }

            if (
                !string.IsNullOrEmpty(
                    currentPresentationClass
                )
            )
            {
                dialogueRoot.RemoveFromClassList(
                    currentPresentationClass
                );
            }

            currentPresentationClass =
                presentation.StyleClass;

            if (
                !string.IsNullOrEmpty(
                    currentPresentationClass
                )
            )
            {
                dialogueRoot.AddToClassList(
                    currentPresentationClass
                );
            }
        }

        public void SetSpeaker(
            string speaker,
            bool visible
        )
        {
            if (dialogueSpeaker == null)
                return;

            bool show =
                visible &&
                !string.IsNullOrWhiteSpace(
                    speaker
                );

            dialogueSpeaker.style.display =
                show
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            dialogueSpeaker.text =
                show
                    ? speaker
                    : string.Empty;
        }

        public void ShowText(
            string source
        )
        {
            IReadOnlyList<TextToken> tokens =
                TextTokenParser.Parse(source);

            IReadOnlyList<TextRichToken> richTokens =
                richTokenParser.Parse(tokens);

            IReadOnlyList<TextTypewriterToken>
                typewriterTokens =
                    TextTypewriterTokenParser.Parse(
                        tokens
                    );

            richRenderer.SetTokens(
                richTokens
            );

            typewriter.Start(
                typewriterTokens
            );
        }

        //public void ShowTextInstant(
        //    string source
        //)
        //{
        //    typewriter.Stop();

        //    IReadOnlyList<TextToken> tokens =
        //        TextTokenParser.Parse(source);

        //    IReadOnlyList<TextRichToken> richTokens =
        //        richTokenParser.Parse(tokens);

        //    richRenderer.SetTokens(
        //        richTokens
        //    );

        //    richRenderer.RenderFull();
        //}

        public void ShowTextInstant(
            string source
        )
        {
            typewriter.Stop();


            IReadOnlyList<TextToken> tokens =
                TextTokenParser.Parse(
                    source
                );

            IReadOnlyList<TextRichToken> richTokens =
                richTokenParser.Parse(
                    tokens
                );


            richRenderer.SetTokens(
                richTokens
            );

            richRenderer.RenderFull();


            SetContinueVisible(
                true
            );
        }

        public void CompleteText()
        {
            typewriter?.Complete();
        }

        public void ContinueText()
        {
            typewriter?.Continue();
        }

        private void OnTextEventTriggered(
            string eventId
        )
        {
            TextEventTriggered?.Invoke(
                eventId
            );
        }

        private void OnTypewriterStateChanged(
            TextTypewriterState state
        )
        {
            bool requiresSubmit =
                state ==
                    TextTypewriterState.Waiting
                ||
                state ==
                    TextTypewriterState.Completed;


            SetContinueVisible(
                requiresSubmit
            );
        }

        private void SetContinueVisible(
            bool visible
        )
        {
            if (dialogueContinue == null)
                return;

            dialogueContinue.style.display =
                visible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        private void DisposeTextSystems()
        {
            if (typewriter != null)
            {
                typewriter.EventTriggered -=
                    OnTextEventTriggered;

                typewriter.StateChanged -=
                    OnTypewriterStateChanged;

                typewriter.Stop();

                typewriter = null;
            }

            if (richRenderer != null)
            {
                richRenderer.Dispose();

                richRenderer = null;
            }

            if (measureRenderer != null)
            {
                measureRenderer.Dispose();

                measureRenderer = null;
            }
        }

        private static bool IsValidSize(
            float value
        )
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value > 0f;
        }
    }
}
