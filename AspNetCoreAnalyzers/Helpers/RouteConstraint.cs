namespace AspNetCoreAnalyzers;

using System.Diagnostics;

[DebuggerDisplay("{this.Span.ToString()}")]
internal readonly record struct RouteConstraint
{
    internal RouteConstraint(Span span)
    {
        this.Span = span;
    }

    internal Span Span { get; }

    public bool Equals(RouteConstraint other) => this.Span.Equals(other.Span);

    public override int GetHashCode() => this.Span.GetHashCode();

    internal static bool TryRead(Span span, int pos, out RouteConstraint constraint)
    {
        if (pos >= span.TextSpan.End ||
            span[pos] != ':')
        {
            constraint = default;
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

        constraint = default;
        return false;
    }
}
