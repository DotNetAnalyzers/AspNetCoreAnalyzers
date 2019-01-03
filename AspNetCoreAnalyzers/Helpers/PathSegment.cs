namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{this.Span.ToString()}")]
    public struct PathSegment : IEquatable<PathSegment>
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

        public static bool operator ==(PathSegment left, PathSegment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PathSegment left, PathSegment right)
        {
            return !left.Equals(right);
        }

        public static bool TryRead(StringLiteral literal, int start, out PathSegment segment)
        {
            // https://tools.ietf.org/html/rfc3986
            var text = literal.ValueText;
            var pos = start;
            if (TrySkipStart())
            {
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

            bool TrySkipStart()
            {
                if (pos >= text.Length)
                {
                    return false;
                }

                if (pos == 0)
                {
                    if (text.StartsWith("~/", StringComparison.Ordinal))
                    {
                        pos += 2;
                        start = 2;
                    }
                    else if (text.StartsWith("~", StringComparison.Ordinal) ||
                             text.StartsWith("/", StringComparison.Ordinal))
                    {
                        pos++;
                        start++;
                    }

                    pos++;
                    return true;
                }

                if (text[pos] == '/')
                {
                    pos++;
                    start++;
                    return true;
                }

                return false;
            }
        }

        public bool Equals(PathSegment other)
        {
            return this.Span.Equals(other.Span);
        }

        public override bool Equals(object obj)
        {
            return obj is PathSegment other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Span.GetHashCode();
        }
    }
}
