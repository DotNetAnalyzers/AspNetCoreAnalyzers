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
            this.Start = start;
            this.End = end;
            this.Text = literal.Token.ValueText.Substring(start, end - start);
        }

        public int Start { get; }

        public int End { get; }

        public string Text { get; }

        public Location Location => Location.Create(this.literal.SyntaxTree, TextSpan.FromBounds(this.literal.SpanStart + this.Start, this.literal.SpanStart + this.End));

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
            return this.literal.Equals(other.literal) && this.Start == other.Start && this.End == other.End;
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
                hashCode = (hashCode * 397) ^ this.Start;
                hashCode = (hashCode * 397) ^ this.End;
                return hashCode;
            }
        }

        internal TextAndLocation Substring(int index, int length)
        {
            return new TextAndLocation(this.literal, this.Start + index, this.Start + index + length);
        }
    }
}
