namespace AspNetCoreAnalyzers.Tests.ASP002RouteParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [TestCase("\"api/{text}\"")]
        [TestCase("\"api/{text?}\"")]
        [TestCase("\"api/{*text}\"")]
        [TestCase("\"api/{**text}\"")]
        [TestCase("@\"api/{text}\"")]
        [TestCase("\"api/{text:alpha}\"")]
        [TestCase("\"api/{text=abc}\"")]
        public void WhenHttpGet(string after)
        {
            var code = @"
namespace AspBox
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{text}"")]
        public IActionResult GetValue(string text)
        {
            return this.Ok(text);
        }
    }
}".AssertReplace("\"api/{text}\"", after);

            AnalyzerAssert.Valid(Analyzer, code);
        }

        [TestCase("\"api/orders/\" + \"{wrong}\"")]
        public void IgnoreWhen(string template)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{value}"")]
        public IActionResult GetId(string value)
        {
            return this.Ok(value);
        }
    }
}".AssertReplace("\"api/orders/{value}\"", template);
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void ImplicitFromRoute()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult Get(int id)
        {
            return this.Ok(id);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void ExplicitFromRoute()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult Get([FromRoute]int id)
        {
            return this.Ok(id);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [Test]
        public void WhenFromHeaderAndNoRouteParameter()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders"")]
        public IActionResult Get([FromHeader]int id)
        {
            return this.Ok(id);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
