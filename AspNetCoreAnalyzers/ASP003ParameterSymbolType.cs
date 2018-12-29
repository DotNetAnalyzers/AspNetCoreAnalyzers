namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP003ParameterSymbolType
    {
        public const string DiagnosticId = "ASP003";

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
