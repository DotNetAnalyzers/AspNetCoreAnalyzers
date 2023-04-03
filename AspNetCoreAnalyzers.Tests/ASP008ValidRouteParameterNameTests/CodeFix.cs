namespace AspNetCoreAnalyzers.Tests.ASP008ValidRouteParameterNameTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP008ValidRouteParameterName);
    private static readonly CodeFixProvider Fix = new TemplateTextFix();

    [TestCase("\"api/orders/{↓id }\"",                         "\"api/orders/{id}\"")]
    [TestCase("\"api/orders/{↓ id}\"",                         "\"api/orders/{id}\"")]
    [TestCase("\"api/orders/{↓ id }\"",                         "\"api/orders/{id}\"")]
    public static void When(string before, string after)
    {
        var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", before);

        var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", after);
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
    }

    [TestCase("\"api/orders/{↓action}\"")]
    [TestCase("\"api/orders/{↓area}\"")]
    [TestCase("\"api/orders/{↓controller}\"")]
    [TestCase("\"api/orders/{↓handler}\"")]
    [TestCase("\"api/orders/{↓page}\"")]
    public static void NoFixWhen(string before)
    {
        var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(string id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", before);

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }
}
