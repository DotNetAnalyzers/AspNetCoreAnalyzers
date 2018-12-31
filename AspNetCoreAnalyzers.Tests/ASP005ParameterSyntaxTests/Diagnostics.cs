namespace AspNetCoreAnalyzers.Tests.ASP005ParameterSyntaxTests
{
    using System.Globalization;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP005ParameterSyntax.Descriptor);

        [TestCase("\"api/orders/{id:↓wrong}\"")]
        [TestCase("@\"api/orders/{id:↓wrong}\"")]
        [TestCase("\"api/orders/{id:min1)}\"")]
        [TestCase("\"api/orders/{id:max1)}\"")]
        [TestCase("\"api/orders/{id:min(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:max(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:↓:long)}\"")]
        [TestCase("\"api/orders/{id:long:↓)}\"")]
        [TestCase("\"api/orders/{id:long:↓:)}\"")]
        public void WhenLong(string before)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/↓{id:wrong}"")]
        public IActionResult GetId(long id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/↓{id:wrong}\"", before);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("\"api/orders/{id:minlength(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:minlength(↓1a))}\"")]
        [TestCase("\"api/orders/{id:minlength(↓a1))}\"")]
        [TestCase("\"api/orders/{id:maxlength(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:regex(\\\\d):minlength(↓wrong))}\"")]
        [TestCase("@\"api/orders/{id:regex(\\d):minlength(↓wrong))}\"")]
        public void WhenString(string before)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/↓{id:wrong}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/↓{id:wrong}\"", before);

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("\"api/orders/{id:regex(\\\\d):minlength(wrong)}\"", 54, 59)]
        [TestCase("@\"api/orders/{id:regex(\\d):minlength(wrong)}\"", 54, 59)]
        public void WhenStringExplicitSpan(string before, int start, int end)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:wrong}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id:wrong}\"", before);

            var expectedDiagnostic = new ExpectedDiagnostic(
                ASP005ParameterSyntax.DiagnosticId,
                ASP005ParameterSyntax.Descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture),
                new FileLinePositionSpan("OrdersController.cs", new LinePosition(8, start), new LinePosition(8, end)));
            AnalyzerAssert.Diagnostics(Analyzer, expectedDiagnostic, code);
        }
    }
}
