namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2#regular-expressions.
    /// </summary>
    public static class ASP006ParameterRegex
    {
        public const string DiagnosticId = "ASP006";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Escape constraint regex.",
            messageFormat: "Escape constraint regex.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Escape constraint regex.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
