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
        private readonly LiteralExpressionSyntax literalExpression;

        public StringLiteral(LiteralExpressionSyntax literalExpression)
        {
            this.literalExpression = literalExpression;
        }

        public bool IsVerbatim
        {
            get
            {
                foreach (var c in this.literalExpression.Token.Text)
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

        public string Text => this.literalExpression.Token.Text;

        public string ValueText => this.literalExpression.Token.ValueText;

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
            var text = this.literalExpression.Token.Text;
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
                this.literalExpression.SyntaxTree,
                verbatim
                    ? new TextSpan(
                        this.literalExpression.SpanStart + start + textSpan.Start,
                        textSpan.Length)
                    : TextSpan.FromBounds(
                        this.literalExpression.SpanStart + GetIndex(textSpan.Start),
                        this.literalExpression.SpanStart + GetIndex(textSpan.End)));

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
            return this.literalExpression.Equals(other.literalExpression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is StringLiteral other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.literalExpression.GetHashCode();
        }
    }
}
