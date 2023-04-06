namespace AspNetCoreAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics;

[DebuggerDisplay("{this.Name.ToString()}")]
internal readonly record struct TemplateParameter
{
    internal TemplateParameter(Span name, ImmutableArray<RouteConstraint> constraints)
    {
        this.Name = name;
        this.Constraints = constraints;
    }

    internal Span Name { get; }

    internal ImmutableArray<RouteConstraint> Constraints { get; }

    public bool Equals(TemplateParameter other) => this.Name.Equals(other.Name);

    public override int GetHashCode() => this.Name.GetHashCode();

    internal static bool TryParse(Span span, out TemplateParameter result)
    {
        if (span.TryIndexOf('{', out var start) &&
            span.TryLastIndexOf('}', out var end) &&
            start < end)
        {
            start++;
            if (span[end - 1] == '?')
            {
                result = new TemplateParameter(span.Slice(start, end - 1), ImmutableArray.Create(new RouteConstraint(span.Substring(end - 1, 1))));
                return true;
            }

            if (span.TryIndexOf(':', start, out var i))
            {
                var name = span.Slice(start, i);
                if (span.TryIndexOf(':', i + 1, out _))
                {
                    var builder = ImmutableArray.CreateBuilder<RouteConstraint>();
                    while (RouteConstraint.TryRead(span, i, out var constraint))
                    {
                        builder.Add(constraint);
                        i += constraint.Span.TextSpan.Length + 1;
                    }

                    result = new TemplateParameter(name, builder.ToImmutable());
                    return true;
                }

                result = new TemplateParameter(name, ImmutableArray.Create(new RouteConstraint(span.Slice(i + 1, end))));
                return true;
            }

            if (span.TryIndexOf('=', start, out i))
            {
                result = new TemplateParameter(span.Slice(start, i), ImmutableArray<RouteConstraint>.Empty);
                return true;
            }

            if (span[start] == '*')
            {
                start++;
                if (span[start] == '*')
                {
                    start++;
                }
            }

            result = new TemplateParameter(span.Slice(start, end), ImmutableArray<RouteConstraint>.Empty);
            return true;
        }

        result = default;
        return false;
    }
}
