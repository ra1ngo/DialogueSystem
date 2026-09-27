namespace DialogueSystem
{
    using DialogueSystem.Text;

    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class DialoguePaginator
    {
        /*
         * Теги-состояния.
         *
         * Если такой тег пересекает границу страницы,
         * мы закрываем его на старой странице
         * и снова открываем на новой.
         */
        private static readonly HashSet<string> ScopeTags =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "effect",
                "style",
                "format",

                "speed",
                "instant"
            };


        /*
         * Одноразовые команды.
         *
         * Они временно ожидают следующий
         * видимый фрагмент текста.
         */
        private static readonly HashSet<string> OneShotTags =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "pause",
                "wait",
                "event"
            };


        public static string[] Paginate(
            string source,
            Func<string, bool> canFit
        )
        {
            if (string.IsNullOrEmpty(source))
            {
                return new[]
                {
                    string.Empty
                };
            }

            string normalizedSource =
                TextWhitespaceNormalizer.Normalize(
                    source
                );


            IReadOnlyList<TextToken> tokens =
                TextTokenParser.Parse(
                    normalizedSource
                );


            List<string> pages =
                new();

            List<TextToken> activeScopes =
                new();

            List<TextToken> pendingCommands =
                new();

            StringBuilder pendingWhitespace =
                new();

            StringBuilder page =
                new();

            bool pageHasText =
                false;


            foreach (TextToken token in tokens)
            {
                /*
                 * Обычный текст.
                 *
                 * Один TextToken может содержать
                 * много слов и переводов строк,
                 * поэтому дополнительно режем его.
                 */
                if (token.Type == TextTokenType.Text)
                {
                    IReadOnlyList<string> textUnits =
                        SplitText(
                            token.Text
                        );

                    foreach (string textUnit in textUnits)
                    {
                        TryAppendText(
                            textUnit,
                            canFit,
                            pages,
                            page,
                            activeScopes,
                            pendingCommands,
                            pendingWhitespace,
                            ref pageHasText
                        );
                    }

                    continue;
                }


                /*
                 * Явный hard page break.
                 */
                if (
                    string.Equals(
                        token.Name,
                        "page",
                        StringComparison.OrdinalIgnoreCase
                    )
                    &&
                    !token.IsClosing
                )
                {
                    /*
                     * [wait][page] не должен
                     * требовать два Submit.
                     */
                    FlushCommandsBeforeHardPageBreak(
                        page,
                        pendingCommands
                    );


                    /*
                     * При ручном [page] сохраняем
                     * whitespace, заданный автором.
                     */
                    page.Append(
                        pendingWhitespace
                    );

                    pendingWhitespace.Clear();


                    AddPage(
                        pages,
                        page,
                        activeScopes,
                        force: true
                    );


                    StartNewPage(
                        page,
                        activeScopes
                    );


                    pageHasText =
                        false;

                    continue;
                }


                /*
                 * Scope:
                 *
                 * effect
                 * style
                 * format
                 * speed
                 * instant
                 */
                if (ScopeTags.Contains(token.Name))
                {
                    /*
                     * Pending-команды должны сохранить
                     * исходный порядок относительно тегов.
                     */
                    FlushPendingCommands(
                        page,
                        pendingCommands
                    );


                    if (pendingWhitespace.Length > 0)
                    {
                        page.Append(
                            pendingWhitespace
                        );

                        pendingWhitespace.Clear();
                    }


                    AppendToken(
                        page,
                        token
                    );


                    UpdateActiveScopes(
                        activeScopes,
                        token
                    );

                    continue;
                }


                /*
                 * pause / wait / event
                 *
                 * Пока не добавляем их в страницу.
                 *
                 * Они логически относятся
                 * к следующему тексту.
                 */
                if (
                    OneShotTags.Contains(token.Name)
                    &&
                    !token.IsClosing
                )
                {
                    pendingCommands.Add(
                        token
                    );

                    continue;
                }


                /*
                 * Неизвестный paginator'у тег.
                 *
                 * Не удаляем его — сохраняем DSL.
                 */
                FlushPendingCommands(
                    page,
                    pendingCommands
                );


                if (pendingWhitespace.Length > 0)
                {
                    page.Append(
                        pendingWhitespace
                    );

                    pendingWhitespace.Clear();
                }


                AppendToken(
                    page,
                    token
                );
            }


            /*
             * Команды в самом конце строки
             * тоже должны сохраниться.
             *
             * Например:
             *
             * текст[event=finished]
             */
            FlushPendingCommands(
                page,
                pendingCommands
            );


            if (pendingWhitespace.Length > 0)
            {
                page.Append(
                    pendingWhitespace
                );

                pendingWhitespace.Clear();
            }


            AddPage(
                pages,
                page,
                activeScopes,
                force: pages.Count == 0
            );


            if (pages.Count == 0)
            {
                pages.Add(
                    string.Empty
                );
            }


            return pages.ToArray();
        }


        private static void TryAppendText(
            string text,
            Func<string, bool> canFit,
            List<string> pages,
            StringBuilder page,
            List<TextToken> activeScopes,
            List<TextToken> pendingCommands,
            StringBuilder pendingWhitespace,
            ref bool pageHasText
        )
        {
            /*
             * Whitespace после pause/wait/event
             * пока не добавляем.
             *
             * Он должен перемещаться вместе
             * со следующим видимым текстом.
             */
            if (
                pendingCommands.Count > 0 &&
                string.IsNullOrWhiteSpace(text)
            )
            {
                pendingWhitespace.Append(
                    text
                );

                return;
            }


            /*
             * Собираем временную версию страницы:
             *
             * текущая страница
             * + pending commands
             * + pending whitespace
             * + новый текст
             * + closing scope tags.
             */
            StringBuilder candidate =
                new(page.ToString());


            AppendTokens(
                candidate,
                pendingCommands
            );


            candidate.Append(
                pendingWhitespace
            );


            candidate.Append(
                text
            );


            AppendClosingScopes(
                candidate,
                activeScopes
            );


            bool fits =
                canFit(
                    candidate.ToString()
                );


            bool isLineBreak =
                IsLineBreak(
                    text
                );


            /*
             * Если слово помещается, но следующий
             * перевод строки уже нет —
             * page break сам заменяет этот newline.
             *
             * Благодаря этому новая страница
             * не начинается с пустой строки.
             */
            if (
                !fits &&
                isLineBreak &&
                pageHasText
            )
            {
                AddPage(
                    pages,
                    page,
                    activeScopes,
                    force: false
                );


                StartNewPage(
                    page,
                    activeScopes
                );


                /*
                 * Сам page flip уже является
                 * ожиданием Submit.
                 */
                RemoveWaitCommands(
                    pendingCommands
                );


                pendingCommands.Clear();
                pendingWhitespace.Clear();


                pageHasText =
                    false;

                return;
            }


            /*
             * Если помещается — просто принимаем.
             *
             * Если страница ещё не содержит
             * видимого текста, тоже принимаем,
             * чтобы не получить бесконечный цикл
             * на одном огромном слове.
             */
            if (
                fits ||
                !pageHasText
            )
            {
                FlushPendingCommands(
                    page,
                    pendingCommands
                );


                if (pendingWhitespace.Length > 0)
                {
                    page.Append(
                        pendingWhitespace
                    );

                    pendingWhitespace.Clear();
                }


                page.Append(
                    text
                );


                if (!string.IsNullOrWhiteSpace(text))
                {
                    pageHasText =
                        true;
                }

                return;
            }


            /*
             * Новый текст уже не помещается.
             *
             * Pending commands пока НЕ добавлены
             * к старой странице, поэтому можем
             * перенести их правильно.
             */
            AddPage(
                pages,
                page,
                activeScopes,
                force: false
            );


            StartNewPage(
                page,
                activeScopes
            );


            /*
             * [wait] на автоматической границе
             * страницы не нужен.
             *
             * Иначе:
             *
             * Submit → новая страница
             * Submit → пройти wait
             */
            RemoveWaitCommands(
                pendingCommands
            );


            /*
             * Не начинаем автоматически созданную
             * страницу с whitespace, который был
             * между командой и следующим словом.
             */
            pendingWhitespace.Clear();


            /*
             * pause / event при этом сохраняются
             * и выполняются уже на новой странице.
             */
            FlushPendingCommands(
                page,
                pendingCommands
            );


            page.Append(
                text
            );


            pageHasText =
                !string.IsNullOrWhiteSpace(
                    text
                );
        }


        private static void AddPage(
            List<string> pages,
            StringBuilder page,
            List<TextToken> activeScopes,
            bool force
        )
        {
            if (
                !force &&
                !ContainsVisibleText(page)
            )
            {
                return;
            }


            StringBuilder result =
                new(page.ToString());


            /*
             * Все открытые scopes временно
             * закрываем в конце страницы.
             */
            AppendClosingScopes(
                result,
                activeScopes
            );


            pages.Add(
                result.ToString()
            );
        }


        private static void StartNewPage(
            StringBuilder page,
            List<TextToken> activeScopes
        )
        {
            page.Clear();


            /*
             * Восстанавливаем все scope states
             * на новой странице.
             *
             * Например:
             *
             * [style=warning]
             * [effect=wave]
             * [speed=0.02]
             */
            foreach (
                TextToken scope
                in activeScopes
            )
            {
                AppendToken(
                    page,
                    scope
                );
            }
        }


        private static void UpdateActiveScopes(
            List<TextToken> activeScopes,
            TextToken token
        )
        {
            if (!token.IsClosing)
            {
                activeScopes.Add(
                    token
                );

                return;
            }


            /*
             * Закрываем последний scope
             * с таким же именем.
             *
             * Это позволяет корректно работать
             * с вложенными тегами.
             */
            for (
                int i = activeScopes.Count - 1;
                i >= 0;
                i--
            )
            {
                if (
                    !string.Equals(
                        activeScopes[i].Name,
                        token.Name,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                activeScopes.RemoveAt(i);

                return;
            }
        }


        private static void AppendClosingScopes(
            StringBuilder builder,
            List<TextToken> activeScopes
        )
        {
            for (
                int i = activeScopes.Count - 1;
                i >= 0;
                i--
            )
            {
                builder.Append(
                    "[/"
                );

                builder.Append(
                    activeScopes[i].Name
                );

                builder.Append(
                    ']'
                );
            }
        }


        private static void FlushPendingCommands(
            StringBuilder page,
            List<TextToken> pendingCommands
        )
        {
            AppendTokens(
                page,
                pendingCommands
            );

            pendingCommands.Clear();
        }


        private static void FlushCommandsBeforeHardPageBreak(
            StringBuilder page,
            List<TextToken> pendingCommands
        )
        {
            /*
             * [wait][page]
             *
             * page break уже требует Submit,
             * поэтому wait здесь лишний.
             */
            RemoveWaitCommands(
                pendingCommands
            );


            FlushPendingCommands(
                page,
                pendingCommands
            );
        }


        private static void RemoveWaitCommands(
            List<TextToken> commands
        )
        {
            for (
                int i = commands.Count - 1;
                i >= 0;
                i--
            )
            {
                if (
                    string.Equals(
                        commands[i].Name,
                        "wait",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    commands.RemoveAt(i);
                }
            }
        }


        private static void AppendTokens(
            StringBuilder builder,
            IReadOnlyList<TextToken> tokens
        )
        {
            foreach (TextToken token in tokens)
            {
                AppendToken(
                    builder,
                    token
                );
            }
        }


        /*
         * Обратно собираем TextToken
         * в наш DSL.
         */
        private static void AppendToken(
            StringBuilder builder,
            TextToken token
        )
        {
            if (token.Type == TextTokenType.Text)
            {
                builder.Append(
                    token.Text
                );

                return;
            }


            builder.Append(
                '['
            );


            if (token.IsClosing)
            {
                builder.Append(
                    '/'
                );
            }


            builder.Append(
                token.Name
            );


            if (
                !token.IsClosing &&
                !string.IsNullOrEmpty(
                    token.Value
                )
            )
            {
                builder.Append(
                    '='
                );

                builder.Append(
                    token.Value
                );
            }


            if (
                !token.IsClosing &&
                token.Parameters != null
            )
            {
                foreach (
                    KeyValuePair<string, string> parameter
                    in token.Parameters
                )
                {
                    builder.Append(
                        ' '
                    );

                    builder.Append(
                        parameter.Key
                    );

                    builder.Append(
                        '='
                    );

                    builder.Append(
                        parameter.Value
                    );
                }
            }


            builder.Append(
                ']'
            );
        }


        /*
         * Делим TextToken на более мелкие части.
         *
         * Важно:
         *
         * "длинный текст...\n"
         *
         * превращается в:
         *
         * "длинный "
         * "текст..."
         * "\n"
         *
         * А не:
         *
         * "текст...\n"
         *
         * Иначе newline может преждевременно
         * вытолкнуть последнее слово
         * на следующую страницу.
         */
        private static IReadOnlyList<string> SplitText(
            string text
        )
        {
            List<string> result =
                new();


            if (string.IsNullOrEmpty(text))
                return result;


            int index =
                0;


            while (index < text.Length)
            {
                /*
                 * Перевод строки —
                 * всегда отдельный unit.
                 */
                if (
                    text[index] == '\r' ||
                    text[index] == '\n'
                )
                {
                    if (
                        text[index] == '\r' &&
                        index + 1 < text.Length &&
                        text[index + 1] == '\n'
                    )
                    {
                        result.Add(
                            "\r\n"
                        );

                        index += 2;
                    }
                    else
                    {
                        result.Add(
                            text[index].ToString()
                        );

                        index++;
                    }

                    continue;
                }


                /*
                 * Горизонтальный whitespace.
                 *
                 * Обычно приклеиваем его
                 * к предыдущему слову.
                 */
                if (char.IsWhiteSpace(text[index]))
                {
                    int start =
                        index;


                    while (
                        index < text.Length &&
                        char.IsWhiteSpace(text[index]) &&
                        text[index] != '\r' &&
                        text[index] != '\n'
                    )
                    {
                        index++;
                    }


                    string whitespace =
                        text.Substring(
                            start,
                            index - start
                        );


                    if (result.Count > 0)
                    {
                        result[
                            result.Count - 1
                        ] += whitespace;
                    }
                    else
                    {
                        result.Add(
                            whitespace
                        );
                    }

                    continue;
                }


                /*
                 * Обычное слово.
                 */
                int wordStart =
                    index;


                while (
                    index < text.Length &&
                    !char.IsWhiteSpace(text[index])
                )
                {
                    index++;
                }


                result.Add(
                    text.Substring(
                        wordStart,
                        index - wordStart
                    )
                );
            }


            return result;
        }


        private static bool IsLineBreak(
            string text
        )
        {
            return
                text == "\n" ||
                text == "\r" ||
                text == "\r\n";
        }


        /*
         * Нужно отличать страницу,
         * содержащую только DSL/whitespace,
         * от страницы с настоящим текстом.
         */
        private static bool ContainsVisibleText(
            StringBuilder page
        )
        {
            IReadOnlyList<TextToken> tokens =
                TextTokenParser.Parse(
                    page.ToString()
                );


            foreach (TextToken token in tokens)
            {
                if (
                    token.Type ==
                        TextTokenType.Text
                    &&
                    !string.IsNullOrWhiteSpace(
                        token.Text
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }
    }
}
