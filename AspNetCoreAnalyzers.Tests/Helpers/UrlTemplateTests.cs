namespace AspNetCoreAnalyzers.Tests.Helpers
{
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class UrlTemplateTests
    {
        [TestCase("{id}/info", new[] { "{id}", "info" })]
        [TestCase("api/orders/{id}", new[] { "api", "orders", "{id}" })]
        [TestCase("api/orders/{id}/info", new[] { "api", "orders", "{id}", "info" })]
        public void TryParse(string text, string[] expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace ValidCode
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public async Task<IActionResult> GetOrder([FromRoute]int id)
        {
        }
    }
}".AssertReplace("api/orders/{id}", text));
            var literal = syntaxTree.FindLiteralExpression(text);
            Assert.AreEqual(true, UrlTemplate.TryParse(literal, out var template));
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Text));
        }
    }
}
