namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            ASP001ParameterName.Descriptor,
            ASP002MissingParameter.Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute &&
                context.ContainingSymbol is IMethodSymbol method &&
                attribute.TryFirstAncestor<MethodDeclarationSyntax>(out var methodDeclaration) &&
                TryGetTemplate(attribute, context, out var template))
            {
                foreach (var component in template.Path)
                {
                    if (TryGetParameterName(component.Text, out var name))
                    {
                        if (method.Parameters.TrySingle(x => IsFromRoute(x), out var single) &&
                            single.Name != name)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP001ParameterName.Descriptor,
                                    single.Locations.Single(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(NameSyntax), name)));
                        }

                        if (!method.Parameters.TryFirst(x => IsFromRoute(x), out _))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP002MissingParameter.Descriptor,
                                    methodDeclaration.ParameterList.GetLocation()));
                        }
                    }
                }
            }
        }

        private static bool TryGetParameterName(string text, out string name)
        {
            var start = text.IndexOf('{');
            if (start < 0 ||
                text.IndexOf('}') < start ||
                text.IndexOf('{', start) > 0)
            {
                name = null;
                return false;
            }

            start++;
            while (start < text.Length &&
                   text[start] == ' ')
            {
                start++;
            }

            var end = text.IndexOf('}', start);
            if (end < 0)
            {
                name = null;
                return false;
            }

            while (text[end] == ' ')
            {
                end--;
            }

            name = text.Substring(start, end - start);
            return true;
        }

        private static bool TryGetTemplate(AttributeSyntax attribute, SyntaxNodeAnalysisContext context, out UrlTemplate template)
        {
            if (attribute.ArgumentList is AttributeArgumentListSyntax argumentList &&
                argumentList.Arguments.TrySingle(out var argument) &&
                argument.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                (Attribute.IsType(attribute, KnownSymbol.HttpGetAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPostAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPutAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpDeleteAttribute, context.SemanticModel, context.CancellationToken)) &&
                UrlTemplate.TryParse(literal, out template))
            {
                return true;
            }

            template = default(UrlTemplate);
            return false;
        }

        private static bool IsFromRoute(IParameterSymbol p)
        {
            foreach (var attributeData in p.GetAttributes())
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
