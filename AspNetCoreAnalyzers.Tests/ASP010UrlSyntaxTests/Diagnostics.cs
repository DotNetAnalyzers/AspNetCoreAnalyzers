namespace AspNetCoreAnalyzers.Tests.ASP010UrlSyntaxTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP010UrlSyntax.Descriptor);

        [TestCase("\"api/a↓?b/{id}\"")]
        [TestCase("\"api/↓/b/{id}\"")]
        public void WhenMethodAttribute(string before)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/a↓?b/{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/a↓?b/{id}\"", before);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("\"api/a↓?b\"")]
        [TestCase("\"api/↓/b\"")]
        public void WhenRouteAttribute(string before)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/a↓?b"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/a↓?b\"", before);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
