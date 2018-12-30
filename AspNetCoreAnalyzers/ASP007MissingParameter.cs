namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP007MissingParameter
    {
        public const string DiagnosticId = "ASP002";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The method has no corresponding parameter.",
            messageFormat: "The method has no corresponding parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method has no corresponding parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
