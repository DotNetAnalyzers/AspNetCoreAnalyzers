namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP004RouteParameterType
    {
        public const string DiagnosticId = "ASP004";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Route parameter type does not match the method parameter type.",
            messageFormat: "Route parameter type does not match the method parameter type.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter type does not match the method parameter type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
