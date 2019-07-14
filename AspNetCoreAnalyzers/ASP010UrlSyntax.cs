namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP010UrlSyntax
    {
        public const string DiagnosticId = "ASP010";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Unexpected character in url.",
            messageFormat: "Literal sections cannot contain the '{0}' character",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Unexpected character in url.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
