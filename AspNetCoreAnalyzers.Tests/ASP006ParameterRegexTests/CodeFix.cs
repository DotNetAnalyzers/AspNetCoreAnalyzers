namespace AspNetCoreAnalyzers.Tests.ASP006ParameterRegexTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP006ParameterRegex.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"api/orders/{id:regex(↓a{1})}\"",                         "\"api/orders/{id:regex(a{{1}})}\"")]
        [TestCase("\"api/orders/{id:regex(↓^[a-z]{2}$)}\"",                   "\"api/orders/{id:regex(^[[a-z]]{{2}}$)}\"")]
        [TestCase("\"api/orders/{id:regex(↓\\\\d+)}\"",                       "\"api/orders/{id:regex(\\\\\\\\d+)}\"")]
        [TestCase("@\"api/orders/{id:regex(↓\\d+)}\"",                        "@\"api/orders/{id:regex(\\\\d+)}\"")]
        [TestCase("\"api/orders/{id:regex(↓^\\\\d{3}-\\\\d{2}-\\\\d{4}$)}\"", "\"api/orders/{id:regex(^\\\\\\\\d{{3}}-\\\\\\\\d{{2}}-\\\\\\\\d{{4}}$)}\"")]
        [TestCase("@\"api/orders/{id:regex(↓^\\d{3}-\\d{2}-\\d{4}$)}\"",      "@\"api/orders/{id:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{{4}}$)}\"")]
        public void When(string before, string after)
        {
            var code = @"
namespace ValidCode
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
namespace ValidCode
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
