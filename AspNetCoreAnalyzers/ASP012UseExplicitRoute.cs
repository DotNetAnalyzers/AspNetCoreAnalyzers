namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    public static class ASP012UseExplicitRoute
    {
        public const string DiagnosticId = "ASP012";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't use [controller].",
            messageFormat: "Don't use [controller].",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use [controller]. Prefer explicit string so that renaming the class is not a breaking change.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
