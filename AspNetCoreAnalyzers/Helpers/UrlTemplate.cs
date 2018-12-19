namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// https://tools.ietf.org/html/rfc1738.
    /// </summary>
    public struct UrlTemplate : IEquatable<UrlTemplate>
    {
        private UrlTemplate(LiteralExpressionSyntax literal, ImmutableArray<PathSegment> path)
        {
            this.Literal = literal;
            this.Path = path;
        }

        public LiteralExpressionSyntax Literal { get; }

        public ImmutableArray<PathSegment> Path { get; }

        public static bool operator ==(UrlTemplate left, UrlTemplate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UrlTemplate left, UrlTemplate right)
        {
            return !left.Equals(right);
        }

        public static bool TryParse(LiteralExpressionSyntax literal, out UrlTemplate template)
        {
            if (literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var text = literal.Token.ValueText;
                var builder = ImmutableArray.CreateBuilder<PathSegment>();
                var start = 0;
                while (TryParse(literal, text, start, out var component))
                {
                    builder.Add(component);
                    start = component.Text.End;
                }

                if (start == text.Length)
                {
                    template = new UrlTemplate(literal, builder.Count == builder.Capacity ? builder.MoveToImmutable() : builder.ToImmutable());
                    return true;
                }
            }

            template = default(UrlTemplate);
            return false;
        }

        public bool Equals(UrlTemplate other)
        {
            return this.Literal.Equals(other.Literal);
        }

        public override bool Equals(object obj)
        {
            return obj is UrlTemplate other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Literal.GetHashCode();
        }

        private static bool TryParse(LiteralExpressionSyntax literal, string text, int start, out PathSegment segment)
        {
            // https://tools.ietf.org/html/rfc3986
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
