namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public struct TextAndLocation
    {
        private readonly LiteralExpressionSyntax literal;
        private readonly int start;
        private readonly int end;

        public TextAndLocation(LiteralExpressionSyntax literal, int start, int end)
        {
            this.literal = literal;
            this.start = start;
            this.end = end;
            this.Text = literal.Token.ValueText.Substring(start, end - start);
        }

        public string Text { get; }

        public Location Location => Location.Create(this.literal.SyntaxTree, TextSpan.FromBounds(this.literal.SpanStart + this.start, this.literal.SpanStart + this.end));

        public static bool operator ==(TextAndLocation left, TextAndLocation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextAndLocation left, TextAndLocation right)
        {
            return !left.Equals(right);
        }

        public bool Equals(TextAndLocation other)
        {
            return this.literal.Equals(other.literal) && this.start == other.start && this.end == other.end;
        }

        public override bool Equals(object obj)
        {
            return obj is TextAndLocation other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.literal.GetHashCode();
                hashCode = (hashCode * 397) ^ this.start;
                hashCode = (hashCode * 397) ^ this.end;
                return hashCode;
            }
        }

        internal TextAndLocation Substring(int index, int length)
        {
            return new TextAndLocation(this.literal, this.start + index, this.start + index + length);
        }
    }
}
