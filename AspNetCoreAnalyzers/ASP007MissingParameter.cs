namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class ASP007MissingParameter
    {
        public const string DiagnosticId = "ASP007";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The method has no corresponding parameter.",
            messageFormat: "The route template has parameter '{0}' that does not have a corresponding method parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The method has no corresponding parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
