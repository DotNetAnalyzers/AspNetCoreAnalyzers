namespace AspNetCoreAnalyzers.Tests.ASP013ControllerNameShouldMatchRouteTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = ASP013ControllerNameShouldMatchRoute.Descriptor;

        [TestCase("api/orders")]
        [TestCase("api/[controller]")]
        public void Simple(string template)
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
}".AssertReplace("api/orders", template);
            AnalyzerAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
