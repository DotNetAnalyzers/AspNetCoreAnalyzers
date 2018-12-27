namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TemplateTextFix))]
    [Shared]
    public class TemplateTextFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            ASP002MissingParameter.DiagnosticId,
            ASP004ParameterSyntax.DiagnosticId,
            ASP006ParameterRegex.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<LiteralExpressionSyntax>(diagnostic, out _) &&
                    diagnostic.Properties.TryGetValue(nameof(Text), out var text))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            GetTitle(diagnostic),
                            _ => Fix(_),
                            equivalenceKey: null),
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

        private static string GetTitle(Diagnostic diagnostic)
        {
            switch (diagnostic.Id)
            {
                case ASP002MissingParameter.DiagnosticId:
                    return "Rename parameter";
                case ASP004ParameterSyntax.DiagnosticId:
                    return "Fix syntax error.";
                case ASP006ParameterRegex.DiagnosticId:
                    return "Escape regex.";
                default:
                    throw new InvalidOperationException("Should never get here.");
            }
        }
    }
}
