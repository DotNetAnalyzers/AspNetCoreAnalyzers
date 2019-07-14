namespace AspNetCoreAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class PooledStringBuilderExt
    {
        internal static StringBuilderPool.PooledStringBuilder Append(this StringBuilderPool.PooledStringBuilder builder, string text, int startIndex)
        {
            for (var i = startIndex; i < text.Length; i++)
            {
                _ = builder.Append(text[i]);
            }

            return builder;
        }
    }
}
