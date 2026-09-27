namespace DialogueSystem.Text
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class TextTypewriter
    {
        private readonly MonoBehaviour coroutineOwner;
        private readonly TextRichRenderer renderer;
        private readonly TextTypewriterCadence cadence;

        private readonly float defaultCharactersPerSecond;

        private IReadOnlyList<TextTypewriterToken>
            tokens;

        private Coroutine coroutine;

        private int currentTokenIndex;
        private int currentCharacterIndex;
        private int visibleCharacters;

        private float currentSpeed;

        private readonly Stack<float>
            speedStack = new();

        private int instantDepth;


        public TextTypewriterState State
        {
            get;
            private set;
        } = TextTypewriterState.Idle;


        public bool IsTyping =>
            State == TextTypewriterState.Typing;

        public bool IsWaiting =>
            State == TextTypewriterState.Waiting;


        public event Action<string>
            EventTriggered;

        public event Action<TextTypewriterState>
            StateChanged;


        public TextTypewriter(
            MonoBehaviour coroutineOwner,
            TextRichRenderer renderer,
            float defaultCharactersPerSecond,
            TextTypewriterCadence cadence
        )
        {
            this.coroutineOwner =
                coroutineOwner;

            this.renderer =
                renderer;

            this.defaultCharactersPerSecond =
                defaultCharactersPerSecond;

            this.cadence =
                cadence;

            currentSpeed =
                defaultCharactersPerSecond;
        }


        private void SetState(
            TextTypewriterState state
        )
        {
            if (State == state)
                return;

            State =
                state;

            StateChanged?.Invoke(
                state
            );
        }


        public void Start(
            IReadOnlyList<TextTypewriterToken> newTokens
        )
        {
            Stop();

            tokens = newTokens;

            ResetRuntimeState();

            renderer.RenderVisible(0);

            SetState(
                TextTypewriterState.Typing
            );

            coroutine =
                coroutineOwner.StartCoroutine(
                    TypeFromCurrentPosition()
                );
        }


        public void Complete()
        {
            if (
                State !=
                TextTypewriterState.Typing
            )
            {
                return;
            }

            StopCoroutine();

            for (
                int i = currentTokenIndex;
                i < tokens.Count;
                i++
            )
            {
                TextTypewriterToken token =
                    tokens[i];

                if (token is WaitTextCommand)
                {
                    currentTokenIndex = i;
                    currentCharacterIndex = 0;

                    renderer.RenderVisible(
                        visibleCharacters
                    );

                    SetState(
                        TextTypewriterState.Waiting
                    );

                    return;
                }

                if (
                    token is TextTypewriterTextToken
                        textToken
                )
                {
                    if (i == currentTokenIndex)
                    {
                        visibleCharacters +=
                            textToken.Text.Length -
                            currentCharacterIndex;
                    }
                    else
                    {
                        visibleCharacters +=
                            textToken.Text.Length;
                    }

                    continue;
                }

                ApplyCommandWhileSkipping(
                    token
                );
            }

            currentTokenIndex =
                tokens.Count;

            currentCharacterIndex = 0;

            renderer.RenderVisible(
                visibleCharacters
            );

            SetState(
                TextTypewriterState.Completed
            );
        }


        public void Continue()
        {
            if (
                State !=
                TextTypewriterState.Waiting
            )
            {
                return;
            }

            currentTokenIndex++;
            currentCharacterIndex = 0;

            SetState(
                TextTypewriterState.Typing
            );

            coroutine =
                coroutineOwner.StartCoroutine(
                    TypeFromCurrentPosition()
                );
        }


        public void Stop()
        {
            StopCoroutine();

            tokens = null;

            ResetRuntimeState();

            SetState(
                TextTypewriterState.Idle
            );
        }


        private IEnumerator TypeFromCurrentPosition()
        {
            while (
                currentTokenIndex <
                tokens.Count
            )
            {
                TextTypewriterToken token =
                    tokens[currentTokenIndex];


                if (
                    token is TextTypewriterTextToken
                        textToken
                )
                {
                    yield return TypeTextToken(
                        textToken
                    );

                    currentTokenIndex++;
                    currentCharacterIndex = 0;

                    continue;
                }


                if (
                    token is PauseTextCommand pause
                )
                {
                    currentTokenIndex++;

                    if (pause.Duration > 0f)
                    {
                        yield return
                            new WaitForSecondsRealtime(
                                pause.Duration
                            );
                    }

                    continue;
                }


                if (token is WaitTextCommand)
                {
                    SetState(
                        TextTypewriterState.Waiting
                    );

                    coroutine = null;

                    yield break;
                }


                ApplyCommand(
                    token
                );

                currentTokenIndex++;
            }


            coroutine = null;

            SetState(
                TextTypewriterState.Completed
            );
        }

        //private void ShowNextCharacter(
        //    TextTypewriterTextToken token
        //)
        //{
        //    if (
        //        currentCharacterIndex >=
        //        token.Text.Length
        //    )
        //    {
        //        return;
        //    }


        //    currentCharacterIndex++;
        //    visibleCharacters++;


        //    renderer.RenderVisible(
        //        visibleCharacters
        //    );
        //}
        private void ShowNextCharacter(
            TextTypewriterTextToken token
        )
        {
            currentCharacterIndex++;
            visibleCharacters++;


            renderer.RenderVisible(
                visibleCharacters
            );
        }

        private IEnumerator TypeTextToken(
            TextTypewriterTextToken token
        )
        {
            if (instantDepth > 0)
            {
                visibleCharacters +=
                    token.Text.Length -
                    currentCharacterIndex;

                currentCharacterIndex =
                    token.Text.Length;

                renderer.RenderVisible(
                    visibleCharacters
                );

                yield break;
            }


            while (
                currentCharacterIndex <
                token.Text.Length
            )
            {
                int characterIndex =
                    currentCharacterIndex;


                ShowNextCharacter(
                    token
                );


                if (currentSpeed <= 0f)
                {
                    yield return null;

                    continue;
                }


                float multiplier =
                    cadence.GetDelayMultiplier(
                        token.Text,
                        characterIndex
                    );


                float delay =
                    (1f / currentSpeed) *
                    multiplier;


                if (delay <= 0f)
                    continue;


                float elapsed =
                    0f;


                while (elapsed < delay)
                {
                    elapsed +=
                        Time.unscaledDeltaTime;

                    yield return null;
                }
            }
        }

        private void ApplyCommand(
            TextTypewriterToken token
        )
        {
            switch (token)
            {
                case EventTextCommand textEvent:
                    EventTriggered?.Invoke(
                        textEvent.EventId
                    );
                    break;


                case PushSpeedTextCommand pushSpeed:
                    speedStack.Push(
                        currentSpeed
                    );

                    currentSpeed =
                        pushSpeed.Speed;
                    break;


                case PopSpeedTextCommand:
                    currentSpeed =
                        speedStack.Count > 0
                            ? speedStack.Pop()
                            : defaultCharactersPerSecond;
                    break;


                case PushInstantTextCommand:
                    instantDepth++;
                    break;


                case PopInstantTextCommand:
                    if (instantDepth > 0)
                        instantDepth--;
                    break;
            }
        }


        private void ApplyCommandWhileSkipping(
            TextTypewriterToken token
        )
        {
            /*
             * Pause intentionally skipped
             * during Complete().
             */
            if (token is PauseTextCommand)
                return;

            ApplyCommand(
                token
            );
        }


        private void ResetRuntimeState()
        {
            currentTokenIndex = 0;
            currentCharacterIndex = 0;
            visibleCharacters = 0;

            currentSpeed =
                defaultCharactersPerSecond;

            speedStack.Clear();

            instantDepth = 0;
        }


        private void StopCoroutine()
        {
            if (coroutine == null)
                return;

            coroutineOwner.StopCoroutine(
                coroutine
            );

            coroutine = null;
        }
    }
}
