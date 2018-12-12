namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;

    public struct ParameterPair
    {
        public ParameterPair(TemplateParameter? template, IParameterSymbol method)
        {
            this.Template = template;
            this.Method = method;
        }

        public TemplateParameter? Template { get; }

        public IParameterSymbol Method { get; }
    }
}
