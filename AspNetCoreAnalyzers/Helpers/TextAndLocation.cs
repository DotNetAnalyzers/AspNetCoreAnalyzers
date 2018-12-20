namespace AspNetCoreAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public struct TextAndLocation
    {
        private readonly LiteralExpressionSyntax literal;

        public TextAndLocation(LiteralExpressionSyntax literal, int start, int end)
        {
            this.literal = literal;
            this.Span = new TextSpan(start, end - start);
            this.Text = literal.Token.ValueText.Substring(start, end - start);
        }

        public TextSpan Span { get; }

        public string Text { get; }

        public Location Location => Location.Create(this.literal.SyntaxTree, this.Span);

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
            return this.literal.Equals(other.literal) && this.Span == other.Span;
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
                hashCode = (hashCode * 397) ^ this.Span.GetHashCode();
                return hashCode;
            }
        }

        internal TextAndLocation Substring(int index, int length)
        {
            return new TextAndLocation(this.literal, this.Span.Start + index, this.Span.Start + index + length);
        }
    }
}
