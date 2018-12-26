namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public struct StringLiteral
    {
        public StringLiteral(LiteralExpressionSyntax literalExpression)
        {
            this.LiteralExpression = literalExpression;
        }

        public LiteralExpressionSyntax LiteralExpression { get; }

        public bool IsVerbatim
        {
            get
            {
                foreach (var c in this.LiteralExpression.Token.Text)
                {
                    switch (c)
                    {
                        case '"':
                            return false;
                        case '@':
                            return true;
                    }
                }

                return false;
            }
        }

        public Location GetLocation(TextSpan textSpan)
        {
            var text = this.LiteralExpression.Token.Text;
            var start = 0;
            var verbatim = false;
            while (start < 3)
            {
                if (text[start] == '"')
                {
                    start++;
                    break;
                }

                if (text[start] == '@')
                {
                    verbatim = true;
                }

                start++;
            }

            return Location.Create(this.LiteralExpression.SyntaxTree,
                                   verbatim
                                       ? new TextSpan(
                                           this.LiteralExpression.SpanStart + start + textSpan.Start,
                                           textSpan.Length)
                                       : TextSpan.FromBounds(
                                           this.LiteralExpression.SpanStart + GetIndex(textSpan.Start),
                                           this.LiteralExpression.SpanStart + GetIndex(textSpan.End)));

            int GetIndex(int pos)
            {
                var index = start;
                for (var i = start; i < pos + start; i++)
                {
                    index++;
                    if (text[i] == '\\' &&
                        text[i + 1] == '\\')
                    {
                        i++;
                        index += 2;
                    }
                }

                return index;
            }
        }
    }
}
