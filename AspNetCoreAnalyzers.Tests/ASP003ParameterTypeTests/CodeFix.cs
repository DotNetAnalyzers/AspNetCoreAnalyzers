namespace AspNetCoreAnalyzers.Tests.ASP003ParameterTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = Gu.Roslyn.Asserts.ExpectedDiagnostic.Create(ASP003ParameterType.Descriptor);
        private static readonly CodeFixProvider Fix = new ParameterTypeFix();

        [TestCase("api/orders/{id:int}", "int id")]
        [TestCase("api/orders/{id:bool}", "bool id")]
        [TestCase("api/orders/{id:decimal}", "decimal id")]
        [TestCase("api/orders/{id:double}", "double id")]
        [TestCase("api/orders/{id:float}", "float id")]

        public void ExplicitType(string template, string parameter)
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
        public async Task<IActionResult> GetOrder(↓byte id)
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

            var fixedCode = @"
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
        public async Task<IActionResult> GetOrder(byte id)
        {
            var match = await this.db.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (match == null)
            {
                return this.NotFound();
            }

            return this.Ok(match);
        }
    }
}".AssertReplace("api/orders/{id:int}", template)
  .AssertReplace("byte id", parameter);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { order, db, code }, fixedCode);
        }
    }
}
