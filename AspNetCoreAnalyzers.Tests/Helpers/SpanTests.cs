namespace AspNetCoreAnalyzers.Tests.Helpers
{
    using System;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class SpanTests
    {
        [TestCase("\"abc\"", "abc",  true)]
        [TestCase("\"abc\"", "ab",   false)]
        [TestCase("\"abc\"", "bc",   false)]
        [TestCase("\"abc\"", "a",    false)]
        [TestCase("\"abc\"", "abcd", false)]
        public void Equals(string text, string value, bool expected)
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
        public void StartsWith(string text, string value, bool expected)
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
        [TestCase("\"abc\"", "bc",   false)]
        [TestCase("\"abc\"", "dabc", false)]
        public void EndsWith(string text, string value, bool expected)
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
    }
}
