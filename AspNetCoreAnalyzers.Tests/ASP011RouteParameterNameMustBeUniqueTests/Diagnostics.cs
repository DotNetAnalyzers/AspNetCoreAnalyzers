namespace AspNetCoreAnalyzers.Tests.ASP011RouteParameterNameMustBeUniqueTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP011RouteParameterNameMustBeUnique);

    [TestCase("\"api/{↓id}/{↓id}\"")]
    public static void WhenMethodAttribute(string template)
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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void WhenRouteOnClassAndHttpGetOnMethod()
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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void Issue39()
    {
        var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values/{↓id}"")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet(""api/values/{↓id}"")]
        public ActionResult<string> Get(string id)
        {
            return this.Ok(id);
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void WhenMultipleRoutesOnClassAndHttpGetOnMethod()
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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}
