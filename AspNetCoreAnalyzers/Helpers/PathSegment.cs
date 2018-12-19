namespace AspNetCoreAnalyzers
{
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Text.Text}")]
    public struct PathSegment
    {
        public PathSegment(LiteralExpressionSyntax literal, int start, int end)
        {
            this.Text = new TextAndLocation(literal, start, end);
            this.Parameter = TemplateParameter.TryParse(this.Text, out var parameter)
                ? parameter
                : (TemplateParameter?)null;
        }

        public TextAndLocation Text { get; }

        public TemplateParameter? Parameter { get; }
    }
}
