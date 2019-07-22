namespace AspNetCoreAnalyzers.Tests.ASP001ParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP001ParameterSymbolName.Descriptor);

        [Test]
        public static void BothParameters()
        {
            var code = @"
namespace AspBox
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{orderId}/items/{itemId}"")]
        public async Task<IActionResult> GetOrder↓(int wrong1, int wrong2)
        {
            return this.Ok(wrong1 * wrong2);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
