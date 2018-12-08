namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// https://tools.ietf.org/html/rfc1738.
    /// </summary>
    public struct UrlTemplate : IEquatable<UrlTemplate>
    {
        private UrlTemplate(LiteralExpressionSyntax literal, ImmutableArray<Component> path)
        {
            this.Literal = literal;
            this.Path = path;
        }

        public LiteralExpressionSyntax Literal { get; }

        public ImmutableArray<Component> Path { get; }

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
                var builder = ImmutableArray.CreateBuilder<Component>(text.Count(x => x == '/') + 1);
                var start = 0;
                while (true)
                {
                    var end = text.IndexOf('/', start);
                    if (end < 0)
                    {
                        builder.Add(new Component(literal, start, text.Length));
                        break;
                    }

                    builder.Add(new Component(literal, start, end));
                    start = end + 1;
                }

                template = new UrlTemplate(literal, builder.MoveToImmutable());
                return true;
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
