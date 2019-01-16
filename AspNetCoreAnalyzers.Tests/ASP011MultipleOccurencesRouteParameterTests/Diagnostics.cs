namespace AspNetCoreAnalyzers.Tests.ASP011MultipleOccurencesRouteParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP011MultipleOccurencesRouteParameter.Descriptor);

        [TestCase("\"api/{↓id}/{↓id}\"")]
        public void WhenMethodAttribute(string template)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{↓id}/{↓id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/{↓id}/{↓id}\"", template);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void WhenSeparateAttributes()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values/{id}"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""items/{↓id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
