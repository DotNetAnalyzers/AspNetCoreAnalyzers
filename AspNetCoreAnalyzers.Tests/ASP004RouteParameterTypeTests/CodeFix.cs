namespace AspNetCoreAnalyzers.Tests.ASP004RouteParameterTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP004RouteParameterType.Descriptor);
        private static readonly CodeFixProvider Fix = new TemplateTextFix();

        [TestCase("\"{id:↓float}\"",                   "\"{id:int}\"",                   "int id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:int}\"",        "int id")]
        [TestCase("\"api/orders/{id:↓float:min(1)}\"", "\"api/orders/{id:int:min(1)}\"", "int id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:long}\"",       "long id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:bool}\"",       "bool id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:datetime}\"",   "System.DateTime id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:decimal}\"",    "decimal id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:double}\"",     "double id")]
        [TestCase("\"api/orders/{id:↓int}\"",          "\"api/orders/{id:float}\"",      "float id")]
        [TestCase("\"api/orders/{id:↓float}\"",        "\"api/orders/{id:guid}\"",       "System.Guid id")]
        public void When(string before, string after, string parameter)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", before)
  .AssertReplace("byte id", parameter);

            var fixedCode = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", after)
  .AssertReplace("byte id", parameter);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("\"api/orders/{id:↓minlength(1)}\"")]
        [TestCase("\"api/orders/{id:↓maxlength(1)}\"")]
        [TestCase("\"api/orders/{id:↓length(1)}\"")]
        [TestCase("\"api/orders/{id:↓length(1,3)}\"")]
        [TestCase("\"api/orders/{id:↓min(1)}\"")]
        [TestCase("\"api/orders/{id:↓max(10)}\"")]
        [TestCase("\"api/orders/{id:↓range(0,10)}\"")]
        [TestCase("\"api/orders/{id:↓alpha}\"")]
        [TestCase("\"api/orders/{id:↓regex(a-(0|1))}\"")]
        [TestCase("\"api/orders/{id:↓regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"")]
        [TestCase("@\"api/orders/{id:↓regex(^\\d{{3}}-\\d{{2}}-\\d{4}$)}\"")]
        [TestCase("@\"api/orders/{id:↓regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"")]
        public void NoFixWhen(string template)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", template);

            AnalyzerAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        public void WhenOptional(string parameter)
        {
            var code = @"
namespace AspBox
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{↓id}"")]
        public IActionResult GetId(int? id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("int?", parameter);

            var fixedCode = @"
namespace AspBox
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id?}"")]
        public IActionResult GetId(int? id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("int?", parameter);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
