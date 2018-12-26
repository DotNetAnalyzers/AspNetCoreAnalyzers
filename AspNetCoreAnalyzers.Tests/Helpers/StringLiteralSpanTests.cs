namespace AspNetCoreAnalyzers.Tests.Helpers
{
    using System;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class StringLiteralSpanTests
    {
        [TestCase("\"abc\"", "abc")]
        public void WhenEqual(string text, string expected)
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
            var span = new StringLiteralSpan(new StringLiteral(literal), 0, 3);
            Assert.AreEqual(true, span.StartsWith(expected, StringComparison.Ordinal));
            Assert.AreEqual(true, span.EndsWith(expected, StringComparison.Ordinal));
            Assert.AreEqual(true, span.Equals(expected, StringComparison.Ordinal));
        }
    }
}
