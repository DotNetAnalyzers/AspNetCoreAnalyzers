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
                attribute.TryFirstAncestor(out MethodDeclarationSyntax methodDeclaration) &&
                TryGetTemplate(attribute, context, out var template))
            {
                using (var pairs = GetPairs(template, method))
                {
                    if (pairs.TrySingle(x => x.Template == null, out var withMethodParameter) &&
                        pairs.TrySingle(x => x.Method == null, out var withTemplateParameter))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP001ParameterName.Descriptor,
                                withMethodParameter.Method.Locations.Single(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(NameSyntax), withTemplateParameter.Template.Value.Name.Text)));
                    }

                    if (pairs.TrySingle(x => x.Template != null, out _) &&
                        pairs.TrySingle(x => x.Method == null, out _))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP002MissingParameter.Descriptor,
                                methodDeclaration.ParameterList.GetLocation()));
                    }
                }
            }
        }

        private static bool TryGetTemplate(AttributeSyntax attribute, SyntaxNodeAnalysisContext context, out UrlTemplate template)
        {
            if (attribute.ArgumentList is AttributeArgumentListSyntax argumentList &&
                argumentList.Arguments.TrySingle(out var argument) &&
                argument.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                (Attribute.IsType(attribute, KnownSymbol.HttpDeleteAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpGetAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPatchAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPostAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPutAttribute, context.SemanticModel, context.CancellationToken)) &&
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

        private static PooledList<ParameterPair> GetPairs(UrlTemplate template, IMethodSymbol method)
        {
            var list = PooledList<ParameterPair>.Borrow();
            foreach (var parameter in method.Parameters)
            {
                if (IsFromRoute(parameter))
                {
                    list.Add(template.Path.TrySingle(x => x.Parameter?.Name.Text == parameter.Name, out var templateParameter)
                                 ? new ParameterPair(templateParameter.Parameter, parameter)
                                 : new ParameterPair(null, parameter));
                }
            }

            foreach (var component in template.Path)
            {
                if (component.Parameter is TemplateParameter templateParameter &&
                    list.All(x => x.Template != templateParameter))
                {
                    list.Add(new ParameterPair(templateParameter, null));
                }
            }

            return list;
        }
    }
}
