namespace AspNetCoreAnalyzers.Tests.ASP003ParameterSymbolTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [TestCase("\"{id}\"",                           "int id")]
        [TestCase("\"{id}\"",                           "string id")]
        [TestCase("\"{id?}\"",                          "string id")]
        [TestCase("@\"{id}\"",                          "int id")]
        [TestCase("\"{id:int}\"",                       "int id")]
        [TestCase("\"api/orders/{id:int}\"",            "int id")]
        [TestCase("\"api/orders/{id:int:min(1)}\"",     "int id")]
        [TestCase("\"api/orders/{id:bool}\"",           "bool id")]
        [TestCase("\"api/orders/{id:datetime}\"",       "System.DateTime id")]
        [TestCase("\"api/orders/{id:decimal}\"",        "decimal id")]
        [TestCase("\"api/orders/{id:double}\"",         "double id")]
        [TestCase("\"api/orders/{id:float}\"",          "float id")]
        [TestCase("\"api/orders/{id:guid}\"",           "System.Guid id")]
        [TestCase("\"api/orders/{id:long}\"",           "long id")]
        [TestCase("\"api/orders/{id:minlength(1)}\"",   "string id")]
        [TestCase("\"api/orders/{id:maxlength(1)}\"",   "string id")]
        [TestCase("\"api/orders/{id:length(1)}\"",      "string id")]
        [TestCase("\"api/orders/{id:length(1,3)}\"",    "string id")]
        [TestCase("\"api/orders/{id:min(1)}\"",         "long id")]
        [TestCase("\"api/orders/{id:max(10)}\"",        "long id")]
        [TestCase("\"api/orders/{id:range(0,10)}\"",    "long id")]
        [TestCase("\"api/orders/{id:alpha}\"",          "string id")]
        [TestCase("\"api/orders/{id:regex(a-(0|1))}\"", "string id")]
        public void When(string template, string parameter)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:int}"")]
        public IActionResult GetId(byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id:int}\"", template)
  .AssertReplace("byte id", parameter);
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [TestCase("\"api/orders/\" + \"{value:int}\"")]
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
        public void ImplicitType()
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

        [TestCase("{value:bool}",     "bool")]
        [TestCase("{value:datetime}", "System.DateTime")]
        [TestCase("{value:decimal}",  "decimal")]
        [TestCase("{value:double}",   "double")]
        [TestCase("{value:float}",    "float")]
        [TestCase("{value:int}",      "int")]
        [TestCase("{value:long}",     "long")]
        [TestCase("{value:guid}",     "System.Guid")]
        public void ExplicitType(string constraint, string type)
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
        [HttpGet(""api/{value}"")]
        public async Task<int> GetOrder(int value)
        {
            return value;
        }
    }
}".AssertReplace("int", type)
  .AssertReplace("{value}", constraint)
                ;
            AnalyzerAssert.Valid(Analyzer, code);
        }

        [TestCase("api/orders/{id:int}")]
        [TestCase("api/orders/{id:int:min(1)}")]
        [TestCase("api/orders/{id:int:max(1)}")]
        [TestCase("api/orders/{id:int:range(1,10)}")]
        [TestCase("api/orders/{id:int:required}")]
        public void ExplicitInt(string template)
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

        [TestCase("api/orders/{id:min(1)}")]
        [TestCase("api/orders/{id:max(1)}")]
        [TestCase("api/orders/{id:range(1,10)}")]
        [TestCase("api/orders/{id:required}")]
        public void ImplicitLong(string template)
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

        [HttpGet(""api/orders/{id:int}"")]
        public async Task<IActionResult> GetOrder(long id)
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
        public void ImplicitString(string template)
        {
            var order = @"
namespace AspBox
{
    public class Order
    {
        public string Id { get; set; }
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
}".AssertReplace("api/orders/{id}", template);
            AnalyzerAssert.Valid(Analyzer, order, db, code);
        }
    }
}
