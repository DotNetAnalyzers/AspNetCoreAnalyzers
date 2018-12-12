namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = Gu.Roslyn.Asserts.ExpectedDiagnostic.Create(ASP001ParameterName.Descriptor);
        private static readonly CodeFixProvider RenameParameterFix = new RenameParameterFix();

        [Test]
        public void ImplicitSingleParameter()
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
            var before = @"
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
            AnalyzerAssert.CodeFix(Analyzer, RenameParameterFix, ExpectedDiagnostic, new[] { order, db, before }, after);
        }

        [Test]
        public void ImplicitFirstParameter()
        {
            var orderItem = @"
namespace ValidCode
{
    public class OrderItem
    {
        public int Id { get; set; }
    }
}";
            var order = @"
namespace ValidCode
{
    using System.Collections.Generic;

    public class Order
    {
        public int Id { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }
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
            var before = @"
namespace ValidCode
{
    using System.Linq;
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

        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public async Task<IActionResult> GetOrder(int ↓wrong, int itemId)
        {
            var order = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == wrong);
            if (order == null)
            {
                return this.NotFound();
            }

            var match = order.Items.FirstOrDefault(x => x.Id == itemId);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";

            var after = @"
namespace ValidCode
{
    using System.Linq;
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

        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public async Task<IActionResult> GetOrder(int orderId, int itemId)
        {
            var order = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
            {
                return this.NotFound();
            }

            var match = order.Items.FirstOrDefault(x => x.Id == itemId);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, RenameParameterFix, ExpectedDiagnostic, new[] { orderItem, order, db, before }, after);
        }

        [Test]
        public void ImplicitLastParameter()
        {
            var orderItem = @"
namespace ValidCode
{
    public class OrderItem
    {
        public int Id { get; set; }
    }
}";
            var order = @"
namespace ValidCode
{
    using System.Collections.Generic;

    public class Order
    {
        public int Id { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }
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
            var before = @"
namespace ValidCode
{
    using System.Linq;
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

        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public async Task<IActionResult> GetOrder(int orderId, int ↓wrong)
        {
            var order = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
            {
                return this.NotFound();
            }

            var match = order.Items.FirstOrDefault(x => x.Id == wrong);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";

            var after = @"
namespace ValidCode
{
    using System.Linq;
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

        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public async Task<IActionResult> GetOrder(int orderId, int itemId)
        {
            var order = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
            {
                return this.NotFound();
            }

            var match = order.Items.FirstOrDefault(x => x.Id == itemId);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, RenameParameterFix, ExpectedDiagnostic, new[] { orderItem, order, db, before }, after);
        }

        [Test]
        public void ExplicitAttributeSingleParameter()
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
            var before = @"
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
        public async Task<IActionResult> GetOrder([FromRoute]int ↓wrong)
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
            AnalyzerAssert.CodeFix(Analyzer, RenameParameterFix, ExpectedDiagnostic, new[] { order, db, before }, after);
        }
    }
}
