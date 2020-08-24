namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class Descriptors
    {
        internal static readonly DiagnosticDescriptor ASP001ParameterSymbolName = Create(
            id: "ASP001",
            title: "Parameter name does not match the name specified by the route parameter.",
            messageFormat: "Parameter name does not match the name specified by the route parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter name does not match the name specified by the route parameter.");

        internal static readonly DiagnosticDescriptor ASP002RouteParameterName = Descriptors.Create(
            id: "ASP002",
            title: "Route parameter name does not match the method parameter name.",
            messageFormat: "Route parameter name does not match the method parameter name.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter name does not match the method parameter name.");

        internal static readonly DiagnosticDescriptor ASP003ParameterSymbolType = Descriptors.Create(
            id: "ASP003",
            title: "Parameter type does not match the type specified by the name specified by the route parameter.",
            messageFormat: "Parameter type does not match the type specified by the name specified by the route parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter type does not match the type specified by the name specified by the route parameter.");

        internal static readonly DiagnosticDescriptor ASP004RouteParameterType = Descriptors.Create(
            id: "ASP004",
            title: "Route parameter type does not match the method parameter type.",
            messageFormat: "Route parameter type does not match the method parameter type.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter type does not match the method parameter type.");

        internal static readonly DiagnosticDescriptor ASP005ParameterSyntax = Descriptors.Create(
            id: "ASP005",
            title: "Syntax error in parameter.",
            messageFormat: "Syntax error in parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Syntax error in parameter.");

        /// <summary>
        /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#regular-expressions.
        /// </summary>
        internal static readonly DiagnosticDescriptor ASP006ParameterRegex = Descriptors.Create(
            id: "ASP006",
            title: "Escape constraint regex.",
            messageFormat: "Escape constraint regex.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Escape constraint regex.");

        internal static readonly DiagnosticDescriptor ASP007MissingParameter = Descriptors.Create(
            id: "ASP007",
            title: "The method has no corresponding parameter.",
            messageFormat: "The route template has parameter '{0}' that does not have a corresponding method parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method has no corresponding parameter.");

        internal static readonly DiagnosticDescriptor ASP008ValidRouteParameterName = Descriptors.Create(
            id: "ASP008",
            title: "Invalid route parameter name.",
            messageFormat: "Invalid route parameter name.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Invalid route parameter name.");

        internal static readonly DiagnosticDescriptor ASP009KebabCaseUrl = Descriptors.Create(
            id: "ASP009",
            title: "Use kebab-cased urls.",
            messageFormat: "Use kebab-cased urls.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use kebab-cased urls.");

        internal static readonly DiagnosticDescriptor ASP010UrlSyntax = Descriptors.Create(
            id: "ASP010",
            title: "Unexpected character in url.",
            messageFormat: "Literal sections cannot contain the '{0}' character",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Unexpected character in url.");

        internal static readonly DiagnosticDescriptor ASP011RouteParameterNameMustBeUnique = Descriptors.Create(
            id: "ASP011",
            title: "Route parameter appears more than once.",
            messageFormat: "Route parameter appears more than once.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter appears more than once.");

        internal static readonly DiagnosticDescriptor ASP012UseExplicitRoute = Descriptors.Create(
            id: "ASP012",
            title: "Don't use [controller].",
            messageFormat: "Don't use [controller].",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use [controller]. Prefer explicit string so that renaming the class is not a breaking change.");

        internal static readonly DiagnosticDescriptor ASP013ControllerNameShouldMatchRoute = Descriptors.Create(
            id: "ASP013",
            title: "Name the controller to match the route.",
            messageFormat: "Name the controller to match the route. Expected: '{0}'.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name the controller to match the route.");

        /// <summary>
        /// Create a DiagnosticDescriptor, which provides description about a <see cref="Diagnostic" />.
        /// </summary>
        /// <param name="id">A unique identifier for the diagnostic. For example, code analysis diagnostic ID "CA1001".</param>
        /// <param name="title">A short title describing the diagnostic. For example, for CA1001: "Types that own disposable fields should be disposable".</param>
        /// <param name="messageFormat">A format message string, which can be passed as the first argument to <see cref="string.Format(string,object[])" /> when creating the diagnostic message with this descriptor.
        /// For example, for CA1001: "Implement IDisposable on '{0}' because it creates members of the following IDisposable types: '{1}'.</param>
        /// <param name="category">The category of the diagnostic (like Design, Naming etc.). For example, for CA1001: "Microsoft.Design".</param>
        /// <param name="defaultSeverity">Default severity of the diagnostic.</param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default.</param>
        /// <param name="description">An optional longer description of the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See <see cref="WellKnownDiagnosticTags" /> for some well known tags.</param>
        internal static DiagnosticDescriptor Create(
          string id,
          string title,
          string messageFormat,
          string category,
          DiagnosticSeverity defaultSeverity,
          bool isEnabledByDefault,
          string? description = null,
          params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: isEnabledByDefault,
                description: description,
                helpLinkUri: $"https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/{id}.md",
                customTags: customTags);
        }
    }
}
