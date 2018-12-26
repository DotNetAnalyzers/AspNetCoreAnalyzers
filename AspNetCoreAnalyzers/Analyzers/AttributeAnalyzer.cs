namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Attribute = Gu.Roslyn.AnalyzerExtensions.Attribute;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            ASP001ParameterName.Descriptor,
            ASP002MissingParameter.Descriptor,
            ASP003ParameterType.Descriptor,
            ASP004ParameterSyntax.Descriptor,
            ASP005ParameterRegex.Descriptor);

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
                        methodDeclaration.TryFindParameter(withMethodParameter.Method.Name, out var parameterSyntax) &&
                        pairs.TrySingle(x => x.Method == null, out var withTemplateParameter) &&
                        withTemplateParameter.Template is TemplateParameter templateParameter)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP001ParameterName.Descriptor,
                                parameterSyntax.Identifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(NameSyntax),
                                    templateParameter.Name.ToString())));

                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP002MissingParameter.Descriptor,
                                templateParameter.Name.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(Text),
                                    withMethodParameter.Method.Name)));
                    }
                    else if (pairs.Count(x => x.Template == null) > 1 &&
                             pairs.Count(x => x.Method == null) > 1)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP001ParameterName.Descriptor,
                                methodDeclaration.ParameterList.GetLocation()));
                    }

                    if (pairs.TryFirst(x => x.Method == null, out _) &&
                        !pairs.TryFirst(x => x.Template == null, out _))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP002MissingParameter.Descriptor,
                                methodDeclaration.ParameterList.GetLocation()));
                    }

                    foreach (var pair in pairs)
                    {
                        if (HasWrongType(pair, out var typeName) &&
                            methodDeclaration.TryFindParameter(pair.Method?.Name, out parameterSyntax))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP003ParameterType.Descriptor,
                                    parameterSyntax.Type.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(TypeSyntax),
                                        typeName)));
                        }
                    }

                    foreach (var segment in template.Path)
                    {
                        if (HasWrongSyntax(segment, out var location, out var syntax))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP004ParameterSyntax.Descriptor,
                                    location,
                                    syntax == null
                                        ? ImmutableDictionary<string, string>.Empty
                                        : ImmutableDictionary<string, string>.Empty.Add(
                                            nameof(Text),
                                            syntax)));
                        }

                        if (HasWrongRegexSyntax(segment, out location, out syntax))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP005ParameterRegex.Descriptor,
                                    location,
                                    syntax == null
                                        ? ImmutableDictionary<string, string>.Empty
                                        : ImmutableDictionary<string, string>.Empty.Add(
                                            nameof(Text),
                                            syntax)));
                        }
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
                    list.Add(template.Path.TrySingle(x => x.Parameter?.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase) == true, out var templateParameter)
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

        private static bool HasWrongType(ParameterPair pair, out string correctType)
        {
            if (pair.Template?.Constraints is ImmutableArray<RouteConstraint> constraints &&
                pair.Method is IParameterSymbol parameter)
            {
                foreach (var constraint in constraints)
                {
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#route-constraint-reference
                    if (TryGetType(constraint, out var type))
                    {
                        correctType = parameter.Type == type ? null : type.Alias ?? type.FullName;
                        return correctType != null;
                    }
                }
            }

            correctType = null;
            return false;

            bool TryGetType(RouteConstraint constraint, out QualifiedType type)
            {
                if (constraint.Span.Equals("bool", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Boolean;
                    return true;
                }

                if (constraint.Span.Equals("decimal", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Decimal;
                    return true;
                }

                if (constraint.Span.Equals("double", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Double;
                    return true;
                }

                if (constraint.Span.Equals("float", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Float;
                    return true;
                }

                if (constraint.Span.Equals("int", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Int32;
                    return true;
                }

                if (constraint.Span.Equals("long", StringComparison.Ordinal) ||
                    constraint.Span.StartsWith("min(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.Span.StartsWith("max(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.Span.StartsWith("range(", StringComparison.OrdinalIgnoreCase))
                {
                    type = KnownSymbol.Int64;
                    return true;
                }

                if (constraint.Span.Equals("datetime", StringComparison.Ordinal))
                {
                    type = KnownSymbol.DateTime;
                    return true;
                }

                if (constraint.Span.Equals("guid", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Guid;
                    return true;
                }

                if (constraint.Span.Equals("alpha", StringComparison.OrdinalIgnoreCase) ||
                    constraint.Span.StartsWith("regex(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.Span.StartsWith("length(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.Span.StartsWith("minlength(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.Span.StartsWith("maxlength(", StringComparison.OrdinalIgnoreCase))
                {
                    type = KnownSymbol.String;
                    return true;
                }

                type = null;
                return false;
            }
        }

        private static bool HasWrongSyntax(PathSegment segment, out Location location, out string correctSyntax)
        {
            if (segment.Parameter is TemplateParameter parameter)
            {
                foreach (var constraint in parameter.Constraints)
                {
                    var text = constraint.Span;
                    if (text.StartsWith("min(", StringComparison.OrdinalIgnoreCase) ||
                        text.StartsWith("max(", StringComparison.OrdinalIgnoreCase) ||
                        text.StartsWith("minlength(", StringComparison.OrdinalIgnoreCase) ||
                        text.StartsWith("maxlength(", StringComparison.OrdinalIgnoreCase) ||
                        text.StartsWith("length(", StringComparison.OrdinalIgnoreCase) ||
                        text.StartsWith("range(", StringComparison.OrdinalIgnoreCase) ||
                        text.StartsWith("regex(", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!text.EndsWith(")", StringComparison.Ordinal))
                        {
                            location = constraint.Span.GetLocation();
                            correctSyntax = text + ")";
                            return true;
                        }
                    }

                    if (HasWrongIntArgumentSyntax(constraint, "min", out location) ||
                        HasWrongIntArgumentSyntax(constraint, "max", out location) ||
                        HasWrongIntArgumentSyntax(constraint, "minlength", out location) ||
                        HasWrongIntArgumentSyntax(constraint, "maxlength", out location))
                    {
                        correctSyntax = null;
                        return true;
                    }

                    if (!text.Equals("?", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("int", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("bool", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("datetime", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("decimal", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("double", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("float", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("guid", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("long", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("alpha", StringComparison.OrdinalIgnoreCase) &&
                        !text.Equals("required", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("minlength(", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("maxlength(", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("length(", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("min(", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("max(", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("regex(", StringComparison.OrdinalIgnoreCase) &&
                        !text.StartsWith("range(", StringComparison.OrdinalIgnoreCase))
                    {
                        location = constraint.Span.GetLocation();
                        correctSyntax = null;
                        return true;
                    }
                }
            }
            else
            {
                var text = segment.Span;
                if (text.StartsWith("{", StringComparison.Ordinal) &&
                    !text.EndsWith("}", StringComparison.Ordinal))
                {
                    location = segment.Span.GetLocation();
                    correctSyntax = text + "}";
                    return true;
                }

                if (!text.StartsWith("{", StringComparison.Ordinal) &&
                    text.EndsWith("}", StringComparison.Ordinal))
                {
                    location = segment.Span.GetLocation();
                    correctSyntax = "{" + text;
                    return true;
                }
            }

            location = null;
            correctSyntax = null;
            return false;

            bool HasWrongIntArgumentSyntax(RouteConstraint constraint, string methodName, out Location result)
            {
                var text = constraint.Span.Text;
                if (text.Length > methodName.Length + 2 &&
                    text.StartsWith(methodName.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
                    text[methodName.Length] == '(' &&
                    text[text.Length - 1] == ')')
                {
                    for (var i = methodName.Length + 1; i < text.Length - 2; i++)
                    {
                        if (!char.IsDigit(text[i]))
                        {
                            result = constraint.Span.GetLocation(methodName.Length + 1, text.Length - methodName.Length - 2);
                            return true;
                        }
                    }
                }

                result = null;
                return false;
            }
        }

        private static bool HasWrongRegexSyntax(PathSegment segment, out Location location, out string correctSyntax)
        {
            if (segment.Parameter is TemplateParameter parameter)
            {
                foreach (var constraint in parameter.Constraints)
                {
                    var text = constraint.Span.Text;
                    if (text.StartsWith("regex(".AsSpan(), StringComparison.OrdinalIgnoreCase))
                    {
                        for (var i = 6; i < text.Length - 1; i++)
                        {
                            if (NotEscaped())
                            {
                                var escaped = new List<char>(text.Length - 7);
                                for (i = 6; i < text.Length - 1; i++)
                                {
                                    escaped.Add(text[i]);
                                    if (NotEscaped())
                                    {
                                        escaped.Add(text[i]);
                                        if (text[i] == '\\' &&
                                            !segment.Span.Literal.IsVerbatim)
                                        {
                                            escaped.Add('\\');
                                            escaped.Add('\\');
                                        }
                                    }
                                }

                                location = constraint.Span.GetLocation(6, text.Length - 7);
                                correctSyntax = new string(escaped.ToArray());
                                return true;
                            }

                            bool NotEscaped()
                            {
                                return NotEscapedChar('\\') ||
                                       NotEscapedChar('{') ||
                                       NotEscapedChar('}') ||
                                       NotEscapedChar('[') ||
                                       NotEscapedChar(']');

                                bool NotEscapedChar(char c)
                                {
                                    return constraint.Span.Text[i] == c &&
                                           constraint.Span.Text[i - 1] != c &&
                                           constraint.Span.Text[i - 1] != '\\' &&
                                           constraint.Span.Text[i + 1] != c;
                                }
                            }
                        }
                    }
                }
            }

            location = null;
            correctSyntax = null;
            return false;
        }
    }
}
