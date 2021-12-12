namespace AspNetCoreAnalyzers.Tests.ASP002RouteParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [TestCase("\"api/{text}\"")]
        [TestCase("\"api/{text?}\"")]
        [TestCase("\"api/{*text}\"")]
        [TestCase("\"api/{**text}\"")]
        [TestCase("@\"api/{text}\"")]
        [TestCase("\"api/{text:alpha}\"")]
        [TestCase("\"api/{text=abc}\"")]
        public static void WhenHttpGet(string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

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

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("\"api/orders/\" + \"{wrong}\"")]
        public static void IgnoreWhen(string template)
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ImplicitFromRoute()
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExplicitFromRoute()
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenFromHeaderAndNoRouteParameter()
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
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
