namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP001ParameterSymbolName);

    [Test]
    public static void BothParameters()
    {
        var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public IActionResult GetOrder↓(int wrong1, int wrong2)
        {
            return this.Ok(wrong1 * wrong2);
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}
