namespace AspNetCoreAnalyzers.Tests.ASP009LowerCaseUrlsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [TestCase("\"{value}\"")]
        [TestCase("\"api/orders/{value}\"")]
        [TestCase("\"api/TwoWords/{value}\"")]
        public void WithParameter(string parameter)
        {
            var code = @"
namespace ValidCode
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{value}"")]
        public IActionResult GetValue(string value)
        {
            return this.Ok(value);
        }
    }
}".AssertReplace("\"api/{value}\"", parameter);
            AnalyzerAssert.Valid(Analyzer, code);
        }
    }
}
