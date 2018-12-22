namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class ParameterSyntaxFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            ASP004ParameterSyntax.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out LiteralExpressionSyntax literal) &&
                    diagnostic.Properties.TryGetValue(nameof(Text), out var text))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Fix syntax error.",
                            _ => Fix(_)),
                        diagnostic);

                    async Task<Document> Fix(CancellationToken cancellationToken)
                    {
                        var sourceText = await context.Document.GetTextAsync(cancellationToken)
                                                      .ConfigureAwait(false);
                        return context.Document.WithText(sourceText.Replace(diagnostic.Location.SourceSpan, text));
                    }
                }
            }
        }
    }
}
