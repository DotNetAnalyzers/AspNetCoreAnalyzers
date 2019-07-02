namespace AspNetCoreAnalyzers.Tests.ASP012UseExplicitRouteTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP012UseExplicitRoute.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("OrdersController",      "[controller]",          "orders")]
        [TestCase("OrdersController",      "api/[controller]",      "api/orders")]
        [TestCase("OrdersController",      "api/[controller]/{id}", "api/orders/{id}")]
        [TestCase("SampleDataController ", "api/[controller]",      "api/sample-data")]
        [TestCase("SampleDataController ", "api/[controller]/{id}", "api/sample-data/{id}")]
        public static void WhenRouteAttribute(string className, string before, string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/[controller]"")]
    [ApiController]
    public class OrdersController : Controller
    {
    }
}".AssertReplace("OrdersController", className)
  .AssertReplace("api/[controller]", before);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/orders"")]
    [ApiController]
    public class OrdersController : Controller
    {
    }
}".AssertReplace("OrdersController", className)
  .AssertReplace("api/orders", after);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
