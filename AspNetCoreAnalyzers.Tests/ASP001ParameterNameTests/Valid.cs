namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = ASP001ParameterSymbolName.Descriptor;

        [TestCase("@\"{value}\"")]
        [TestCase("\"{value}\"")]
        [TestCase("\"{value?}\"")]
        [TestCase("\"{*value}\"")]
        [TestCase("\"{**value}\"")]
        [TestCase("\"{value=abc}\"")]
        [TestCase("@\"{value?}\"")]
        [TestCase("\"api/orders/{value}\"")]
        [TestCase("\"api/orders/{value?}\"")]
        [TestCase("\"api/orders/{value:alpha}\"")]
        [TestCase("\"api/orders/{value:regex(a-(0|1))}\"")]
        public static void When(string template)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{value}"")]
        public IActionResult Get(string value)
        {
            return this.Ok(value);
        }
    }
}".AssertReplace("\"api/orders/{value}\"", template);
            RoslynAssert.Valid(Analyzer,  code);
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
        public IActionResult Get(string value)
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
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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
        public static void ImplicitOptionalFromRoute()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id?}"")]
        public IActionResult Get(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            return this.Ok(id);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ImplicitTypedFromRoute()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:int}"")]
        public IActionResult Get(int id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

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

        [TestCase("[FromHeader]")]
        [TestCase("[FromBody]")]
        public static void WhenWrongAttribute(string attribute)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult Get([FromHeader]int headerValue)
        {
            return this.Ok(headerValue);
        }
    }
}".AssertReplace("[FromHeader]", attribute);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
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
