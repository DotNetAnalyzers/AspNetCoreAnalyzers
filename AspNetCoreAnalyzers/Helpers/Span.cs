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

        public int Length => this.TextSpan.Length;

        public char this[int index] => this.Literal.ValueText[this.TextSpan.Start + index];

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

        public override string ToString() => this.TextSpan.Length == 0
            ? string.Empty
            : this.Literal.ValueText.Substring(this.TextSpan.Start, this.TextSpan.Length);

        public string ToString(Location location) => this.Literal.ToString(location);

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

        internal bool TryIndexOf(char value, out int index)
        {
            index = this.IndexOf(value, 0);
            return index >= 0;
        }

        internal bool TryIndexOf(char value, int startIndex, out int index)
        {
            index = this.IndexOf(value, startIndex);
            return index >= startIndex;
        }

        internal bool TryLastIndexOf(char value, out int index)
        {
            index = this.LastIndexOf(value);
            return index >= 0;
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
            return this.Length >= value.Length &&
                   this.Literal.ValueText.IndexOf(value, this.TextSpan.Start, value.Length, comparisonType) == this.TextSpan.Start;
        }

        internal bool EndsWith(string value, StringComparison comparisonType)
        {
            if (this.Length >= value.Length)
            {
                var start = this.TextSpan.End - value.Length;
                return this.Literal.ValueText.IndexOf(value, start, value.Length, comparisonType) == start;
            }

            return false;
        }
    }
}
