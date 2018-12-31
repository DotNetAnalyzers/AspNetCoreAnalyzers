namespace AspNetCoreAnalyzers.Tests.ASP003ParameterSymbolTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(ASP003ParameterSymbolType.Descriptor);
        private static readonly CodeFixProvider Fix = new ParameterTypeFix();

        [TestCase("\"{id:int}\"",                                                 "int id")]
        [TestCase("\"api/orders/{id:int}\"",                                      "int id")]
        [TestCase("\"api/orders/{id:int:min(1)}\"",                               "int id")]
        [TestCase("\"api/orders/{id:bool}\"",                                     "bool id")]
        [TestCase("\"api/orders/{id:datetime}\"",                                 "System.DateTime id")]
        [TestCase("\"api/orders/{id:decimal}\"",                                  "decimal id")]
        [TestCase("\"api/orders/{id:double}\"",                                   "double id")]
        [TestCase("\"api/orders/{id:float}\"",                                    "float id")]
        [TestCase("\"api/orders/{id:guid}\"",                                     "System.Guid id")]
        [TestCase("\"api/orders/{id:long}\"",                                     "long id")]
        [TestCase("\"api/orders/{id:minlength(1)}\"",                             "string id")]
        [TestCase("\"api/orders/{id:maxlength(1)}\"",                             "string id")]
        [TestCase("\"api/orders/{id:length(1)}\"",                                "string id")]
        [TestCase("\"api/orders/{id:length(1,3)}\"",                              "string id")]
        [TestCase("\"api/orders/{id:min(1)}\"",                                   "long id")]
        [TestCase("\"api/orders/{id:max(10)}\"",                                  "long id")]
        [TestCase("\"api/orders/{id:range(0,10)}\"",                              "long id")]
        [TestCase("\"api/orders/{id:alpha}\"",                                    "string id")]
        [TestCase("\"api/orders/{id:regex(a-(0|1))}\"",                           "string id")]
        [TestCase("\"api/orders/{id:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"",  "string id")]
        [TestCase("@\"api/orders/{id:regex(^\\d{{3}}-\\d{{2}}-\\d{4}$)}\"",       "string id")]
        [TestCase("@\"api/orders/{id:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{4}$)}\"", "string id")]
        public void When(string template, string parameter)
        {
            var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(↓byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", template);

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
}".AssertReplace("\"api/orders/{id}\"", template)
  .AssertReplace("byte id", parameter);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        public void RemoveNullableWhenNotOptional(string parameter)
        {
            var code = @"
namespace AspBox
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(↓int? id)
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
        [HttpGet(""api/orders/{id}"")]
        public IActionResult GetId(int id)
        {
            return this.Ok(id);
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void MakeNullableToMatchOptional()
        {
            var code = @"
namespace AspBox
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id?}"")]
        public IActionResult GetId(↓int id)
        {
            return this.Ok(id);
        }
    }
}";

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
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
