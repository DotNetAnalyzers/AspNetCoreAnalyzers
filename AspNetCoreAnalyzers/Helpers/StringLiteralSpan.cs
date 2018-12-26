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
        }

        public StringLiteral Literal { get; }

        public TextSpan TextSpan { get; }

        public int Length => this.TextSpan.Length;

        public char this[int index] => this.Literal.ValueText[this.TextSpan.Start + index];

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

        public override string ToString() => this.TextSpan.Length == 0
            ? string.Empty
            : this.Literal.ValueText.Substring(this.TextSpan.Start, this.TextSpan.Length);

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

        internal int IndexOf(char value, int startIndex = 0)
        {
            var valueText = this.Literal.ValueText;
            for (var i = startIndex; i < this.TextSpan.Length; i++)
            {
                if (valueText[this.TextSpan.Start + i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        internal int LastIndexOf(char value)
        {
            var valueText = this.Literal.ValueText;
            for (var i = this.TextSpan.Length - 1; i >= 0; i--)
            {
                if (valueText[this.TextSpan.Start + i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        internal bool Equals(string value, StringComparison comparisonType)
        {
            return this.Length == value.Length &&
                   this.StartsWith(value, comparisonType);
        }

        internal bool StartsWith(string value, StringComparison comparisonType)
        {
            return this.Literal.ValueText.IndexOf(value, this.TextSpan.Start, comparisonType) == this.TextSpan.Start;
        }

        internal bool EndsWith(string value, StringComparison comparisonType)
        {
            var start = this.TextSpan.End - value.Length;
            return this.Literal.ValueText.IndexOf(value, start, comparisonType) == start;
        }
    }
}
