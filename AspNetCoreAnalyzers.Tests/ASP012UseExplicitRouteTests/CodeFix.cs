namespace AspNetCoreAnalyzers.Tests.ASP012UseExplicitRouteTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP012UseExplicitRoute.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("OrdersController",      "orders")]
        [TestCase("SampleDataController ", "sample-data")]
        public void WhenMethodAttribute(string className, string after)
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
}".AssertReplace("OrdersController", className);

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
  .AssertReplace("orders", after);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
