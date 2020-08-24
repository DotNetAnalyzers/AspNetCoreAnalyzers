namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.ASP001ParameterSymbolName,
            Descriptors.ASP002RouteParameterName,
            Descriptors.ASP003ParameterSymbolType,
            Descriptors.ASP004RouteParameterType,
            Descriptors.ASP005ParameterSyntax,
            Descriptors.ASP006ParameterRegex,
            Descriptors.ASP007MissingParameter,
            Descriptors.ASP008ValidRouteParameterName,
            Descriptors.ASP009KebabCaseUrl,
            Descriptors.ASP010UrlSyntax,
            Descriptors.ASP011RouteParameterNameMustBeUnique,
            Descriptors.ASP012UseExplicitRoute,
            Descriptors.ASP013ControllerNameShouldMatchRoute);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute &&
                UrlAttribute.TryCreate(attribute, context, out var urlAttribute) &&
                urlAttribute.UrlTemplate is { } template)
            {
                foreach (var segment in template.Path)
                {
                    if (HasWrongName(segment, urlAttribute, context, out var nameReplacement, out var spanReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP001ParameterSymbolName,
                                nameReplacement.Node,
                                nameReplacement.Property(nameof(NameSyntax))));

                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP002RouteParameterName,
                                spanReplacement.Node.GetLocation(),
                                spanReplacement.Property(nameof(UrlTemplate))));
                    }

                    if (HasWrongType(segment, context, out var typeReplacement, out spanReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP003ParameterSymbolType,
                                typeReplacement.Node.GetLocation(),
                                typeReplacement.Property(nameof(TypeSyntax))));

                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP004RouteParameterType,
                                spanReplacement.Node.GetLocation(),
                                spanReplacement.Property(nameof(UrlTemplate))));
                    }

                    if (HasWrongSyntax(segment, out spanReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP005ParameterSyntax,
                                spanReplacement.Node.GetLocation(),
                                spanReplacement.Property(nameof(UrlTemplate))));
                    }

                    if (HasWrongRegexSyntax(segment, out spanReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP006ParameterRegex,
                                spanReplacement.Node.GetLocation(),
                                spanReplacement.Property(nameof(UrlTemplate))));
                    }

                    if (HasMissingMethodParameter(segment, context, out var location, out var name))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP007MissingParameter,
                                location,
                                name));
                    }

                    if (HasInvalidName(segment, out spanReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP008ValidRouteParameterName,
                                spanReplacement.Node.GetLocation(),
                                spanReplacement.Property(nameof(UrlTemplate))));
                    }

                    if (ShouldKebabCase(segment, out var kebabCase))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP009KebabCaseUrl,
                                segment.Span.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(UrlTemplate), kebabCase)));
                    }

                    if (HasSyntaxError(segment, out location))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP010UrlSyntax,
                                location,
                                segment.Span.ToString(location)));
                    }

                    if (IsMultipleOccurringParameter(segment, urlAttribute, context, out location))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP011RouteParameterNameMustBeUnique,
                                location));
                    }

                    if (ShouldUseExplicitRoute(segment, context, out spanReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP012UseExplicitRoute,
                                spanReplacement.Node.GetLocation(),
                                spanReplacement.Property(nameof(UrlTemplate))));
                    }

                    if (ShouldRenameController(segment, urlAttribute, context, out nameReplacement))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.ASP013ControllerNameShouldMatchRoute,
                                nameReplacement.Node,
                                nameReplacement.Property(nameof(NameSyntax)),
                                nameReplacement.NewText));
                    }
                }
            }
        }

        private static bool HasWrongName(PathSegment segment, UrlAttribute urlAttribute, SyntaxNodeAnalysisContext context, out Replacement<Location> nameReplacement, out Replacement<Span> spanReplacement)
        {
            if (context.ContainingSymbol is IMethodSymbol method &&
                segment.Parameter is { } templateParameter)
            {
                if (!TryFindParameter(templateParameter, method, out _))
                {
                    if (method.Parameters.TrySingle(x => IsOrphan(x), out var symbol) &&
                        symbol.TrySingleDeclaration(context.CancellationToken, out var parameterSyntax))
                    {
                        nameReplacement = new Replacement<Location>(parameterSyntax.Identifier.GetLocation(), templateParameter.Name.ToString());
                        spanReplacement = new Replacement<Span>(templateParameter.Name, symbol.Name);
                        return true;
                    }

                    // Using TryFirst instead of Count() here as a silly optimization
                    // As it is called after TrySingle it means Count() > 1
                    if (method.Parameters.TryFirst(x => IsOrphan(x), out _) &&
                        method.TrySingleDeclaration(context.CancellationToken, out MethodDeclarationSyntax? methodDeclaration))
                    {
                        nameReplacement = new Replacement<Location>(methodDeclaration.ParameterList.GetLocation(), null);
                        spanReplacement = new Replacement<Span>(templateParameter.Name, null);
                        return true;
                    }
                }
            }

            nameReplacement = default;
            spanReplacement = default;
            return false;

            bool IsOrphan(IParameterSymbol p)
            {
                if (IsFromRoute(p) &&
                    urlAttribute.UrlTemplate is { } template)
                {
                    foreach (var candidateSegment in template.Path)
                    {
                        if (candidateSegment.Parameter is { } candidateParameter &&
                            candidateParameter.Name.Equals(p.Name, StringComparison.Ordinal))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        private static bool HasWrongType(PathSegment segment, SyntaxNodeAnalysisContext context, out Replacement<TypeSyntax> typeReplacement, out Replacement<Span> constraintReplacement)
        {
            if (segment.Parameter is { Constraints: { } constraints } templateParameter)
            {
                if (context.ContainingSymbol is IMethodSymbol containingMethod &&
                    TryFindParameter(templateParameter, containingMethod, out var parameterSymbol))
                {
                    return HasWrongType(parameterSymbol, out typeReplacement, out constraintReplacement);
                }

                if (context.ContainingSymbol is INamedTypeSymbol &&
                    context.Node.TryFirstAncestor(out ClassDeclarationSyntax? classDeclaration) &&
                    !classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    foreach (var member in classDeclaration.Members)
                    {
                        if (member is MethodDeclarationSyntax methodDeclaration &&
                            HasHttpVerbAttribute(methodDeclaration, context) &&
                            TryFindParameter(templateParameter, methodDeclaration, out var candidateParameter) &&
                            context.SemanticModel.TryGetSymbol(candidateParameter, context.CancellationToken, out parameterSymbol) &&
                            HasWrongType(parameterSymbol, out typeReplacement, out constraintReplacement))
                        {
                            return true;
                        }
                    }
                }
            }

            typeReplacement = default;
            constraintReplacement = default;
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

                type = null!;
                return false;
            }

            string? GetCorrectConstraintType(IParameterSymbol parameterSymbol, RouteConstraint constraint)
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
#pragma warning disable CA1308 // Normalize strings to uppercase
                    return parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).ToLower(CultureInfo.InvariantCulture);
#pragma warning restore CA1308 // Normalize strings to uppercase
                }

                return null;
            }

            bool HasWrongType(IParameterSymbol parameterSymbol, out Replacement<TypeSyntax> newType, out Replacement<Span> newConstraint)
            {
                foreach (var constraint in constraints)
                {
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#route-constraint-reference
                    if (TryGetType(constraint.Span, out var type) &&
                        parameterSymbol.TrySingleDeclaration(context.CancellationToken, out var parameter))
                    {
                        newType = new Replacement<TypeSyntax>(
                            parameter.Type,
                            parameterSymbol.Type == type ? null : type.Alias ?? type.FullName);
                        newConstraint = new Replacement<Span>(
                            constraint.Span,
                            GetCorrectConstraintType(parameterSymbol, constraint));
                        return newType.NewText != null;
                    }

                    if (constraint.Span.Equals("?", StringComparison.Ordinal) &&
                        parameterSymbol.Type.IsValueType &&
                        parameterSymbol.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T &&
                        parameterSymbol.TrySingleDeclaration(context.CancellationToken, out parameter))
                    {
                        newType = new Replacement<TypeSyntax>(
                            parameter.Type,
                            parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) + "?");
                        newConstraint = new Replacement<Span>(
                            constraint.Span,
                            string.Empty);
                        return true;
                    }
                }

                {
                    if (!constraints.TryFirst(x => x.Span.Equals("?", StringComparison.Ordinal), out _) &&
                        parameterSymbol.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                        parameterSymbol.Type is INamedTypeSymbol namedType &&
                        namedType.TypeArguments.TrySingle(out var typeArg) &&
                        parameterSymbol.TrySingleDeclaration(context.CancellationToken, out var parameter))
                    {
                        newType = new Replacement<TypeSyntax>(
                            parameter.Type,
                            typeArg.ToString());
                        newConstraint = new Replacement<Span>(
                            templateParameter.Name,
                            $"{templateParameter.Name}?");
                        return true;
                    }
                }

                newType = default;
                newConstraint = default;
                return false;
            }
        }

        private static bool HasWrongSyntax(PathSegment segment, out Replacement<Span> replacement)
        {
            if (segment.Parameter is { } parameter)
            {
                if (parameter.Name.EndsWith("}", StringComparison.Ordinal))
                {
                    replacement = new Replacement<Span>(
                        parameter.Name,
                        parameter.Name.ToString().TrimEnd('}'));
                    return true;
                }

                if (parameter.Name.StartsWith("{", StringComparison.Ordinal))
                {
                    replacement = new Replacement<Span>(
                        parameter.Name,
                        parameter.Name.ToString().TrimStart('{'));
                    return true;
                }

                if (parameter.Name.Length == 0 ||
                    parameter.Name.Contains('*') ||
                    parameter.Name.Contains('{') ||
                    parameter.Name.Contains('}') ||
                    parameter.Name.Contains('/') ||
                    parameter.Name.Contains('?'))
                {
                    replacement = new Replacement<Span>(parameter.Name, null);
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
                            replacement = new Replacement<Span>(constraint.Span, text + ")");
                            return true;
                        }
                    }

                    if (HasWrongIntArgumentSyntax(constraint, "min", out var span) ||
                        HasWrongIntArgumentSyntax(constraint, "max", out span) ||
                        HasWrongIntArgumentSyntax(constraint, "minlength", out span) ||
                        HasWrongIntArgumentSyntax(constraint, "maxlength", out span))
                    {
                        replacement = new Replacement<Span>(span, null);
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
                        replacement = new Replacement<Span>(constraint.Span, null);
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
                    replacement = new Replacement<Span>(segment.Span, text + "}");
                    return true;
                }

                if (!text.StartsWith("{", StringComparison.Ordinal) &&
                    text.EndsWith("}", StringComparison.Ordinal))
                {
                    replacement = new Replacement<Span>(segment.Span, "{" + text);
                    return true;
                }
            }

            replacement = default;
            return false;

            bool HasWrongIntArgumentSyntax(RouteConstraint constraint, string methodName, out Span result)
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
                            result = constraint.Span.Substring(methodName.Length + 1, text.Length - methodName.Length - 2);
                            return true;
                        }
                    }
                }

                result = default;
                return false;
            }
        }

        private static bool HasWrongRegexSyntax(PathSegment segment, out Replacement<Span> replacement)
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

                                replacement = new Replacement<Span>(
                                    constraint.Span.Substring(6, text.Length - 7),
                                    new string(escaped.ToArray()));
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

            replacement = default;
            return false;
        }

        private static bool HasMissingMethodParameter(PathSegment segment, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out string? name)
        {
            if (segment.Parameter is { } templateParameter)
            {
                if (context.ContainingSymbol is IMethodSymbol containingMethod &&
                    !TryFindParameter(templateParameter, containingMethod, out _))
                {
                    location = templateParameter.Name.GetLocation();
                    name = templateParameter.Name.ToString();
                    return true;
                }

                if (context.ContainingSymbol is INamedTypeSymbol &&
                    context.Node.TryFirstAncestor(out ClassDeclarationSyntax? classDeclaration) &&
                    !classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    foreach (var member in classDeclaration.Members)
                    {
                        if (member is MethodDeclarationSyntax candidate &&
                            HasHttpVerbAttribute(candidate, context) &&
                            TryFindParameter(templateParameter, candidate, out _))
                        {
                            location = null;
                            name = null;
                            return false;
                        }
                    }

                    location = templateParameter.Name.GetLocation();
                    name = templateParameter.Name.ToString();
                    return true;
                }
            }

            location = null;
            name = null;
            return false;
        }

        private static bool HasInvalidName(PathSegment segment, out Replacement<Span> replacement)
        {
            if (segment.Parameter is { } parameter)
            {
                if (parameter.Name.StartsWith(" ", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.EndsWith(" ", StringComparison.OrdinalIgnoreCase))
                {
                    replacement = new Replacement<Span>(
                        parameter.Name,
                        parameter.Name.ToString().Trim());
                    return true;
                }

                if (parameter.Name.Equals("action", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("area", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("controller", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("handler", StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals("page", StringComparison.OrdinalIgnoreCase))
                {
                    replacement = new Replacement<Span>(
                        parameter.Name,
                        null);
                    return true;
                }
            }

            replacement = default;
            return false;
        }

        private static bool ShouldKebabCase(PathSegment segment, [NotNullWhen(true)] out string? kebabCase)
        {
            if (segment.Parameter == null &&
                IsHumpOrSnakeCased(segment.Span))
            {
                kebabCase = KebabCase(segment.Span.ToString());
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

        private static string KebabCase(string text)
        {
            var builder = StringBuilderPool.Borrow();
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        _ = builder.Append("-");
                    }

                    _ = builder.Append(char.ToLower(c, CultureInfo.InvariantCulture));
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

            return builder.Return();
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc3986#section-2.2.
        /// </summary>
        private static bool HasSyntaxError(PathSegment segment, [NotNullWhen(true)] out Location? location)
        {
            if (segment.Parameter == null)
            {
                for (var i = 0; i < segment.Span.Length; i++)
                {
                    switch (segment.Span[i])
                    {
                        case '?':
                            location = segment.Span.GetLocation(i, 1);
                            return true;
                    }
                }
            }

            if (segment.Span.Length == 0)
            {
                location = segment.Span.GetLocation();
                return true;
            }

            location = null;
            return false;
        }

        private static bool IsMultipleOccurringParameter(PathSegment segment, UrlAttribute urlAttribute, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out Location? location)
        {
            if (segment.Parameter is TemplateParameter parameter &&
                urlAttribute.UrlTemplate is UrlTemplate template)
            {
                if (ContainsName(template.Path))
                {
                    location = parameter.Name.GetLocation();
                    return true;
                }

                if (urlAttribute.TryGetParentMember(out var parentMember))
                {
                    if (parentMember is MethodDeclarationSyntax parentMethod &&
                        parentMethod.Parent is ClassDeclarationSyntax classDeclaration &&
                        TryGetOtherTemplate(classDeclaration.AttributeLists, out var otherTemplate) &&
                        ContainsName(otherTemplate.Path))
                    {
                        location = parameter.Name.GetLocation();
                        return true;
                    }

                    if (parentMember is ClassDeclarationSyntax parentClass)
                    {
                        foreach (var member in parentClass.Members)
                        {
                            if (member is MethodDeclarationSyntax methodDeclaration &&
                                TryGetOtherTemplate(methodDeclaration.AttributeLists, out otherTemplate) &&
                                ContainsName(otherTemplate.Path))
                            {
                                location = parameter.Name.GetLocation();
                                return true;
                            }
                        }
                    }
                }
            }

            location = null;
            return false;

            bool ContainsName(ImmutableArray<PathSegment> candidates)
            {
                return candidates.TryFirst(
                       x => x.Parameter is { } other &&
                       other != parameter &&
                       parameter.Name.TextEquals(other.Name),
                       out _);
            }

            bool TryGetOtherTemplate(SyntaxList<AttributeListSyntax> declaration, out UrlTemplate result)
            {
                foreach (var attributeList in declaration)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (UrlAttribute.TryCreate(attribute, context, out var candidate) &&
                            candidate.UrlTemplate is { } temp)
                        {
                            result = temp;
                            return true;
                        }
                    }
                }

                result = default;
                return false;
            }
        }

        private static bool ShouldUseExplicitRoute(PathSegment segment, SyntaxNodeAnalysisContext context, out Replacement<Span> replacement)
        {
            if (segment.Span.Equals("[controller]", StringComparison.OrdinalIgnoreCase) &&
                context.ContainingSymbol is INamedTypeSymbol containingType)
            {
                replacement = new Replacement<Span>(
                    segment.Span,
                    containingType.Name.EndsWith("Controller", StringComparison.Ordinal)
                        ? KebabCase(containingType.Name.Substring(0, containingType.Name.Length - 10))
                        : null);
                return true;
            }

            replacement = default;
            return false;
        }

        private static bool ShouldRenameController(PathSegment segment, UrlAttribute urlAttribute, SyntaxNodeAnalysisContext context, out Replacement<Location> replacement)
        {
            if (urlAttribute.UrlTemplate is { } template &&
                template.Path.TryLast(x => x.Parameter == null, out var last) &&
                last == segment &&
                segment.Span.Length > 0 &&
                segment.Span[0] != '[' &&
                context.ContainingSymbol is INamedTypeSymbol containingType &&
                urlAttribute.TryGetParentMember(out var parent) &&
                parent is ClassDeclarationSyntax classDeclaration)
            {
                var builder = ClassName();
                if (builder != null)
                {
                    if (Equals(builder, containingType.Name))
                    {
                        _ = builder.Clear()
                                   .Return();
                        replacement = default;
                        return false;
                    }

                    replacement = new Replacement<Location>(classDeclaration.Identifier.GetLocation(), builder.Return());
                    return true;
                }
            }

            replacement = default;
            return false;

            StringBuilderPool.PooledStringBuilder? ClassName()
            {
                var builder = StringBuilderPool.Borrow()
                                               .Append(char.ToUpper(segment.Span[0], CultureInfo.InvariantCulture));
                for (var i = 1; i < segment.Span.Length; i++)
                {
                    var c = segment.Span[i];
                    if (c == '-')
                    {
                        if (i == segment.Span.Length - 2)
                        {
                            _ = builder.Clear()
                                       .Return();
                            return null;
                        }

                        i++;
                        _ = builder.Append(char.ToUpper(segment.Span[i], CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        _ = builder.Append(c);
                    }
                }

                return builder.Append("Controller");
            }

            bool Equals(StringBuilderPool.PooledStringBuilder x, string y)
            {
                if (x.Length == y.Length)
                {
                    for (var i = 0; i < x.Length; i++)
                    {
                        if (x[i] != y[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        private static bool TryFindParameter(TemplateParameter templateParameter, IMethodSymbol method, [NotNullWhen(true)] out IParameterSymbol? result)
        {
            foreach (var candidate in method.Parameters)
            {
                if (IsFromRoute(candidate) &&
                    templateParameter.Name.Equals(candidate.Name, StringComparison.Ordinal))
                {
                    result = candidate;
                    return true;
                }
            }

            result = null;
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

        private static bool TryFindParameter(TemplateParameter templateParameter, MethodDeclarationSyntax methodDeclaration, [NotNullWhen(true)] out ParameterSyntax? result)
        {
            if (methodDeclaration.ParameterList is { } parameterList)
            {
                foreach (var candidate in parameterList.Parameters)
                {
                    if (IsFromRoute(candidate) &&
                        templateParameter.Name.Equals(candidate.Identifier.Text, StringComparison.Ordinal))
                    {
                        result = candidate;
                        return true;
                    }
                }
            }

            result = null;
            return false;

            bool IsFromRoute(ParameterSyntax p)
            {
                foreach (var attributeList in p.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (attribute.Name == KnownSymbol.FromRouteAttribute)
                        {
                            continue;
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        private static bool HasHttpVerbAttribute(MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context)
        {
            foreach (var attributeList in methodDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpDeleteAttribute, context.CancellationToken, out _) ||
                        context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpGetAttribute, context.CancellationToken, out _) ||
                        context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpHeadAttribute, context.CancellationToken, out _) ||
                        context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpOptionsAttribute, context.CancellationToken, out _) ||
                        context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpPatchAttribute, context.CancellationToken, out _) ||
                        context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpPostAttribute, context.CancellationToken, out _) ||
                        context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.HttpPutAttribute, context.CancellationToken, out _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private struct Replacement<T>
        {
            internal readonly T Node;

            internal readonly string? NewText;

            internal Replacement(T node, string? newText)
            {
                this.Node = node;
                this.NewText = newText;
            }

            internal ImmutableDictionary<string, string> Property(string key)
            {
                return this.NewText is { } value
                    ? ImmutableDictionary<string, string>.Empty.Add(key, value)
                    : ImmutableDictionary<string, string>.Empty;
            }
        }
    }
}
