namespace AspNetCoreAnalyzers
{
    using System.Diagnostics;

    [DebuggerDisplay("{this.Span.Text}")]
    public struct PathSegment
    {
        public PathSegment(StringLiteral literal, int start, int end)
        {
            this.Span = new Span(literal, start, end);
            this.Parameter = TemplateParameter.TryParse(this.Span, out var parameter)
                ? parameter
                : (TemplateParameter?)null;
        }

        public Span Span { get; }

        public TemplateParameter? Parameter { get; }

        public static bool TryRead(StringLiteral literal, int start, out PathSegment segment)
        {
            // https://tools.ietf.org/html/rfc3986
            var text = literal.LiteralExpression.Token.ValueText;
            var pos = start;
            if (pos < text.Length - 1)
            {
                if (pos == 0)
                {
                    pos++;
                }
                else if (text[pos] == '/')
                {
                    pos++;
                    start++;
                }
                else
                {
                    segment = default(PathSegment);
                    return false;
                }

                while (pos < text.Length)
                {
                    if (text[pos] == '/')
                    {
                        segment = new PathSegment(literal, start, pos);
                        return true;
                    }

                    if (text[pos] == '(')
                    {
                        pos++;
                        while (Text.TrySkipPast(text, ref pos, ")"))
                        {
                            Text.SkipWhiteSpace(text, ref pos);
                            switch (text[pos])
                            {
                                case ':':
                                    break;
                                case '}':
                                    pos++;
                                    segment = new PathSegment(literal, start, pos);
                                    return true;
                            }
                        }
                    }

                    pos++;
                }

                if (pos == text.Length)
                {
                    segment = new PathSegment(literal, start, pos);
                    return true;
                }
            }

            segment = default(PathSegment);
            return false;
        }
    }
}
