namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP004RouteParameterType
    {
        public const string DiagnosticId = "ASP004";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Route parameter type does not match.",
            messageFormat: "Route parameter type does not match.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter type does not match.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
