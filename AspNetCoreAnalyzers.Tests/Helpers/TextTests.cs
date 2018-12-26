namespace AspNetCoreAnalyzers.Tests.Helpers
{
    using System;
    using NUnit.Framework;

    public class TextTests
    {
        [TestCase("abc", 0, 'a', 0)]
        [TestCase("abc", 1, 'a', -1)]
        [TestCase("abc", 0, 'b', 1)]
        [TestCase("abc", 1, 'b', 1)]
        [TestCase("abc", 2, 'b', -1)]
        [TestCase("abc", 0, 'c', 2)]
        public void IndexOf(string text, int startIndex, char c, int expected)
        {
            Assert.AreEqual(expected, text.AsSpan().IndexOf(c, startIndex));
        }
    }
}
