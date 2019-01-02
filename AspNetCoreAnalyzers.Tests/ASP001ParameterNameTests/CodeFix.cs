namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP001ParameterSymbolName.Descriptor);
        private static readonly CodeFixProvider Fix = new ParameterNameFix();

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
        [TestCase("\"api/orders/{value:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"")]
        public void When(string template)
        {
            var code = @"
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

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void ImplicitSingleParameter()
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { order, db, before }, after);
        }

        [Test]
        public void ImplicitFirstParameter()
        {
            var orderItem = @"
namespace AspBox
{
    public class OrderItem
    {
        public int Id { get; set; }
    }
}";
            var order = @"
namespace AspBox
{
    using System.Collections.Generic;

    public class Order
    {
        public int Id { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }
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
namespace AspBox
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { orderItem, order, db, before }, after);
        }

        [Test]
        public void ImplicitLastParameter()
        {
            var orderItem = @"
namespace AspBox
{
    public class OrderItem
    {
        public int Id { get; set; }
    }
}";
            var order = @"
namespace AspBox
{
    using System.Collections.Generic;

    public class Order
    {
        public int Id { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }
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
namespace AspBox
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { orderItem, order, db, before }, after);
        }

        [Test]
        public void ExplicitAttributeSingleParameter()
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { order, db, before }, after);
        }

        [Test]
        public void BothParameters()
        {
            var orderItem = @"
namespace AspBox
{
    public class OrderItem
    {
        public int Id { get; set; }
    }
}";
            var order = @"
namespace AspBox
{
    public class Order
    {
        public int Id { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }
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

        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public async Task<IActionResult> GetOrder↓(int wrong1, int wrong2)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == wrong1)?.FirstOrDefaultAsync(x => x.Id == wrong2);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, orderItem, order, db, before);
        }
    }
}
