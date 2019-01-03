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
            ASP001ParameterSymbolName.Descriptor,
            ASP002RouteParameterName.Descriptor,
            ASP003ParameterSymbolType.Descriptor,
            ASP004RouteParameterType.Descriptor,
            ASP005ParameterSyntax.Descriptor,
            ASP006ParameterRegex.Descriptor,
            ASP007MissingParameter.Descriptor,
            ASP008ValidRouteParameterName.Descriptor,
            ASP009KebabCaseUrl.Descriptor,
            ASP010UrlSyntax.Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute &&
                TryGetTemplate(attribute, context, out var template))
            {
                if (context.ContainingSymbol is IMethodSymbol method &&
                    attribute.TryFirstAncestor(out MethodDeclarationSyntax methodDeclaration))
                {
                    using (var pairs = GetPairs(template, method))
                    {
                        if (pairs.TrySingle(x => x.Route == null, out var withMethodParameter) &&
                            methodDeclaration.TryFindParameter(withMethodParameter.Symbol.Name, out var parameterSyntax) &&
                            pairs.TrySingle(x => x.Symbol == null, out var withTemplateParameter) &&
                            withTemplateParameter.Route is TemplateParameter templateParameter)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP001ParameterSymbolName.Descriptor,
                                    parameterSyntax.Identifier.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(NameSyntax),
                                        templateParameter.Name.ToString())));

                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP002RouteParameterName.Descriptor,
                                    templateParameter.Name.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(UrlTemplate),
                                        withMethodParameter.Symbol.Name)));
                        }
                        else if (pairs.Count(x => x.Route == null) > 1 &&
                                 pairs.Count(x => x.Symbol == null) > 1)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP001ParameterSymbolName.Descriptor,
                                    methodDeclaration.ParameterList.GetLocation()));
                        }

                        if (pairs.TryFirst(x => x.Symbol == null, out _) &&
                            !pairs.TryFirst(x => x.Route == null, out _))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    ASP007MissingParameter.Descriptor,
                                    methodDeclaration.ParameterList.GetLocation()));
                        }

                        foreach (var pair in pairs)
                        {
                            if (HasWrongType(pair, out var typeName, out var constraintLocation, out var text) &&
                                methodDeclaration.TryFindParameter(pair.Symbol?.Name, out parameterSyntax))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        ASP003ParameterSymbolType.Descriptor,
                                        parameterSyntax.Type.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), typeName)));

                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        ASP004RouteParameterType.Descriptor,
                                        constraintLocation,
                                        text == null
                                            ? ImmutableDictionary<string, string>.Empty
                                            : ImmutableDictionary<string, string>.Empty.Add(nameof(UrlTemplate), text)));
                            }
                        }
                    }
                }

                foreach (var segment in template.Path)
                {
                    if (HasWrongSyntax(segment, out var location, out var syntax))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP005ParameterSyntax.Descriptor,
                                location,
                                syntax == null
                                    ? ImmutableDictionary<string, string>.Empty
                                    : ImmutableDictionary<string, string>.Empty.Add(nameof(UrlTemplate), syntax)));
                    }

                    if (HasWrongRegexSyntax(segment, out location, out syntax))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP006ParameterRegex.Descriptor,
                                location,
                                syntax == null
                                    ? ImmutableDictionary<string, string>.Empty
                                    : ImmutableDictionary<string, string>.Empty.Add(nameof(UrlTemplate), syntax)));
                    }

                    if (HasInvalidName(segment, out location, out var name))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP008ValidRouteParameterName.Descriptor,
                                location,
                                name == null
                                ? ImmutableDictionary<string, string>.Empty
                                : ImmutableDictionary<string, string>.Empty.Add(nameof(UrlTemplate), name)));
                    }

                    if (ShouldKebabCase(segment, out var kebabCase))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP009KebabCaseUrl.Descriptor,
                                segment.Span.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(UrlTemplate), kebabCase)));
                    }

                    if (ContainsReservedCharacter(segment, out location))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ASP010UrlSyntax.Descriptor,
                                location,
                                segment.Span.ToString(location)));
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
                 Attribute.IsType(attribute, KnownSymbol.HttpHeadAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpOptionsAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPatchAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPostAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.HttpPutAttribute, context.SemanticModel, context.CancellationToken) ||
                 Attribute.IsType(attribute, KnownSymbol.RouteAttribute, context.SemanticModel, context.CancellationToken)) &&
                UrlTemplate.TryParse(literal, out template))
            {
                return true;
            }

            template = default(UrlTemplate);
            return false;
        }

        private static PooledList<ParameterPair> GetPairs(UrlTemplate template, IMethodSymbol method)
        {
            var list = PooledList<ParameterPair>.Borrow();
            foreach (var parameter in method.Parameters)
            {
                if (IsFromRoute(parameter))
                {
                    list.Add(template.Path.TrySingle(x => x.Parameter?.Name.Equals(parameter.Name, StringComparison.Ordinal) == true, out var templateParameter)
                                 ? new ParameterPair(templateParameter.Parameter, parameter)
                                 : new ParameterPair(null, parameter));
                }
            }

            foreach (var component in template.Path)
            {
                if (component.Parameter is TemplateParameter templateParameter &&
                    list.All(x => x.Route != templateParameter))
                {
                    list.Add(new ParameterPair(templateParameter, null));
                }
            }

            return list;

            bool IsFromRoute(IParameterSymbol p)
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

        private static bool HasWrongType(ParameterPair pair, out string correctType, out Location constraintLocation, out string correctConstraint)
        {
            if (pair.Route is TemplateParameter templateParameter &&
                templateParameter.Constraints is ImmutableArray<RouteConstraint> constraints &&
                pair.Symbol is IParameterSymbol parameterSymbol)
            {
                foreach (var constraint in constraints)
                {
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#route-constraint-reference
                    if (TryGetType(constraint.Span, out var type))
                    {
                        correctType = parameterSymbol.Type == type ? null : type.Alias ?? type.FullName;
                        constraintLocation = constraint.Span.GetLocation();
                        correctConstraint = GetCorrectConstraintType(constraint);
                        return correctType != null;
                    }

                    if (constraint.Span.Equals("?", StringComparison.Ordinal) &&
                        parameterSymbol.Type.IsValueType &&
                        parameterSymbol.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                    {
                        correctType = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) + "?";
                        constraintLocation = constraint.Span.GetLocation();
                        correctConstraint = string.Empty;
                        return true;
                    }
                }

                if (!constraints.TryFirst(x => x.Span.Equals("?", StringComparison.Ordinal), out _) &&
                    parameterSymbol.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                    parameterSymbol.Type is INamedTypeSymbol namedType &&
                    namedType.TypeArguments.TrySingle(out var typeArg))
                {
                    correctType = typeArg.ToString();
                    constraintLocation = templateParameter.Name.GetLocation();
                    correctConstraint = $"{templateParameter.Name}?";
                    return true;
                }
            }

            correctType = null;
            constraintLocation = null;
            correctConstraint = null;
            return false;

            bool TryGetType(Span constraint, out QualifiedType type)
            {
                if (constraint.Equals("bool", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Boolean;
                    return true;
                }

                if (constraint.Equals("decimal", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Decimal;
                    return true;
                }

                if (constraint.Equals("double", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Double;
                    return true;
                }

                if (constraint.Equals("float", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Float;
                    return true;
                }

                if (constraint.Equals("int", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Int32;
                    return true;
                }

                if (constraint.Equals("long", StringComparison.Ordinal) ||
                    constraint.StartsWith("min(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.StartsWith("max(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.StartsWith("range(", StringComparison.OrdinalIgnoreCase))
                {
                    type = KnownSymbol.Int64;
                    return true;
                }

                if (constraint.Equals("datetime", StringComparison.Ordinal))
                {
                    type = KnownSymbol.DateTime;
                    return true;
                }

                if (constraint.Equals("guid", StringComparison.Ordinal))
                {
                    type = KnownSymbol.Guid;
                    return true;
                }

                if (constraint.Equals("alpha", StringComparison.OrdinalIgnoreCase) ||
                    constraint.StartsWith("regex(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.StartsWith("length(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.StartsWith("minlength(", StringComparison.OrdinalIgnoreCase) ||
                    constraint.StartsWith("maxlength(", StringComparison.OrdinalIgnoreCase))
                {
                    type = KnownSymbol.String;
                    return true;
                }

                type = null;
                return false;
            }

            string GetCorrectConstraintType(RouteConstraint constraint)
            {
                if (constraint.Span.Equals("bool", StringComparison.Ordinal) ||
                    constraint.Span.Equals("decimal", StringComparison.Ordinal) ||
                    constraint.Span.Equals("double", StringComparison.Ordinal) ||
                    constraint.Span.Equals("float", StringComparison.Ordinal) ||
                    constraint.Span.Equals("int", StringComparison.Ordinal) ||
                    constraint.Span.Equals("long", StringComparison.Ordinal) ||
                    constraint.Span.Equals("datetime", StringComparison.Ordinal) ||
                    constraint.Span.Equals("guid", StringComparison.Ordinal))
                {
                    return parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                                          .ToLower();
                }

                return null;
            }
        }

        private static bool HasWrongSyntax(PathSegment segment, out Location location, out string correctSyntax)
        {
            if (segment.Parameter is TemplateParameter parameter)
            {
                if (parameter.Name.EndsWith("}", StringComparison.Ordinal))
                {
                    location = parameter.Name.GetLocation();
                    correctSyntax = parameter.Name.ToString().TrimEnd('}');
                    return true;
                }

                if (parameter.Name.StartsWith("{", StringComparison.Ordinal))
                {
                    location = parameter.Name.GetLocation();
                    correctSyntax = parameter.Name.ToString().TrimStart('{');
                    return true;
                }

                if (parameter.Name.Length == 0 ||
                    parameter.Name.Contains('*') ||
                    parameter.Name.Contains('{') ||
                    parameter.Name.Contains('}') ||
                    parameter.Name.Contains('/') ||
                    parameter.Name.Contains('?'))
                {
                    location = parameter.Name.GetLocation();
                    correctSyntax = null;
                    return true;
                }

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
                var text = constraint.Span;
                if (text.Length > methodName.Length + 2 &&
                    text.StartsWith(methodName, StringComparison.OrdinalIgnoreCase) &&
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
                    var text = constraint.Span;
                    if (text.StartsWith("regex(", StringComparison.OrdinalIgnoreCase))
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
                                    return text[i] == c &&
                                           text[i - 1] != c &&
                                           text[i - 1] != '\\' &&
                                           text[i + 1] != c;
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

        private static bool HasInvalidName(PathSegment segment, out Location location, out string correctName)
        {
            if (segment.Parameter is TemplateParameter parameter)
            {
                if (parameter.Name.StartsWith(" ", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.EndsWith(" ", StringComparison.OrdinalIgnoreCase))
                {
                    location = parameter.Name.GetLocation();
                    correctName = parameter.Name.ToString().Trim();
                    return true;
                }

                if (parameter.Name.Equals("action", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("area", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("controller", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("handler", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("page", StringComparison.OrdinalIgnoreCase))
                {
                    location = parameter.Name.GetLocation();
                    correctName = null;
                    return true;
                }
            }

            location = null;
            correctName = null;
            return false;
        }

        private static bool ShouldKebabCase(PathSegment segment, out string kebabCase)
        {
            if (segment.Parameter == null &&
                IsHumpOrSnakeCased(segment.Span))
            {
                var builder = StringBuilderPool.Borrow();
                for (var i = 0; i < segment.Span.Length; i++)
                {
                    var c = segment.Span[i];
                    if (char.IsUpper(c))
                    {
                        if (i > 0)
                        {
                            _ = builder.Append("-");
                        }

                        _ = builder.Append(char.ToLower(c));
                    }
                    else if (c == '_')
                    {
                        _ = builder.Append("-");
                    }
                    else
                    {
                        _ = builder.Append(c);
                    }
                }

                kebabCase = builder.Return();
                return true;
            }

            kebabCase = null;
            return false;

            bool IsHumpOrSnakeCased(Span span)
            {
                for (var i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if (char.IsUpper(c) ||
                        c == '_')
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc3986#section-2.2.
        /// </summary>
        private static bool ContainsReservedCharacter(PathSegment segment, out Location location)
        {
            if (segment.Parameter == null)
            {
                for (var i = 0; i < segment.Span.Length; i++)
                {
                    switch (segment.Span[i])
                    {
                        //case ':':
                        //case '/':
                        case '?':
                        //case '#':
                        //case '[':
                        //case ']':
                        //case '@':
                            location = segment.Span.GetLocation(i, 1);
                            return true;
                    }
                }
            }

            location = null;
            return false;
        }
    }
}
