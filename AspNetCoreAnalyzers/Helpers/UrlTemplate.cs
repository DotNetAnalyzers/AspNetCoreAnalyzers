namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// https://tools.ietf.org/html/rfc1738.
    /// </summary>
    [DebuggerDisplay("{this.Literal.ToString()}")]
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
                var builder = ImmutableArray.CreateBuilder<PathSegment>();
                var pos = 0;
                while (PathSegment.TryRead(literal, pos, out var component))
                {
                    builder.Add(component);
                    pos = component.Span.TextSpan.End;
                }

                if (pos == literal.Token.ValueText.Length)
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
    }
}
