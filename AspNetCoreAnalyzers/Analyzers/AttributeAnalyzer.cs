namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text.RegularExpressions;
    using AspNetCoreAnalyzers.Helpers;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            ASP001ParameterName.Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute)
            {
                if (context.ContainingSymbol is IMethodSymbol method &&
                    TryGetTemplate(attribute, context, out var template) &&
                    TryGetRouteParameter(template, out var name) &&
                    method.Parameters.TrySingle(x => IsFromRoute(x), out var parameter) &&
                    parameter.Name != name)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            ASP001ParameterName.Descriptor,
                            parameter.Locations.Single(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(NameSyntax), name)));
                }
            }
        }

        private static bool TryGetTemplate(AttributeSyntax attribute, SyntaxNodeAnalysisContext context, out LiteralExpressionSyntax literal)
        {
            if (attribute.ArgumentList is AttributeArgumentListSyntax argumentList &&
                argumentList.Arguments.TrySingle(out var argument) &&
                argument.Expression is LiteralExpressionSyntax candidate &&
                candidate.IsKind(SyntaxKind.StringLiteralExpression) &&
                (Attribute.IsType(attribute, KnownSymbol.HttpGetAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPostAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPutAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpDeleteAttribute, context.SemanticModel, context.CancellationToken)))
            {
                literal = candidate;
                return true;
            }

            literal = null;
            return false;
        }

        private static bool TryGetRouteParameter(LiteralExpressionSyntax literal, out string name)
        {
            var match = Regex.Match(literal.Token.ValueText, @"\{(?<name>\w+)}");
            if (match.Success)
            {
                name = match.Groups["name"].Value;
                return true;
            }

            name = null;
            return false;
        }

        private static bool IsFromRoute(IParameterSymbol parameter)
        {
            foreach (var attributeData in parameter.GetAttributes())
            {
                if (attributeData.AttributeClass == KnownSymbol.FromRouteAttribute)
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
