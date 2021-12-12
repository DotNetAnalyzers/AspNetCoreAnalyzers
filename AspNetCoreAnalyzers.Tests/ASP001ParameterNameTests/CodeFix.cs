namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP001ParameterSymbolName);
        private static readonly CodeFixProvider Fix = new RenameParameterFix();

        [TestCase("\"{value}\"")]
        [TestCase("@\"{value}\"")]
        [TestCase("\"{value?}\"")]
        [TestCase("\"{*value}\"")]
        [TestCase("\"{**value}\"")]
        [TestCase("\"{value=abc}\"")]
        [TestCase("@\"{value?}\"")]
        [TestCase("\"api/orders/{value}\"")]
        [TestCase("\"api/orders/{value?}\"")]
        [TestCase("\"api/orders/{value:alpha}\"")]
        [TestCase("\"api/orders/{value:regex(a-(0|1))}\"")]
        [TestCase("\"api/orders/{value:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"")]
        public static void WhenHttpGet(string template)
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{value}"")]
        public IActionResult GetId(string ↓wrong)
        {
            return this.Ok(wrong);
        }
    }
}".AssertReplace("\"api/orders/{value}\"", template);

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenRouteAndHttpGetOnMethod()
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [Route(""api/values/{value}"")]
        [HttpGet]
        public IActionResult GetId(string ↓wrong)
        {
            return this.Ok(wrong);
        }
    }
}";

            var after = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [Route(""api/values/{value}"")]
        [HttpGet]
        public IActionResult GetId(string value)
        {
            return this.Ok(value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ImplicitSingleParameter()
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
        public DbSet<Order> Orders { get; set; }
    }
}";
            var before = @"
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

        [HttpGet(""api/orders/{id}"")]
        public async Task<IActionResult> GetOrder(int ↓wrong)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == wrong);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";

            var after = @"
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

        [HttpGet(""api/orders/{id}"")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { order, db, before }, after);
        }

        [Test]
        public static void FirstParameter()
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public IActionResult GetOrder(int ↓wrong, int itemId)
        {
            return this.Ok(wrong * itemId);
        }
    }
}";

            var after = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public IActionResult GetOrder(int orderId, int itemId)
        {
            return this.Ok(orderId * itemId);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void LastParameter()
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public IActionResult GetOrder(int orderId, int ↓wrong)
        {
            return this.Ok(orderId * wrong);
        }
    }
}";

            var after = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public IActionResult GetOrder(int orderId, int itemId)
        {
            return this.Ok(orderId * itemId);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExplicitFromRouteAttributeSingleParameter()
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{value}"")]
        public IActionResult GetId(string ↓wrong)
        {
            return this.Ok(wrong);
        }
    }
}";

            var after = @"
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
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
