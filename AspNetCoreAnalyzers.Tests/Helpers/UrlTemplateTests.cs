namespace AspNetCoreAnalyzers.Tests.Helpers
{
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class UrlTemplateTests
    {
        [TestCase("foo",     new[] { "foo" })]
        [TestCase("foo/bar", new[] { "foo", "bar" })]
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
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Text.Text));
        }

        [TestCase("{id}",                 new[] { "{id}" })]
        [TestCase("{id}/info",            new[] { "{id}", "info" })]
        [TestCase("{id?}/info",           new[] { "{id?}", "info" })]
        [TestCase("{id:int}/info",        new[] { "{id:int}", "info" })]
        [TestCase("{id:required}/info",   new[] { "{id:required}", "info" })]
        [TestCase("api/orders/{id}",      new[] { "api", "orders", "{id}" })]
        [TestCase("api/orders/{id}/info", new[] { "api", "orders", "{id}", "info" })]
        public void TryParseWithParameter(string text, string[] expected)
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
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Text.Text));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual("id", parameter.Name.Text);
        }

        [TestCase("orders/{id:min(1)}",            new[] { "orders", "{id:min(1)}" })]
        [TestCase("orders/{id:int:min(1):max(2)}", new[] { "orders", "{id:int:min(1):max(2)}" })]
        [TestCase("orders/{id:max(2)}",            new[] { "orders", "{id:max(2)}" })]
        [TestCase("orders/{id:int:max(2)}",        new[] { "orders", "{id:int:max(2)}" })]
        [TestCase("orders/{id:range(1,23)}",       new[] { "orders", "{id:range(1,23)}" })]
        public void TryParseWhenIntParameter(string text, string[] expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace ValidCode
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""orders/{id}"")]
        public async Task<IActionResult> GetOrder([FromRoute]int id)
        {
        }
    }
}".AssertReplace("orders/{id}", text));
            var literal = syntaxTree.FindLiteralExpression(text);
            Assert.AreEqual(true, UrlTemplate.TryParse(literal, out var template));
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Text.Text));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual("id", parameter.Name.Text);
        }

        [TestCase("orders/{id:alpha}",                               new[] { "orders", "{id:alpha}" })]
        [TestCase("orders/{id:alpha:minlength(1)}",                  new[] { "orders", "{id:alpha:minlength(1)}" })]
        [TestCase("orders/{id:minlength(1)}",                        new[] { "orders", "{id:minlength(1)}" })]
        [TestCase("orders/{id:maxlength(1)}",                        new[] { "orders", "{id:maxlength(1)}" })]
        [TestCase("orders/{id:length(1)}",                           new[] { "orders", "{id:length(1)}" })]
        [TestCase("orders/{id:length(1,2)}",                         new[] { "orders", "{id:length(1,2)}" })]
        [TestCase("orders/{id:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{{4}}$)}", new[] { "orders", "{id:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}}$)}" })]
        [TestCase("orders/{id:regex(a/b)}",                          new[] { "orders", "{id:regex(a/b)}" })]
        ////[TestCase("orders/{id:regex(a[)}/]b)}",                      new[] { "orders", "{id:regex(a[)}/]b)}" })]
        public void TryParseWhenStringParameter(string text, string[] expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace ValidCode
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""orders/{id}"")]
        public async Task<IActionResult> GetOrder(string id)
        {
        }
    }
}".AssertReplace("orders/{id}", text));
            var literal = syntaxTree.FindLiteralExpression(text);
            Assert.AreEqual(true, UrlTemplate.TryParse(literal, out var template));
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Text.Text));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual("id", parameter.Name.Text);
        }
    }
}
