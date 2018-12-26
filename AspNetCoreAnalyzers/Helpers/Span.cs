namespace AspNetCoreAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public struct Span : IEquatable<Span>
    {
        public Span(StringLiteral literal, int start, int end)
        {
            this.Literal = literal;
            this.TextSpan = new TextSpan(start, end - start);
            this.Text = literal.LiteralExpression.Token.ValueText.Substring(this.TextSpan.Start, this.TextSpan.Length);
        }

        public StringLiteral Literal { get; }

        public TextSpan TextSpan { get; }

        public string Text { get; }

        public static bool operator ==(Span left, Span right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Span left, Span right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Span other)
        {
            return this.Literal.Equals(other.Literal) && this.TextSpan == other.TextSpan;
        }

        public override bool Equals(object obj)
        {
            return obj is Span other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Literal.GetHashCode();
                hashCode = (hashCode * 397) ^ this.TextSpan.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => this.Literal.LiteralExpression.Token.ValueText.Substring(this.TextSpan.Start, this.TextSpan.Length);

        public Location GetLocation() => this.Literal.GetLocation(this.TextSpan);

        public Location GetLocation(int start, int length) => this.Literal.GetLocation(new TextSpan(this.TextSpan.Start + start, length));

        internal Span Slice(int start, int end)
        {
            if (start > end)
            {
                throw new InvalidOperationException("Expected start to be less than end.");
            }

            if (end > this.TextSpan.End)
            {
                throw new InvalidOperationException("Expected end to be less than TextSpan.End.");
            }

            return new Span(this.Literal, this.TextSpan.Start + start, this.TextSpan.Start + end);
        }

        internal Span Substring(int index, int length)
        {
            return new Span(this.Literal, this.TextSpan.Start + index, this.TextSpan.Start + index + length);
        }

        internal Span Substring(int index)
        {
            return new Span(this.Literal, this.TextSpan.Start + index, this.TextSpan.Start + index + this.TextSpan.Length);
        }

        private static Location GetLocation(LiteralExpressionSyntax literal, TextSpan textSpan)
        {
            var text = literal.Token.Text;
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
                literal.SyntaxTree,
                verbatim
                    ? new TextSpan(literal.SpanStart + start + textSpan.Start, textSpan.Length)
                    : TextSpan.FromBounds(GetIndex(textSpan.Start), GetIndex(textSpan.End)));

            int GetIndex(int pos)
            {
                var index = literal.SpanStart + start;
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
