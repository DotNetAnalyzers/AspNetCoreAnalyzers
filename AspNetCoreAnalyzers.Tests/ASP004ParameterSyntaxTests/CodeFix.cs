namespace AspNetCoreAnalyzers.Tests.ASP004ParameterSyntaxTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP004ParameterSyntax.Descriptor);
        private static readonly CodeFixProvider Fix = new ParameterSyntaxFix();

        [TestCase("api/orders/↓id:long}", "api/orders/{id:long}")]
        [TestCase("api/orders/↓{id:long", "api/orders/{id:long}")]
        [TestCase("api/orders/{id:min(1}", "api/orders/{id:min(1)}")]
        [TestCase("api/orders/{id:max(1}", "api/orders/{id:max(1)}")]
        [TestCase("api/orders/{id:minlength(1}", "api/orders/{id:minlength(1)}")]
        [TestCase("api/orders/{id:maxlength(1}", "api/orders/{id:maxlength(1)}")]
        [TestCase("api/orders/{id:length(1}", "api/orders/{id:length(1)}")]
        [TestCase("api/orders/{id:length(1,2}", "api/orders/{id:length(1,2)}")]
        [TestCase("api/orders/{id:range(1,2}", "api/orders/{id:range(1,2)}")]
        [TestCase("api/orders/{id:regex((a|b)-c}", "api/orders/{id:regex((a|b)-c)}")]
        public void WhenFixable(string before, string after)
        {
            var code = @"
namespace ValidCode
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
}".AssertReplace("api/orders/{id:long}", before);

            var fixedCode = @"
namespace ValidCode
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
}".AssertReplace("api/orders/{id:long}", after);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("api/orders/{id:↓wrong}")]
        [TestCase("api/orders/{id:min1)}")]
        [TestCase("api/orders/{id:max1)}")]
        public void WhenNot(string before)
        {
            var code = @"
namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/↓{id:wrong}"")]
        public IActionResult GetId(long id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("api/orders/↓{id:wrong}", before);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
