namespace AspNetCoreAnalyzers.Tests.ASP011RouteParameterNameMustBeUniqueTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [TestCase("\"{value}\"")]
        [TestCase("\"api/orders/{value}\"")]
        [TestCase("\"api/two-words/{value}\"")]
        public static void WithParameter(string parameter)
        {
            var code = @"
namespace AspBox
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void TwoActions()
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class RegexController : Controller
    {
        [HttpGet(""get1/{value:regex(a{{0,1}})}"")]
        public IActionResult Get1(string value)
        {
            return this.Ok(value);
        }

        [HttpGet(""get2/{value:regex(\\\\d+)}"")]
        public IActionResult Get2(string value)
        {
            return this.Ok(value);
        }

        [HttpGet(@""get3/{value:regex(\\d+)}"")]
        public IActionResult Get3(string value)
        {
            return this.Ok(value);
        }

        [HttpGet(""get4/{value:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{{4}}$)}"")]
        public IActionResult Get4(string value)
        {
            return this.Ok(value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
