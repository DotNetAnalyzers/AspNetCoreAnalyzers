namespace AspNetCoreAnalyzers.Tests.Helpers
{
    using System;
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
namespace AspBox
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
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Span.ToString()));
            Assert.IsTrue(template.Path.All(x => x.Parameter == null));
        }

        [TestCase("{id}",                 new[] { "{id}" })]
        [TestCase("{*id}",                new[] { "{*id}" })]
        [TestCase("{**id}",               new[] { "{**id}" })]
        [TestCase("{id=1}",               new[] { "{id=1}" })]
        [TestCase("{id}/info",            new[] { "{id}", "info" })]
        [TestCase("{id?}/info",           new[] { "{id?}", "info" })]
        [TestCase("{id:int}/info",        new[] { "{id:int}", "info" })]
        [TestCase("{id:required}/info",   new[] { "{id:required}", "info" })]
        [TestCase("api/orders/{id}",      new[] { "api", "orders", "{id}" })]
        [TestCase("api/orders/{id}/info", new[] { "api", "orders", "{id}", "info" })]
        public void TryParseWithParameter(string text, string[] expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox
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
            CollectionAssert.AreEqual(expected, template.Path.Select(x => x.Span.ToString()));

            // ReSharper disable once PossibleInvalidOperationException
            var segment = template.Path.Single(x => x.Parameter.HasValue);

            Assert.AreEqual(expected.Single(x => x.StartsWith("{", StringComparison.Ordinal)), segment.Span.ToString());
            Assert.AreEqual("id",                                                              segment.Parameter?.Name.ToString());
        }

        [TestCase("\"orders/{id}\"",                   new[] { "orders", "{id}" },                   new string[0])]
        [TestCase("@\"orders/{id}\"",                  new[] { "orders", "{id}" },                   new string[0])]
        [TestCase("\"orders/{id?}\"",                  new[] { "orders", "{id?}" },                  new[] { "?" })]
        [TestCase("\"orders/{id:min(1)}\"",            new[] { "orders", "{id:min(1)}" },            new[] { "min(1)" })]
        [TestCase("\"orders/{id:int:min(1):max(2)}\"", new[] { "orders", "{id:int:min(1):max(2)}" }, new[] { "int", "min(1)", "max(2)" })]
        [TestCase("\"orders/{id:max(2)}\"",            new[] { "orders", "{id:max(2)}" },            new[] { "max(2)" })]
        [TestCase("\"orders/{id:int:max(2)}\"",        new[] { "orders", "{id:int:max(2)}" },        new[] { "int", "max(2)" })]
        [TestCase("\"orders/{id:range(1,23)}\"",       new[] { "orders", "{id:range(1,23)}" },       new[] { "range(1,23)" })]
        public void TryParseWhenIntParameter(string text, string[] segments, string[] constraints)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox
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
}".AssertReplace("\"orders/{id}\"", text));
            var literal = syntaxTree.FindLiteralExpression(text);
            Assert.AreEqual(true, UrlTemplate.TryParse(literal, out var template));
            CollectionAssert.AreEqual(segments, template.Path.Select(x => x.Span.ToString()));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual("id", parameter.Name.ToString());
            CollectionAssert.AreEqual(constraints, parameter.Constraints.Select(x => x.Span.ToString()));
        }

        [TestCase("\"orders/{id}\"",                                         new[] { "orders", "{id}" },                                   new string[0])]
        [TestCase("\"orders/{id?}\"",                                        new[] { "orders", "{id?}" },                                  new[] { "?" })]
        [TestCase("\"orders/{id:alpha}\"",                                   new[] { "orders", "{id:alpha}" },                             new[] { "alpha" })]
        [TestCase("\"orders/{id:ALPHA}\"",                                   new[] { "orders", "{id:ALPHA}" },                             new[] { "ALPHA" })]
        [TestCase("\"orders/{id:alpha:minlength(1)}\"",                      new[] { "orders", "{id:alpha:minlength(1)}" },                new[] { "alpha", "minlength(1)" })]
        [TestCase("\"orders/{id:minlength(1)}\"",                            new[] { "orders", "{id:minlength(1)}" },                      new[] { "minlength(1)" })]
        [TestCase("\"orders/{id:maxlength(1)}\"",                            new[] { "orders", "{id:maxlength(1)}" },                      new[] { "maxlength(1)" })]
        [TestCase("\"orders/{id:length(1)}\"",                               new[] { "orders", "{id:length(1)}" },                         new[] { "length(1)" })]
        [TestCase("\"orders/{id:length(1,2)}\"",                             new[] { "orders", "{id:length(1,2)}" },                       new[] { "length(1,2)" })]
        [TestCase("\"orders/{id:regex(^\\\\d$)}\"",                          new[] { "orders", "{id:regex(^\\d$)}" },                      new[] { "regex(^\\d$)" })]
        [TestCase("@\"orders/{id:regex(^\\d$)}\"",                           new[] { "orders", "{id:regex(^\\d$)}" },                      new[] { "regex(^\\d$)" })]
        [TestCase("\"orders/{id:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"", new[] { "orders", "{id:regex(^\\d{{3}}-\\d{{2}}-\\d{4}$)}" }, new[] { "regex(^\\d{{3}}-\\d{{2}}-\\d{4}$)" })]
        [TestCase("\"orders/{id:regex(a/b)}\"",                              new[] { "orders", "{id:regex(a/b)}" },                        new[] { "regex(a/b)" })]
        [TestCase("\"orders/{id:regex(a{{0,1}})}\"",                         new[] { "orders", "{id:regex(a{{0,1}})}" },                   new[] { "regex(a{{0,1}})" })]
        [TestCase("\"orders/{id:minlength(1):regex(a{{0,1}})}\"",            new[] { "orders", "{id:minlength(1):regex(a{{0,1}})}" },      new[] { "minlength(1)", "regex(a{{0,1}})" })]
        public void TryParseWhenStringParameter(string text, string[] segments, string[] constraints)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox
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
}".AssertReplace("\"orders/{id}\"", text));
            var literal = syntaxTree.FindLiteralExpression(text);
            Assert.AreEqual(true, UrlTemplate.TryParse(literal, out var template));
            CollectionAssert.AreEqual(segments, template.Path.Select(x => x.Span.ToString()));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual("id", parameter.Name.ToString());
            CollectionAssert.AreEqual(constraints, parameter.Constraints.Select(x => x.Span.ToString()));
        }

        [TestCase("{?id}",          new[] { "{?id}" },             "?id")]
        [TestCase("{id*}",          new[] { "{id*}" },             "id*")]
        [TestCase("{i*d*}",         new[] { "{i*d*}" },            "i*d*")]
        [TestCase("orders/{id*}",   new[] { "orders", "{id*}" },   "id*")]
        public void TryParseWhenSyntaxErrorParameter(string text, string[] segments, string name)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""{id}"")]
        public async Task<IActionResult> GetOrder([FromRoute]int id)
        {
        }
    }
}".AssertReplace("{id}", text));
            var literal = syntaxTree.FindLiteralExpression(text);
            Assert.AreEqual(true, UrlTemplate.TryParse(literal, out var template));
            CollectionAssert.AreEqual(segments, template.Path.Select(x => x.Span.ToString()));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual(name, parameter.Name.ToString());
        }

        [TestCase("orders/{id:}",      new[] { "orders", "{id:}" },      new[] { "" })]
        [TestCase("orders/{id:min(1}", new[] { "orders", "{id:min(1}" }, new[] { "min(1" })]
        [TestCase("orders/{id:min1)}", new[] { "orders", "{id:min1)}" }, new[] { "min1)" })]
        public void TryParseWhenSyntaxErrorConstraint(string text, string[] segments, string[] constraints)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace AspBox
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
            CollectionAssert.AreEqual(segments, template.Path.Select(x => x.Span.ToString()));

            // ReSharper disable once PossibleInvalidOperationException
            var parameter = template.Path.Single(x => x.Parameter.HasValue)
                                    .Parameter.Value;
            Assert.AreEqual("id", parameter.Name.ToString());
            CollectionAssert.AreEqual(constraints, parameter.Constraints.Select(x => x.Span.ToString()));
        }
    }
}
