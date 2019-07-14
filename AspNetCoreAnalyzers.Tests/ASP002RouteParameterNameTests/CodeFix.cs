namespace AspNetCoreAnalyzers.Tests.ASP002RouteParameterNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP002RouteParameterName.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"api/{↓value}\"",       "\"api/{text}\"")]
        [TestCase("\"api/{↓text*}\"",       "\"api/{text}\"")]
        [TestCase("@\"api/{↓value}\"",      "@\"api/{text}\"")]
        [TestCase("\"api/{↓value:alpha}\"", "\"api/{text:alpha}\"")]
        public static void WhenHttpGet(string before, string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{↓value}"")]
        public IActionResult GetValue(string text)
        {
            return this.Ok(text);
        }
    }
}".AssertReplace("\"api/{↓value}\"", before);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{text}"")]
        public IActionResult GetValue(string text)
        {
            return this.Ok(text);
        }
    }
}".AssertReplace("\"api/{text}\"", after);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public static void WhenRouteAndHttpGetOnMethod()
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [Route(""api/values/{↓wrong}"")]
        [HttpGet]
        public IActionResult GetId(string value)
        {
            return this.Ok(value);
        }
    }
}";

            var after = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [Route(""api/values/{value}"")]
        [HttpGet]
        public IActionResult GetId(string value)
        {
            return this.Ok(value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("\"api/{text1}/{↓value}\"",                                                        "\"api/{text1}/{text2}\"")]
        [TestCase("\"api/{↓value}/{text2}\"",                                                        "\"api/{text1}/{text2}\"")]
        [TestCase("\"api/{text1:regex(\\\\d+)}/{↓value}\"",                                          "\"api/{text1:regex(\\\\d+)}/{text2}\"")]
        [TestCase("\"api/{text1:regex(\\\\\\\\d+)}/{↓value}\"",                                      "\"api/{text1:regex(\\\\\\\\d+)}/{text2}\"")]
        [TestCase("@\"api/{text1:regex(\\d+)}/{↓value}\"",                                           "@\"api/{text1:regex(\\d+)}/{text2}\"")]
        [TestCase("\"api/{text1::regex(^\\\\\\\\d{{3}}-\\\\\\\\d{{2}}-\\\\\\\\d{{4}}$)}/{↓value}\"", "\"api/{text1::regex(^\\\\\\\\d{{3}}-\\\\\\\\d{{2}}-\\\\\\\\d{{4}}$)}/{text2}\"")]
        public static void WhenWrongNameSecondParameter(string before, string after)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{text1}/{↓value}"")]
        public IActionResult GetValue(string text1, string text2)
        {
            return this.Ok(text1 + text2);
        }
    }
}".AssertReplace("\"api/{text1}/{↓value}\"", before);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{text1}/{text2}"")]
        public IActionResult GetValue(string text1, string text2)
        {
            return this.Ok(text1 + text2);
        }
    }
}".AssertReplace("\"api/{text1}/{text2}\"", after);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
