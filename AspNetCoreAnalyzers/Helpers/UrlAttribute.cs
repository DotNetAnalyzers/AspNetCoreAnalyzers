﻿namespace AspNetCoreAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

internal readonly record struct UrlAttribute
{
    internal UrlAttribute(AttributeSyntax attribute, ITypeSymbol type, UrlTemplate? urlTemplate)
    {
        this.Attribute = attribute;
        this.Type = type;
        this.UrlTemplate = urlTemplate;
    }

    internal AttributeSyntax Attribute { get; }

    internal ITypeSymbol Type { get; }

    internal UrlTemplate? UrlTemplate { get; }

    public bool Equals(UrlAttribute other) => Equals(this.Attribute, other.Attribute);

    public override int GetHashCode() => this.Attribute?.GetHashCode() ?? 0;

    internal static bool TryCreate(AttributeSyntax attribute, SyntaxNodeAnalysisContext context, out UrlAttribute result)
    {
        if (context.SemanticModel.TryGetType(attribute, context.CancellationToken, out var type) &&
            (type == KnownSymbol.HttpDeleteAttribute ||
             type == KnownSymbol.HttpGetAttribute ||
             type == KnownSymbol.HttpHeadAttribute ||
             type == KnownSymbol.HttpOptionsAttribute ||
             type == KnownSymbol.HttpPatchAttribute ||
             type == KnownSymbol.HttpPostAttribute ||
             type == KnownSymbol.HttpPutAttribute ||
             type == KnownSymbol.RouteAttribute))
        {
            if (attribute.ArgumentList is AttributeArgumentListSyntax argumentList &&
                argumentList.Arguments.TrySingle(out var argument) &&
                argument.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                AspNetCoreAnalyzers.UrlTemplate.TryParse(literal, out var template))
            {
                result = new UrlAttribute(attribute, type, template);
            }
            else
            {
                result = new UrlAttribute(attribute, type, null);
            }

            return true;
        }

        result = default;
        return false;
    }

    internal bool TryGetParentMember(out MemberDeclarationSyntax memberDeclaration)
    {
        if (this.Attribute.Parent is AttributeListSyntax { Parent: MemberDeclarationSyntax temp })
        {
            memberDeclaration = temp;
            return true;
        }

        memberDeclaration = null!;
        return false;
    }
}
