namespace AspNetCoreAnalyzers
{
    using System;

    public static class Text
    {
        public static int IndexOf(this ReadOnlySpan<char> span, char value, int startIndex)
        {
            for (var i = startIndex; i < span.Length; i++)
            {
                if (span[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void SkipWhiteSpace(ReadOnlySpan<char> text, ref int pos)
        {
            while (pos < text.Length &&
                   text[pos] == ' ')
            {
                pos++;
            }
        }

        public static bool TrySkipDigits(ReadOnlySpan<char> text, ref int pos)
        {
            var before = pos;
            while (pos < text.Length &&
                  char.IsDigit(text[pos]))
            {
                pos++;
            }

            return pos != before;
        }

        public static bool TrySkipPast(ReadOnlySpan<char> text, ref int pos, string substring)
        {
            var before = pos;
            while (pos + substring.Length <= text.Length)
            {
                if (IsAt(text, pos, substring))
                {
                    pos += substring.Length;
                    return true;
                }

                pos++;
            }

            pos = before;
            return false;
        }

        public static void BackWhiteSpace(ReadOnlySpan<char> text, ref int pos)
        {
            while (pos >= 0 &&
                   text[pos] == ' ')
            {
                pos--;
            }
        }

        private static bool IsAt(ReadOnlySpan<char> text, int pos, string substring)
        {
            for (var i = 0; i < substring.Length; i++)
            {
                if (text[pos + i] != substring[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
