namespace AspNetCoreAnalyzers.Tests.ASP004ParameterSyntaxTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP004ParameterSyntax.Descriptor);

        [TestCase("\"api/orders/{id:↓wrong}\"")]
        [TestCase("\"api/orders/{id:min1)}\"")]
        [TestCase("\"api/orders/{id:max1)}\"")]
        [TestCase("\"api/orders/{id:min(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:max(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:↓:long)}\"")]
        [TestCase("\"api/orders/{id:long:↓)}\"")]
        [TestCase("\"api/orders/{id:long:↓:)}\"")]
        public void WhenLong(string before)
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
}".AssertReplace("\"api/orders/↓{id:wrong}\"", before);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("api/orders/{id:minlength(↓wrong))}")]
        [TestCase("api/orders/{id:maxlength(↓wrong))}")]
        public void WhenString(string before)
        {
            var code = @"
namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/↓{id:wrong}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("api/orders/↓{id:wrong}", before);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
