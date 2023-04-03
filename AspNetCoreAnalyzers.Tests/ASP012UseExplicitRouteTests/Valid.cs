namespace AspNetCoreAnalyzers.Tests.ASP012UseExplicitRouteTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

public static class Valid
{
    private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

    [Test]
    public static void Simple()
    {
        var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

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
