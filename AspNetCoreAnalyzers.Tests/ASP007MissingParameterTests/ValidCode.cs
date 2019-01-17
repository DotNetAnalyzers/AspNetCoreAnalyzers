namespace AspNetCoreAnalyzers.Tests.ASP007MissingParameterTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = ASP007MissingParameter.Descriptor;

        [TestCase("\"api/{text}\"")]
        [TestCase("@\"api/{text}\"")]
        [TestCase("\"api/{text:alpha}\"")]
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

        [Test]
        public void WhenHttpGetAndTwoRoutesOnClass()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/values"")]
    [Route(""api/values/{id}"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult GetValue()
        {
            return this.Ok();
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
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
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }

        [Test]
        public void ExplicitFromRoute()
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

        [HttpGet(""api/orders/{id}"")]
        public async Task<IActionResult> GetOrder([FromRoute]int id)
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
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }

        [Test]
        public void WhenFromHeaderAndNoRouteParameter()
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

        [HttpGet(""api/orders"")]
        public async Task<IActionResult> GetOrder([FromHeader]int id)
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
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }
    }
}
