namespace AspNetCoreAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParameterTypeFix))]
    [Shared]
    public class ParameterTypeFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            ASP003ParameterSymbolType.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out TypeSyntax typeSyntax) &&
                    diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeName))
                {
                    context.RegisterCodeFix(
                        $"Change to {typeName}",
                        (e, _) => e.ReplaceNode(
                            typeSyntax,
                            x => SyntaxFactory.ParseTypeName(typeName)
                                              .WithSimplifiedNames()
                                              .WithTriviaFrom(x)),
                        nameof(ParameterTypeFix),
                        diagnostic);
                }
            }
        }
    }
}
