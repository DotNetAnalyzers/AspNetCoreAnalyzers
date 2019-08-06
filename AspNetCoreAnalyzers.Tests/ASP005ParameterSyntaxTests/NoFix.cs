namespace AspNetCoreAnalyzers.Tests.ASP005ParameterSyntaxTests
{
    using System.Globalization;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using NUnit.Framework;

    public static class NoFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP005ParameterSyntax);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"{↓id*}\"")]
        [TestCase("\"{↓id/}\"")]
        [TestCase("\"{↓i/d}\"")]
        [TestCase("\"{↓/i/d/}\"")]
        [TestCase("\"{id:↓wrong}\"")]
        [TestCase("\"api/orders/{id:↓wrong}\"")]
        [TestCase("@\"api/orders/{id:↓wrong}\"")]
        [TestCase("\"api/orders/{id:↓min1)}\"")]
        [TestCase("\"api/orders/{↓/i/d/:min1)}\"")]
        [TestCase("\"api/orders/{↓/i/d/:min(1)}\"")]
        [TestCase("\"api/orders/{id:↓max1)}\"")]
        [TestCase("\"api/orders/{id:min(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:max(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:↓:long)}\"")]
        [TestCase("\"api/orders/{id:long:↓)}\"")]
        [TestCase("\"api/orders/{id:long:↓:)}\"")]
        public static void WhenLong(string before)
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

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [TestCase("\"api/orders/{id:minlength(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:minlength(↓1a))}\"")]
        [TestCase("\"api/orders/{id:minlength(↓a1))}\"")]
        [TestCase("\"api/orders/{id:maxlength(↓wrong))}\"")]
        [TestCase("\"api/orders/{id:regex(\\\\d):minlength(↓wrong))}\"")]
        [TestCase("@\"api/orders/{id:regex(\\d):minlength(↓wrong))}\"")]
        public static void WhenString(string before)
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

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [TestCase("\"api/orders/{id:regex(\\\\d):minlength(wrong)}\"", 54, 59)]
        [TestCase("@\"api/orders/{id:regex(\\d):minlength(wrong)}\"", 54, 59)]
        public static void WhenStringExplicitSpan(string before, int start, int end)
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
                Descriptors.ASP005ParameterSyntax.Id,
                Descriptors.ASP005ParameterSyntax.MessageFormat.ToString(CultureInfo.InvariantCulture),
                new FileLinePositionSpan("OrdersController.cs", new LinePosition(8, start), new LinePosition(8, end)));
            RoslynAssert.Diagnostics(Analyzer, expectedDiagnostic, code);
        }
    }
}
