namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP009LowerCaseUrl
    {
        public const string DiagnosticId = "ASP009";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use lowercase urls.",
            messageFormat: "Use lowercase urls.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use lowercase urls.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
