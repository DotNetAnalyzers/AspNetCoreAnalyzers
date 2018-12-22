namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ASP004ParameterSyntax
    {
        public const string DiagnosticId = "ASP004";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Syntax error in parameter.",
            messageFormat: "Syntax error in parameter.",
            category: AnalyzerCategory.Routing,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Syntax error in parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
