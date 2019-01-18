namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP011RouteParameterNameMustBeUnique
    {
        public const string DiagnosticId = "ASP011";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Route parameter appears more than once.",
            messageFormat: "Route parameter appears more than once.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter appears more than once.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
