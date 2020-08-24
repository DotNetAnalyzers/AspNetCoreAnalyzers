namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [DebuggerDisplay("{this.Text}")]
    internal struct StringLiteral : IEquatable<StringLiteral>
    {
        internal StringLiteral(LiteralExpressionSyntax literalExpression)
        {
            this.LiteralExpression = literalExpression;
        }

        internal LiteralExpressionSyntax LiteralExpression { get; }

        internal string Text => this.LiteralExpression.Token.Text;

        internal string ValueText => this.LiteralExpression.Token.ValueText;

        internal bool IsVerbatim
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

        internal Location GetLocation(TextSpan textSpan)
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
#pragma warning disable SA1118 // Parameter should not span multiple lines
                verbatim
                    ? new TextSpan(
                        this.LiteralExpression.SpanStart + start + textSpan.Start,
                        textSpan.Length)
                    : TextSpan.FromBounds(
                        this.LiteralExpression.SpanStart + GetIndex(textSpan.Start),
                        this.LiteralExpression.SpanStart + GetIndex(textSpan.End)));
#pragma warning restore SA1118 // Parameter should not span multiple lines

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

        internal string ToString(Location location) => location.SourceSpan.Length == 0
            ? string.Empty
            : this.Text.Substring(location.SourceSpan.Start - this.LiteralExpression.SpanStart, location.SourceSpan.Length);
    }
}
