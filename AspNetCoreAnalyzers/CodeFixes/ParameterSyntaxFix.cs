namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;

    public class ParameterSyntaxFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            ASP004ParameterSyntax.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {

            }
        }
    }
}