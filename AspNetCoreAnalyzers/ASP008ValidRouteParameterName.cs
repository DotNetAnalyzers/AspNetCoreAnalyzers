namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP008ValidRouteParameterName
    {
        public const string DiagnosticId = "ASP008";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Invalid route parameter name.",
            messageFormat: "Invalid route parameter name.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Invalid route parameter name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
