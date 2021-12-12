namespace AspNetCoreAnalyzers.Tests.ASP007MissingParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP007MissingParameter);

        [Test]
        public static void WhenNoParameter()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{↓id}"")]
        public IActionResult Get() => this.Ok();
    }
}";
            var message = "The route template has parameter 'id' that does not have a corresponding method parameter.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public static void WhenLastIsMissing()
        {
            var order = @"
namespace AspBox
{
    public class Order
    {
        public int Id { get; set; }
    }
}";

            var db = @"
namespace AspBox
{
    using Microsoft.EntityFrameworkCore;

    public class Db : DbContext
    {
        public DbSet<Order> Orders => this.Set<Order>();
    }
}";
            var code = @"
namespace AspBox
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    public class OrdersController : Controller
    {
        private readonly Db db;

        public OrdersController(Db db)
        {
            this.db = db;
        }

        [HttpGet(""api/orders/{orderId}/items/{↓itemId}"")]
        public async Task<IActionResult> Get(int orderId)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";
            var message = "The route template has parameter 'itemId' that does not have a corresponding method parameter.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), order, db, code);
        }

        [Test]
        public static void WhenFirstIsMissing()
        {
            var order = @"
namespace AspBox
{
    public class Order
    {
        public int Id { get; set; }
    }
}";

            var db = @"
namespace AspBox
{
    using Microsoft.EntityFrameworkCore;

    public class Db : DbContext
    {
        public DbSet<Order> Orders => this.Set<Order>();
    }
}";
            var code = @"
namespace AspBox
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    public class OrdersController : Controller
    {
        private readonly Db db;

        public OrdersController(Db db)
        {
            this.db = db;
        }

        [HttpGet(""api/orders/{↓orderId}/items/{itemId}"")]
        public async Task<IActionResult> Get(int itemId)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == itemId);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, order, db, code);
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
        [HttpGet(""api/orders/{↓id}"")]
        public IActionResult Get([FromHeader]int headerValue)
        {
            return this.Ok(headerValue);
        }
    }
}".AssertReplace("[FromHeader]", attribute);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void WhenRouteOnClass()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values/{↓id}"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok();
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void WhenRoutesOnClass()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values"")]
    [Route(""api/values/{↓id}"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok();
        }
    }
}";
            var message = "The route template has parameter 'id' that does not have a corresponding method parameter.";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage(message), code);
        }

        [Test]
        public static void WhenMultipleRouteAttributesMissingActionWithParameter()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values"")]
    [Route(""api/values/{↓id}"")]
    [ApiController]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok();
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
