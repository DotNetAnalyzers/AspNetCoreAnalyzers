namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP004RouteParameterType
    {
        public const string DiagnosticId = "ASP004";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Parameter type does not match.",
            messageFormat: "Parameter type does not match.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter type does not match.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}