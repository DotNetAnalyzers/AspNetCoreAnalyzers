namespace AspNetCoreAnalyzers.Tests.ASP013ControllerNameShouldMatchRouteTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.ASP013ControllerNameShouldMatchRoute;

        [TestCase("api/orders")]
        [TestCase("api/[controller]")]
        public static void Simple(string template)
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
}".AssertReplace("api/orders", template);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
