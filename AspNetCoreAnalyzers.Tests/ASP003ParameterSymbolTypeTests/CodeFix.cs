namespace AspNetCoreAnalyzers.Tests.ASP003ParameterSymbolTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
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
        public static void WhenHttpGet(string template, string parameter)
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult Get(↓byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", template);

            var after = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id}"")]
        public IActionResult Get(byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("\"api/orders/{id}\"", template)
  .AssertReplace("byte id", parameter);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenHttpGetAndRouteOnClass()
        {
            var before = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/orders/{id:int}"")]
    [Route(""api/orders"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult Get(↓byte id)
        {
            return this.Ok(id);
        }

        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}";

            var after = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [Route(""api/orders/{id:int}"")]
    [Route(""api/orders"")]
    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult Get(int id)
        {
            return this.Ok(id);
        }

        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        public static void RemoveNullableWhenNotOptional(string parameter)
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void MakeNullableToMatchOptional()
        {
            var before = @"
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

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
