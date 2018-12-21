namespace AspNetCoreAnalyzers.Tests.ASP004ParameterSyntaxTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [Explicit("Failing tests.")]
    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP004ParameterSyntax.Descriptor);
        private static readonly CodeFixProvider Fix = new ParameterTypeFix();

        [TestCase("api/orders/↓id:long}", "api/orders/{id:long}")]
        [TestCase("api/orders/↓{id:long", "api/orders/{id:long}")]
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
        [HttpGet(""api/orders/{id:ilongnt}"")]
        public IActionResult GetId(long id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("api/orders/{id:long}", after);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
