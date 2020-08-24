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
    internal class TemplateTextFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.ASP002RouteParameterName.Id,
            Descriptors.ASP004RouteParameterType.Id,
            Descriptors.ASP005ParameterSyntax.Id,
            Descriptors.ASP006ParameterRegex.Id,
            Descriptors.ASP008ValidRouteParameterName.Id,
            Descriptors.ASP009KebabCaseUrl.Id,
            Descriptors.ASP012UseExplicitRoute.Id);

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
                        var token = literal!.Token;
                        return SyntaxFactory.Token(
                            token.LeadingTrivia,
                            token.Kind(),
                            ReplaceSpan(token.Text, literal.SpanStart),
                            ReplaceSpan(token.ValueText, literal.SpanStart + +token.Text.IndexOf('"') + 1),
                            token.TrailingTrivia);

                        string ReplaceSpan(string oldText, int offset)
                        {
                            return StringBuilderPool.Borrow()
                                                    .Append(oldText, 0, diagnostic!.Location.SourceSpan.Start - offset)
                                                    .Append(text!)
                                                    .Append(oldText, diagnostic.Location.SourceSpan.End - offset)
                                                    .Return();
                        }
                    }
                }
            }
        }

        private static string GetTitle(Diagnostic diagnostic)
        {
            if (diagnostic.Id == Descriptors.ASP002RouteParameterName.Id)
            {
                return "Rename parameter.";
            }

            if (diagnostic.Id == Descriptors.ASP004RouteParameterType.Id)
            {
                return "Change type to match symbol.";
            }

            if (diagnostic.Id == Descriptors.ASP005ParameterSyntax.Id)
            {
                return "Fix syntax error.";
            }

            if (diagnostic.Id == Descriptors.ASP006ParameterRegex.Id)
            {
                return "Escape regex.";
            }

            if (diagnostic.Id == Descriptors.ASP008ValidRouteParameterName.Id)
            {
                return "Fix name.";
            }

            if (diagnostic.Id == Descriptors.ASP009KebabCaseUrl.Id)
            {
                return "To lowercase.";
            }

            if (diagnostic.Id == Descriptors.ASP012UseExplicitRoute.Id)
            {
                return "To explicit route.";
            }

            throw new InvalidOperationException("Should never get here.");
        }
    }
}
