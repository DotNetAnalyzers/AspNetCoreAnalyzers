namespace AspNetCoreAnalyzers.Tests.Helpers;

using System;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static class SpanTests
{
    [TestCase("\"abc\"", "abc",  true)]
    [TestCase("\"abc\"", "ab",   false)]
    [TestCase("\"abc\"", "bc",   false)]
    [TestCase("\"abc\"", "a",    false)]
    [TestCase("\"abc\"", "abcd", false)]
    public static void Equals(string text, string value, bool expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""abc"")]
    class C
    {
        
    }
}".AssertReplace("\"abc\"", text));
        var literal = syntaxTree.FindLiteralExpression(text);
        var span = new Span(new StringLiteral(literal), 0, 3);
        Assert.AreEqual(expected, span.Equals(value, StringComparison.Ordinal));
    }

    [TestCase("\"abc\"", "abc",  true)]
    [TestCase("\"abc\"", "ab",   true)]
    [TestCase("\"abc\"", "a",    true)]
    [TestCase("\"abc\"", "bc",   false)]
    [TestCase("\"abc\"", "abcd", false)]
    public static void StartsWith(string text, string value, bool expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""abc"")]
    class C
    {
        
    }
}".AssertReplace("\"abc\"", text));
        var literal = syntaxTree.FindLiteralExpression(text);
        var span = new Span(new StringLiteral(literal), 0, 3);
        Assert.AreEqual(expected, span.StartsWith(value, StringComparison.Ordinal));
    }

    [TestCase("\"abc\"", "abc",  true)]
    [TestCase("\"abc\"", "bc",   true)]
    [TestCase("\"abc\"", "c",    true)]
    [TestCase("\"abc\"", "ab",   false)]
    [TestCase("\"abc\"", "dabc", false)]
    public static void EndsWith(string text, string value, bool expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""abc"")]
    class C
    {
        
    }
}".AssertReplace("\"abc\"", text));
        var literal = syntaxTree.FindLiteralExpression(text);
        var span = new Span(new StringLiteral(literal), 0, 3);
        Assert.AreEqual(expected, span.EndsWith(value, StringComparison.Ordinal));
    }

    [TestCase("\"abc\"", "abc",  0)]
    [TestCase("\"abc\"", "bc",   1)]
    [TestCase("\"abc\"", "c",    2)]
    [TestCase("\"abc\"", "ab",   0)]
    [TestCase("\"abc\"", "e",    -1)]
    [TestCase("\"abc\"", "dabc", -1)]
    public static void IndexOf(string text, string value, int expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""abc"")]
    class C
    {
        
    }
}".AssertReplace("\"abc\"", text));
        var literal = syntaxTree.FindLiteralExpression(text);
        var span = new Span(new StringLiteral(literal), 0, 3);
        Assert.AreEqual(expected, span.IndexOf(value));
        if (expected >= 0)
        {
            Assert.AreEqual(true,     span.TryIndexOf(value, 0, out var index));
            Assert.AreEqual(expected, index);
        }
        else
        {
            Assert.AreEqual(false, span.TryIndexOf(value, 0, out _));
        }
    }

    [TestCase("\"123abc\"", "abc",  0)]
    [TestCase("\"123abc\"", "bc",   1)]
    [TestCase("\"123abc\"", "c",    2)]
    [TestCase("\"123abc\"", "ab",   0)]
    [TestCase("\"123abc\"", "e",    -4)]
    [TestCase("\"123abc\"", "dabc", -4)]
    public static void IndexOfWhenOffset(string text, string value, int expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""123abc"")]
    class C
    {
        
    }
}".AssertReplace("\"123abc\"", text));
        var literal = syntaxTree.FindLiteralExpression(text);
        var span = new Span(new StringLiteral(literal), 3, 3);
        Assert.AreEqual(expected, span.IndexOf(value));
        if (expected >= 0)
        {
            Assert.AreEqual(true,     span.TryIndexOf(value, 0, out var index));
            Assert.AreEqual(expected, index);
        }
        else
        {
            Assert.AreEqual(false, span.TryIndexOf(value, 0, out _));
        }
    }
}
