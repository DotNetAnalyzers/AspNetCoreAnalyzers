namespace AspNetCoreAnalyzers.Tests.ASP012UseExplicitRouteTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [Test]
        public void Simple()
        {
            var code = @"
namespace AspBox
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route(""api/orders"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""{value}"")]
        public IActionResult GetValue(string value)
        {
            return this.Ok(value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
