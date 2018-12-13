namespace AspNetCoreAnalyzers.Tests.ASP003ParameterTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [Test]
        public void ImplicitType()
        {
            var order = @"
namespace ValidCode
{
    public class Order
    {
        public int Id { get; set; }
    }
}";

            var db = @"
namespace ValidCode
{
    using Microsoft.EntityFrameworkCore;

    public class Db : DbContext
    {
        public DbSet<Order> Orders { get; set; }
    }
}";
            var code = @"
namespace ValidCode
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

        [TestCase("api/orders/{id:int}")]
        [TestCase("api/orders/{id:min(1)}")]
        [TestCase("api/orders/{id:max(1)}")]
        [TestCase("api/orders/{id:range(1,10)}")]
        [TestCase("api/orders/{id:required}")]
        public void ExplicitInteger(string template)
        {
            var order = @"
namespace ValidCode
{
    public class Order
    {
        public int Id { get; set; }
    }
}";

            var db = @"
namespace ValidCode
{
    using Microsoft.EntityFrameworkCore;

    public class Db : DbContext
    {
        public DbSet<Order> Orders { get; set; }
    }
}";
            var code = @"
namespace ValidCode
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

        [HttpGet(""api/orders/{id:int}"")]
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
}".AssertReplace("api/orders/{id:int}", template);
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }

        [TestCase("api/orders/{id:minlength(1)}")]
        [TestCase("api/orders/{id:maxlength(1)}")]
        [TestCase("api/orders/{id:length(1)}")]
        [TestCase("api/orders/{id:length(1,3)}")]
        [TestCase("api/orders/{id:alpha}")]
        [TestCase("api/orders/{id:regex(a-(0|1))}")]
        [TestCase("api/orders/{id:required}")]
        public void ExplicitString(string template)
        {
            var order = @"
namespace ValidCode
{
    public class Order
    {
        public string Id { get; set; }
    }
}";

            var db = @"
namespace ValidCode
{
    using Microsoft.EntityFrameworkCore;

    public class Db : DbContext
    {
        public DbSet<Order> Orders { get; set; }
    }
}";
            var code = @"
namespace ValidCode
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

        [HttpGet(""api/orders/{id:int}"")]
        public async Task<IActionResult> GetOrder(string id)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}".AssertReplace("api/orders/{id:int}", template);
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }
    }
}
