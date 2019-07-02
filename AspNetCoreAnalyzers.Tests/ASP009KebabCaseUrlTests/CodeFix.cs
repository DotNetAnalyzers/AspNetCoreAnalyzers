namespace AspNetCoreAnalyzers.Tests.ASP009KebabCaseUrlTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP009KebabCaseUrl.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"api/↓Orders/{id}\"",    "\"api/orders/{id}\"")]
        [TestCase("\"api/↓TwoWords/{id}\"",  "\"api/two-words/{id}\"")]
        [TestCase("\"api/↓twoWords/{id}\"",  "\"api/two-words/{id}\"")]
        [TestCase("\"api/↓two_words/{id}\"", "\"api/two-words/{id}\"")]
        public static void WhenMethodAttribute(string before, string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/Orders/{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/Orders/{id}\"", before);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", after);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("\"api/↓Orders\"",    "\"api/orders\"")]
        [TestCase("\"api/↓TwoWords\"",  "\"api/two-words\"")]
        [TestCase("\"api/↓twoWords\"",  "\"api/two-words\"")]
        [TestCase("\"api/↓two_words\"", "\"api/two-words\"")]
        public static void WhenRouteAttribute(string before, string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/↓TwoWords"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/↓TwoWords\"", before);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/orders"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders\"", after);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
