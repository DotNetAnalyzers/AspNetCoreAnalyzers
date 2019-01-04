namespace AspNetCoreAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    public static class PooledStringBuilderExt
    {
        public static StringBuilderPool.PooledStringBuilder Append(this StringBuilderPool.PooledStringBuilder builder, string text, int startIndex)
        {
            for (var i = startIndex; i < text.Length; i++)
            {
                _ = builder.Append(text[i]);
            }

            return builder;
        }
    }
}
