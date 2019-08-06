namespace AspNetCoreAnalyzers.Tests.ASP006ParameterRegexTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.ASP006ParameterRegex);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"api/orders/{id:regex(↓a{1})}\"",                         "\"api/orders/{id:regex(a{{1}})}\"")]
        [TestCase("\"api/orders/{id:regex(↓^[a-z]{2}$)}\"",                   "\"api/orders/{id:regex(^[[a-z]]{{2}}$)}\"")]
        [TestCase("\"api/orders/{id:regex(↓\\\\d+)}\"",                       "\"api/orders/{id:regex(\\\\\\\\d+)}\"")]
        [TestCase("@\"api/orders/{id:regex(↓\\d+)}\"",                        "@\"api/orders/{id:regex(\\\\d+)}\"")]
        [TestCase("\"api/orders/{id:regex(↓^\\\\d{3}-\\\\d{2}-\\\\d{4}$)}\"", "\"api/orders/{id:regex(^\\\\\\\\d{{3}}-\\\\\\\\d{{2}}-\\\\\\\\d{{4}}$)}\"")]
        [TestCase("@\"api/orders/{id:regex(↓^\\d{3}-\\d{2}-\\d{4}$)}\"",      "@\"api/orders/{id:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{{4}}$)}\"")]
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
    }
}
