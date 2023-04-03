namespace AspNetCoreAnalyzers;

using System;

internal static class StringExt
{
    internal static bool TryIndexOf(this string text, string value, int startIndex, out int indexOf)
    {
        indexOf = text.IndexOf(value, startIndex, StringComparison.Ordinal);
        return indexOf >= 0;
    }

    internal static bool TryIndexOf(this string text, string value, out int indexOf)
    {
        indexOf = text.IndexOf(value, 0, StringComparison.Ordinal);
        return indexOf >= 0;
    }
}
