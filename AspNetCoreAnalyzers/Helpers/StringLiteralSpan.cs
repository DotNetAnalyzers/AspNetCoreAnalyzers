namespace AspNetCoreAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    public struct StringLiteralSpan : IEquatable<StringLiteralSpan>
    {
        public StringLiteralSpan(StringLiteral literal, int start, int end)
        {
            this.Literal = literal;
            this.TextSpan = new TextSpan(start, end - start);
            this.Text = literal.LiteralExpression.Token.ValueText.Substring(this.TextSpan.Start, this.TextSpan.Length);
        }

        public StringLiteral Literal { get; }

        public TextSpan TextSpan { get; }

        public string Text { get; }

        public static bool operator ==(StringLiteralSpan left, StringLiteralSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringLiteralSpan left, StringLiteralSpan right)
        {
            return !left.Equals(right);
        }

        public bool Equals(StringLiteralSpan other)
        {
            return this.Literal.Equals(other.Literal) && this.TextSpan == other.TextSpan;
        }

        public override bool Equals(object obj)
        {
            return obj is StringLiteralSpan other &&
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

        internal StringLiteralSpan Slice(int start, int end)
        {
            if (start > end)
            {
                throw new InvalidOperationException("Expected start to be less than end.");
            }

            if (end > this.TextSpan.End)
            {
                throw new InvalidOperationException("Expected end to be less than TextSpan.End.");
            }

            return new StringLiteralSpan(this.Literal, this.TextSpan.Start + start, this.TextSpan.Start + end);
        }

        internal StringLiteralSpan Substring(int index, int length)
        {
            return new StringLiteralSpan(this.Literal, this.TextSpan.Start + index, this.TextSpan.Start + index + length);
        }

        internal StringLiteralSpan Substring(int index)
        {
            return new StringLiteralSpan(this.Literal, this.TextSpan.Start + index, this.TextSpan.Start + index + this.TextSpan.Length);
        }
    }
}
