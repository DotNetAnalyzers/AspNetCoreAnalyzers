namespace AspNetCoreAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// https://tools.ietf.org/html/rfc1738.
/// </summary>
[DebuggerDisplay("{this.Literal.LiteralExpression.ToString()}")]
internal readonly record struct UrlTemplate
{
    private UrlTemplate(StringLiteral literal, ImmutableArray<PathSegment> path)
    {
        this.Literal = literal;
        this.Path = path;
    }

    internal StringLiteral Literal { get; }

    internal ImmutableArray<PathSegment> Path { get; }

    public bool Equals(UrlTemplate other) => this.Literal.Equals(other.Literal);

    public override int GetHashCode() => this.Literal.GetHashCode();

    internal static bool TryParse(LiteralExpressionSyntax literal, out UrlTemplate template)
    {
        if (literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var stringLiteral = new StringLiteral(literal);
            var builder = ImmutableArray.CreateBuilder<PathSegment>();
            var pos = 0;
            while (PathSegment.TryRead(stringLiteral, pos, out var component))
            {
                builder.Add(component);
                pos = component.Span.TextSpan.End;
            }

            if (pos == literal.Token.ValueText.Length)
            {
                template = new UrlTemplate(stringLiteral, builder.Count == builder.Capacity ? builder.MoveToImmutable() : builder.ToImmutable());
                return true;
            }
        }

        template = default;
        return false;
    }
}
