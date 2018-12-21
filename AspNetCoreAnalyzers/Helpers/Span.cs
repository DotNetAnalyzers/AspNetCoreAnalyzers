namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [DebuggerDisplay("{this.Text}")]
    public struct Span : IEquatable<Span>
    {
        private readonly LiteralExpressionSyntax literal;

        public Span(LiteralExpressionSyntax literal, int start, int end)
        {
            this.literal = literal;
            this.TextSpan = new TextSpan(start, end - start);
            this.Text = literal.Token.ValueText.Substring(start, end - start);
        }

        public TextSpan TextSpan { get; }

        public string Text { get; }

        public Location Location => Location.Create(this.literal.SyntaxTree, this.TextSpan);

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
            return this.literal.Equals(other.literal) && this.TextSpan == other.TextSpan;
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
                var hashCode = this.literal.GetHashCode();
                hashCode = (hashCode * 397) ^ this.TextSpan.GetHashCode();
                return hashCode;
            }
        }

        internal Span Slice(int start, int end)
        {
            if (start >= end)
            {
                throw new InvalidOperationException("Expected start to be less than end.");
            }

            if (end > this.TextSpan.End)
            {
                throw new InvalidOperationException("Expected end to be less than TextSpan.End.");
            }

            return new Span(this.literal, this.TextSpan.Start + start, this.TextSpan.Start + end);
        }

        internal Span Substring(int index, int length)
        {
            return new Span(this.literal, this.TextSpan.Start + index, this.TextSpan.Start + index + length);
        }

        internal Span Substring(int index)
        {
            return new Span(this.literal, this.TextSpan.Start + index, this.TextSpan.Start + index + this.TextSpan.Length);
        }
    }
}
