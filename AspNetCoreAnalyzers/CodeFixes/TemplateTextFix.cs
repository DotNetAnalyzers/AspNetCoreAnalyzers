namespace AspNetCoreAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TemplateTextFix))]
    [Shared]
    public class TemplateTextFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            ASP002RouteParameterName.DiagnosticId,
            ASP004RouteParameterType.DiagnosticId,
            ASP005ParameterSyntax.DiagnosticId,
            ASP006ParameterRegex.DiagnosticId,
            ASP008ValidRouteParameterName.DiagnosticId,
            ASP009KebabCaseUrl.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<LiteralExpressionSyntax>(diagnostic, out var literal) &&
                    diagnostic.Properties.TryGetValue(nameof(UrlTemplate), out var text))
                {
                    context.RegisterCodeFix(
                        GetTitle(diagnostic),
                        (editor, _) => editor.ReplaceToken(
                            literal.Token,
                            WithValueText()),
                        nameof(TemplateTextFix),
                        diagnostic);

                    SyntaxToken WithValueText()
                    {
                        var token = literal.Token;
                        return SyntaxFactory.Token(
                            token.LeadingTrivia,
                            token.Kind(),
                            ReplaceSpan(token.Text, literal.SpanStart),
                            ReplaceSpan(token.ValueText, literal.SpanStart + +token.Text.IndexOf('"') + 1),
                            token.TrailingTrivia);

                        string ReplaceSpan(string oldText, int offset)
                        {
                            return StringBuilderPool.Borrow()
                                                    .Append(oldText, 0, diagnostic.Location.SourceSpan.Start - offset)
                                                    .Append(text)
                                                    .Append(oldText, diagnostic.Location.SourceSpan.End - offset)
                                                    .Return();
                        }
                    }
                }
            }
        }

        private static string GetTitle(Diagnostic diagnostic)
        {
            switch (diagnostic.Id)
            {
                case ASP002RouteParameterName.DiagnosticId:
                    return "Rename parameter.";
                case ASP004RouteParameterType.DiagnosticId:
                    return "Change type to match symbol.";
                case ASP005ParameterSyntax.DiagnosticId:
                    return "Fix syntax error.";
                case ASP006ParameterRegex.DiagnosticId:
                    return "Escape regex.";
                case ASP008ValidRouteParameterName.DiagnosticId:
                    return "Fix name.";
                case ASP009KebabCaseUrl.DiagnosticId:
                    return "To lowercase.";
                default:
                    throw new InvalidOperationException("Should never get here.");
            }
        }
    }
}
