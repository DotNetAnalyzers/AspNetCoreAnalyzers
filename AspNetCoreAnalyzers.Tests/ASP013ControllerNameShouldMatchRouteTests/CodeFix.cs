namespace AspNetCoreAnalyzers.Tests.ASP013ControllerNameShouldMatchRouteTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP013ControllerNameShouldMatchRoute.Descriptor);
        private static readonly CodeFixProvider Fix = new RenameTypeFix();

        [TestCase("orders",               "OrdersController")]
        [TestCase("sample-data",          "SampleDataController")]
        [TestCase("api/orders",           "OrdersController")]
        [TestCase("api/sample-data",      "SampleDataController")]
        [TestCase("api/orders/{id}",      "OrdersController")]
        [TestCase("api/sample-data/{id}", "SampleDataController")]
        public static void WhenMethodAttribute(string template, string className)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/orders"")]
    [ApiController]
    public class WrongNameController : Controller
    {
    }
}".AssertReplace("api/orders", template);

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
  .AssertReplace("api/orders", template);

            var message = $"Name the controller to match the route. Expected: '{className}'.";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), code, fixedCode);
        }
    }
}
