namespace AspNetCoreAnalyzers.Tests.ASP003ParameterTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = Gu.Roslyn.Asserts.ExpectedDiagnostic.Create(ASP003ParameterType.Descriptor);
        private static readonly CodeFixProvider Fix = new ParameterTypeFix();

        [TestCase("api/orders/{id:int}", "int id")]
        [TestCase("api/orders/{id:bool}", "bool id")]
        [TestCase("api/orders/{id:datetime}", "System.DateTime id")]
        [TestCase("api/orders/{id:decimal}", "decimal id")]
        [TestCase("api/orders/{id:double}", "double id")]
        [TestCase("api/orders/{id:float}", "float id")]
        [TestCase("api/orders/{id:guid}", "System.Guid id")]
        [TestCase("api/orders/{id:long}", "long id")]
        [TestCase("api/orders/{id:alpha}", "string id")]
        public void ExplicitType(string template, string parameter)
        {
            var code = @"
namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:int}"")]
        public IActionResult GetId(↓byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("api/orders/{id:int}", template);

            var fixedCode = @"
namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/orders/{id:int}"")]
        public IActionResult GetId(byte id)
        {
            return this.Ok(id);
        }
    }
}".AssertReplace("api/orders/{id:int}", template)
  .AssertReplace("byte id", parameter);
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, fixedCode);
        }
    }
}
