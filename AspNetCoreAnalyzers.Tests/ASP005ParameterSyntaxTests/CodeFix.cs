namespace AspNetCoreAnalyzers.Tests.ASP005ParameterSyntaxTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP005ParameterSyntax.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"↓id}\"",                       "\"{id}\"")]
        [TestCase("\"↓{id\"",                       "\"{id}\"")]
        [TestCase("\"{↓id}}\"",                     "\"{id}\"")]
        [TestCase("\"{↓{id}\"",                     "\"{id}\"")]
        [TestCase("\"api/orders/↓id}\"",            "\"api/orders/{id}\"")]
        [TestCase("\"api/orders/↓{id\"",            "\"api/orders/{id}\"")]
        [TestCase("\"api/orders/{↓id}}\"",          "\"api/orders/{id}\"")]
        [TestCase("\"api/orders/{↓{id}\"",          "\"api/orders/{id}\"")]
        [TestCase("\"api/orders/↓id:long}\"",       "\"api/orders/{id:long}\"")]
        [TestCase("\"api/orders/↓{id:long\"",       "\"api/orders/{id:long}\"")]
        [TestCase("\"api/orders/{id:min(1}\"",      "\"api/orders/{id:min(1)}\"")]
        [TestCase("\"api/orders/{id:max(1}\"",      "\"api/orders/{id:max(1)}\"")]
        [TestCase("\"api/orders/{id:range(1,2}\"",  "\"api/orders/{id:range(1,2)}\"")]
        public void WhenLong(string before, string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:long}"")]
        public IActionResult GetId(long id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id:long}\"", before);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:long}"")]
        public IActionResult GetId(long id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id:long}\"", after);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("\"api/orders/{id:↓minlength(1}\"",            "\"api/orders/{id:minlength(1)}\"")]
        [TestCase("\"api/orders/{id:↓maxlength(1}\"",            "\"api/orders/{id:maxlength(1)}\"")]
        [TestCase("\"api/orders/{id:↓length(1}\"",               "\"api/orders/{id:length(1)}\"")]
        [TestCase("\"api/orders/{id:↓length(1,2}\"",             "\"api/orders/{id:length(1,2)}\"")]
        [TestCase("\"api/orders/{id:↓regex((a|b)-c}\"",          "\"api/orders/{id:regex((a|b)-c)}\"")]
        [TestCase("\"api/orders/{id:regex(\\\\d+):↓length(1}\"", "\"api/orders/{id:regex(\\\\d+):length(1)}\"")]
        public void WhenString(string before, string after)
        {
            var code = @"
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
}".AssertReplace("\"api/orders/{id}\"", before);

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
    }
}
