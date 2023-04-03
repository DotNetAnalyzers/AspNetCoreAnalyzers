﻿namespace AspNetCoreAnalyzers;

using System;
using System.Diagnostics;

[DebuggerDisplay("{this.Span.ToString()}")]
internal struct PathSegment : IEquatable<PathSegment>
{
    internal PathSegment(StringLiteral literal, int start, int end)
    {
        this.Span = new Span(literal, start, end);
        this.Parameter = TemplateParameter.TryParse(this.Span, out var parameter)
            ? parameter
            : (TemplateParameter?)null;
    }

    internal Span Span { get; }

    internal TemplateParameter? Parameter { get; }

    public static bool operator ==(PathSegment left, PathSegment right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PathSegment left, PathSegment right)
    {
        return !left.Equals(right);
    }

    public bool Equals(PathSegment other)
    {
        return this.Span.Equals(other.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is PathSegment other &&
               this.Equals(other);
    }

    public override int GetHashCode()
    {
        return this.Span.GetHashCode();
    }

    internal static bool TryRead(StringLiteral literal, int start, out PathSegment segment)
    {
        // https://tools.ietf.org/html/rfc3986
        var text = literal.ValueText;
        var pos = start;
        if (TrySkipStart() &&
            pos < text.Length)
        {
            if (text[pos] == '{')
            {
                if (text.TryIndexOf("}/", pos, out var end))
                {
                    segment = new PathSegment(literal, start, end + 1);
                    return true;
                }

                segment = new PathSegment(literal, start, text.Length);
                return true;
            }
            else if (text.TryIndexOf("/", pos, out var end))
            {
                segment = new PathSegment(literal, start, end);
                return true;
            }

            segment = new PathSegment(literal, start, text.Length);
            return true;
        }

        segment = default;
        return false;

        bool TrySkipStart()
        {
            if (pos >= text.Length)
            {
                return false;
            }

            if (pos == 0)
            {
                if (text.StartsWith("~/", StringComparison.Ordinal))
                {
                    pos += 2;
                    start = 2;
                }
                else if (text.StartsWith("~", StringComparison.Ordinal) ||
                         text.StartsWith("/", StringComparison.Ordinal))
                {
                    pos++;
                    start++;
                }

                return true;
            }

            if (text[pos] == '/')
            {
                pos++;
                start++;
                return true;
            }

            return false;
        }
    }
}
