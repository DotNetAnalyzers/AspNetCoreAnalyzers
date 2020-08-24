namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [DebuggerDisplay("{this.Text}")]
    public struct StringLiteral : IEquatable<StringLiteral>
    {
        public StringLiteral(LiteralExpressionSyntax literalExpression)
        {
            this.LiteralExpression = literalExpression;
        }

        public LiteralExpressionSyntax LiteralExpression { get; }

        public string Text => this.LiteralExpression.Token.Text;

        public string ValueText => this.LiteralExpression.Token.ValueText;

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

        public static bool operator ==(StringLiteral left, StringLiteral right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringLiteral left, StringLiteral right)
        {
            return !left.Equals(right);
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

            return Location.Create(
                this.LiteralExpression.SyntaxTree,
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

        public bool Equals(StringLiteral other)
        {
            return this.LiteralExpression.Equals(other.LiteralExpression);
        }

        public override bool Equals(object? obj)
        {
            return obj is StringLiteral other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.LiteralExpression.GetHashCode();
        }

        public string ToString(Location location) => location.SourceSpan.Length == 0
            ? string.Empty
            : this.Text.Substring(location.SourceSpan.Start - this.LiteralExpression.SpanStart, location.SourceSpan.Length);
    }
}
