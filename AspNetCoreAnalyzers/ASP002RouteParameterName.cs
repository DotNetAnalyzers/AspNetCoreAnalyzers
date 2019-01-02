namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP002RouteParameterName
    {
        public const string DiagnosticId = "ASP002";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Route parameter name does not match the method parameter name.",
            messageFormat: "Route parameter name does not match the method parameter name.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Route parameter name does not match the method parameter name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
