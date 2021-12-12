namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Rename;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameParameterFix))]
    [Shared]
    internal class RenameParameterFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.ASP001ParameterSymbolName.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ParameterSyntax? parameterSyntax) &&
                    semanticModel is { } &&
                    semanticModel.TryGetSymbol(parameterSyntax, context.CancellationToken, out var parameter) &&
                    diagnostic.Properties.TryGetValue(nameof(NameSyntax), out var name))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Rename parameter",
                            cancellationToken => Renamer.RenameSymbolAsync(
                                context.Document.Project.Solution,
                                parameter,
                                name!,
                                context.Document.Project.Solution.Options,
                                cancellationToken),
                            nameof(RenameParameterFix)),
                        diagnostic);
                }
            }
        }
    }
}
