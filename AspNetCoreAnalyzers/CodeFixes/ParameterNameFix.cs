namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Rename;

    public class ParameterNameFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            ASP001ParameterName.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ParameterSyntax parameterSyntax) &&
                    semanticModel.TryGetSymbol(parameterSyntax, context.CancellationToken, out var parameter) &&
                    diagnostic.Properties.TryGetValue(nameof(NameSyntax), out var name))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Rename parameter",
                            cancellationToken => Renamer.RenameSymbolAsync(
                                context.Document.Project.Solution,
                                parameter,
                                name,
                                null,
                                cancellationToken),
                            nameof(ParameterNameFix)),
                        diagnostic);
                }
            }
        }
    }
}
