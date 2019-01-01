namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics;

    [DebuggerDisplay("{this.Name.ToString()}")]
    public struct TemplateParameter : IEquatable<TemplateParameter>
    {
        public TemplateParameter(Span name, ImmutableArray<RouteConstraint> constraints)
        {
            this.Name = name;
            this.Constraints = constraints;
        }

        public Span Name { get; }

        public ImmutableArray<RouteConstraint> Constraints { get; }

        public static bool operator ==(TemplateParameter left, TemplateParameter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TemplateParameter left, TemplateParameter right)
        {
            return !left.Equals(right);
        }

        public static bool TryParse(Span span, out TemplateParameter result)
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
                    var name = span.Slice(start, i);
                    result = new TemplateParameter(name, ImmutableArray<RouteConstraint>.Empty);
                    return true;
                }

                result = new TemplateParameter(span.Slice(start, end), ImmutableArray<RouteConstraint>.Empty);
                return true;
            }

            result = default(TemplateParameter);
            return false;
        }

        public bool Equals(TemplateParameter other)
        {
            return this.Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            return obj is TemplateParameter other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
