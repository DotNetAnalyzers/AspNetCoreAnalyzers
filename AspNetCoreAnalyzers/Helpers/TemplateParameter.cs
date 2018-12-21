namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;

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

        public static bool TryParse(Span source, out TemplateParameter result)
        {
            var text = source.Text;
            var start = text.IndexOf('{');
            if (start < 0 ||
                text.IndexOf('{', start) > 0)
            {
                result = default(TemplateParameter);
                return false;
            }

            start++;
            Text.SkipWhiteSpace(text, ref start);
            var end = text.LastIndexOf('}');
            if (end < start)
            {
                result = default(TemplateParameter);
                return false;
            }

            Text.BackWhiteSpace(text, ref end);
            if (text[end - 1] == '?')
            {
                result = new TemplateParameter(source.Slice(start, end - 1), ImmutableArray.Create(new RouteConstraint(source.Substring(end - 1, 1))));
                return true;
            }

            if (text.IndexOf(':') is int i &&
                i > start)
            {
                var name = source.Slice(start, i);
                if (text.IndexOf(':', i + 1) > i)
                {
                    var builder = ImmutableArray.CreateBuilder<RouteConstraint>();
                    while (RouteConstraint.TryRead(source, i, out var constraint))
                    {
                        builder.Add(constraint);
                        i += constraint.Span.TextSpan.Length + 1;
                    }

                    result = new TemplateParameter(name, builder.ToImmutable());
                    return true;
                }

                result = new TemplateParameter(name, ImmutableArray.Create(new RouteConstraint(source.Slice(i + 1, end))));
                return true;
            }

            result = new TemplateParameter(source.Slice(start, end), ImmutableArray<RouteConstraint>.Empty);
            return true;
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
