namespace AspNetCoreAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    public struct Span : IEquatable<Span>
    {
        public Span(StringLiteral literal, int start, int end)
        {
            this.Literal = literal;
            this.TextSpan = new TextSpan(start, end - start);
        }

        public StringLiteral Literal { get; }

        public TextSpan TextSpan { get; }

        public ReadOnlySpan<char> Text => this.Literal.LiteralExpression.Token.ValueText.AsSpan(this.TextSpan.Start, this.TextSpan.Length);

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

        public bool Equals(string text, StringComparison stringComparison) => this.Text.Equals(text.AsSpan(), stringComparison);

        public bool StartsWith(string text, StringComparison stringComparison) => this.Text.StartsWith(text.AsSpan(), stringComparison);

        public bool EndsWith(string text, StringComparison stringComparison) => this.Text.EndsWith(text.AsSpan(), stringComparison);

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

        public override string ToString() => new string(this.Text.ToArray());

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
    }
}
