namespace AspNetCoreAnalyzers
{
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;

    [DebuggerDisplay("{this.FromTemplate?.Name ?? this.FromMethodSymbol.Name}")]
    public struct ParameterPair
    {
        public ParameterPair(TemplateParameter? fromTemplate, IParameterSymbol fromMethodSymbol)
        {
            this.FromTemplate = fromTemplate;
            this.FromMethodSymbol = fromMethodSymbol;
        }

        public TemplateParameter? FromTemplate { get; }

        public IParameterSymbol FromMethodSymbol { get; }
    }
}
