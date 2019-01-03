namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{this.Span.ToString()}")]
    public struct RouteConstraint : IEquatable<RouteConstraint>
    {
        public RouteConstraint(Span span)
        {
            this.Span = span;
        }

        public Span Span { get; }

        public static bool operator ==(RouteConstraint left, RouteConstraint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RouteConstraint left, RouteConstraint right)
        {
            return !left.Equals(right);
        }

        public static bool TryRead(Span span, int pos, out RouteConstraint constraint)
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
                    case '(' when span.TryIndexOf("):", i, out var end) ||
                                  span.TryIndexOf(")}", i, out end):
                        constraint = new RouteConstraint(span.Slice(pos, end + 1));
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
