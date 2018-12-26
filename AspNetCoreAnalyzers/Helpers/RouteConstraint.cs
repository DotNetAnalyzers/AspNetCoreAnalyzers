namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{this.Span.ToString()}")]
    public struct RouteConstraint : IEquatable<RouteConstraint>
    {
        public RouteConstraint(StringLiteralSpan span)
        {
            this.Span = span;
        }

        public StringLiteralSpan Span { get; }

        public static bool operator ==(RouteConstraint left, RouteConstraint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RouteConstraint left, RouteConstraint right)
        {
            return !left.Equals(right);
        }

        public static bool TryRead(StringLiteralSpan span, int pos, out RouteConstraint constraint)
        {
            if (pos >= span.TextSpan.End ||
                span[pos] != ':')
            {
                constraint = default(RouteConstraint);
                return false;
            }

            pos++;
            for (var i = pos; i < span.TextSpan.Length; i++)
            {
                switch (span[i])
                {
                    case '(' when Text.TrySkipPast(span, ref i, "):") ||
                                  Text.TrySkipPast(span, ref i, ")}"):
                        constraint = new RouteConstraint(span.Slice(pos, i - 1));
                        return true;
                    case '}':
                    case ':':
                        constraint = new RouteConstraint(span.Slice(pos, i));
                        return true;
                }
            }

            constraint = default(RouteConstraint);
            return false;
        }

        public bool Equals(RouteConstraint other)
        {
            return this.Span.Equals(other.Span);
        }

        public override bool Equals(object obj)
        {
            return obj is RouteConstraint other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Span.GetHashCode();
        }
    }
}
