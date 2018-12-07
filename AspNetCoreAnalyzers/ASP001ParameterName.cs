namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP001ParameterName
    {
        public const string DiagnosticId = "ASP001";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The parameter name does not match the url parameter.",
            messageFormat: "The parameter name does not match the url parameter.",
            category: AnalyzerCategory.Correctness,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "The parameter name does not match the url parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
