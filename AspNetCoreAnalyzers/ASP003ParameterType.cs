namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP003ParameterType
    {
        public const string DiagnosticId = "ASP003";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Parameter type does not match template.",
            messageFormat: "Parameter type does not match template.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "Parameter type does not match template.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}