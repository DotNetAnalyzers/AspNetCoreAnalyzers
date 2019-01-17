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
        public void WhenRouteOnClassAndHttpGetOnMethod()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values/{↓id}"")]
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

        [Test]
        public void Issue39()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values/{id}"")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet(""api/values/{id}"")]
        public ActionResult<string> Get(string id)
        {
            return this.Ok(id);
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public void WhenMultipleRoutesOnClassAndHttpGetOnMethod()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values/{↓id}"")]
    [Route(""api/values"")]
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
