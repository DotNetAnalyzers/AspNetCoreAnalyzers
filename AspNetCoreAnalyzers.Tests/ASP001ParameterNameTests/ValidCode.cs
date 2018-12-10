namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = ASP001ParameterName.Descriptor;

        [Test]
        public void ImplicitFromRoute()
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

        [Test]
        public void ImplicitOptionalFromRoute()
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

        [HttpGet(""api/orders/{id?}"")]
        public async Task<IActionResult> GetOrder(int? id)
        {
            var match = id == null 
                            ? await this.db.Orders.FirstOrDefaultAsync()
                            : await this.db.Orders.FirstOrDefaultAsync(x => x.Id == id);
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
        public void ImplicitTypedFromRoute()
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
}";
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }

        [Test]
        public void ExplicitFromRoute()
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

        [TestCase("[FromHeader]")]
        [TestCase("[FromBody]")]
        public void WhenWrongAttribute(string attribute)
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
        public async Task<IActionResult> GetOrder([FromHeader]int headerValue)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == headerValue);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}".AssertReplace("[FromHeader]", attribute);
            AnalyzerAssert.Valid(Analyzer, Descriptor, order, db, code);
        }

        [Test]
        public void WhenFromHeaderAndNoRouteParameter()
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
