namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP013ControllerNameShouldMatchRoute
    {
        public const string DiagnosticId = "ASP013";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name the controller to match the route.",
            messageFormat: "Name the controller to match the route. Expected: '{0}'.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name the controller to match the route.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
