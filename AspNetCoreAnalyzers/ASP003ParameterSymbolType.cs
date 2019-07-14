namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP003ParameterSymbolType
    {
        public const string DiagnosticId = "ASP003";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Parameter type does not match the type specified by the name specified by the route parameter.",
            messageFormat: "Parameter type does not match the type specified by the name specified by the route parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter type does not match the type specified by the name specified by the route parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
