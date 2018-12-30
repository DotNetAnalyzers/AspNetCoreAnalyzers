namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP001ParameterSymbolName
    {
        public const string DiagnosticId = "ASP001";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Parameter name does not match the name specified by the route parameter.",
            messageFormat: "Parameter name does not match the name specified by the route parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter name does not match the name specified by the route parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
