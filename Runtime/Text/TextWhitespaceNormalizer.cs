namespace DialogueSystem.Text
{
    using System.Text;

    public static class TextWhitespaceNormalizer
    {
        public static string Normalize(
            string source
        )
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;


            StringBuilder result =
                new StringBuilder(
                    source.Length
                );


            bool pendingSpace = false;
            bool atLineStart = true;
            bool lastVisibleWasSpace = false;


            int index = 0;

            while (index < source.Length)
            {
                char character =
                    source[index];


                /*
                 * DSL tag.
                 *
                 * Его содержимое вообще
                 * не нормализуем.
                 */
                if (character == '[')
                {
                    int closingBracket =
                        source.IndexOf(
                            ']',
                            index + 1
                        );


                    if (closingBracket >= 0)
                    {
                        FlushPendingSpace(
                            result,
                            ref pendingSpace,
                            atLineStart,
                            ref lastVisibleWasSpace
                        );


                        result.Append(
                            source,
                            index,
                            closingBracket -
                            index +
                            1
                        );


                        index =
                            closingBracket + 1;

                        continue;
                    }
                }


                /*
                 * Windows / old Mac line endings
                 * приводим к \n.
                 */
                if (character == '\r')
                {
                    if (
                        index + 1 < source.Length &&
                        source[index + 1] == '\n'
                    )
                    {
                        index++;
                    }


                    AppendLineBreak(
                        result,
                        ref pendingSpace,
                        ref atLineStart,
                        ref lastVisibleWasSpace
                    );


                    index++;

                    continue;
                }


                if (character == '\n')
                {
                    AppendLineBreak(
                        result,
                        ref pendingSpace,
                        ref atLineStart,
                        ref lastVisibleWasSpace
                    );


                    index++;

                    continue;
                }


                /*
                 * Горизонтальный whitespace.
                 */
                if (
                    character == ' ' ||
                    character == '\t'
                )
                {
                    pendingSpace = true;

                    index++;

                    continue;
                }


                /*
                 * Перед обычным символом
                 * добавляем максимум один пробел.
                 */
                if (
                    pendingSpace &&
                    !atLineStart &&
                    !lastVisibleWasSpace
                )
                {
                    result.Append(' ');

                    lastVisibleWasSpace =
                        true;
                }


                pendingSpace = false;


                result.Append(
                    character
                );


                atLineStart = false;
                lastVisibleWasSpace = false;


                index++;
            }


            /*
             * pendingSpace специально
             * не flush'им.
             *
             * Благодаря этому trailing spaces
             * исчезают.
             */


            return result.ToString();
        }


        private static void FlushPendingSpace(
            StringBuilder result,
            ref bool pendingSpace,
            bool atLineStart,
            ref bool lastVisibleWasSpace
        )
        {
            if (
                pendingSpace &&
                !atLineStart &&
                !lastVisibleWasSpace
            )
            {
                result.Append(' ');

                lastVisibleWasSpace =
                    true;
            }


            pendingSpace = false;
        }


        private static void AppendLineBreak(
            StringBuilder result,
            ref bool pendingSpace,
            ref bool atLineStart,
            ref bool lastVisibleWasSpace
        )
        {
            pendingSpace = false;


            result.Append('\n');


            atLineStart = true;
            lastVisibleWasSpace = false;
        }
    }
}
