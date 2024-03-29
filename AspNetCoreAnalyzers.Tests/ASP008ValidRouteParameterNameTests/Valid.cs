﻿namespace AspNetCoreAnalyzers.Tests.ASP008ValidRouteParameterNameTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

public static class Valid
{
    private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

    [TestCase("\"{value}\"",                                                                  "string")]
    [TestCase("\"{value?}\"",                                                                 "string")]
    [TestCase("\"{value:bool}\"",                                                             "bool")]
    [TestCase("\"{value:datetime}\"",                                                         "System.DateTime")]
    [TestCase("\"{value:decimal}\"",                                                          "decimal")]
    [TestCase("\"{value:double}\"",                                                           "double")]
    [TestCase("\"{value:float}\"",                                                            "float")]
    [TestCase("\"{value:int}\"",                                                              "int")]
    [TestCase("\"api/orders/{value:int:min(1)}\"",                                            "int")]
    [TestCase("\"api/orders/{value:int:max(1)}\"",                                            "int")]
    [TestCase("\"api/orders/{value:int:range(1,10)}\"",                                       "int")]
    [TestCase("\"api/orders/{value:int:required}\"",                                          "int")]
    [TestCase("\"{value:long}\"",                                                             "long")]
    [TestCase("\"api/orders/{value:min(1)}\"",                                                "long")]
    [TestCase("\"api/orders/{value:max(1)}\"",                                                "long")]
    [TestCase("\"api/orders/{value:range(1,10)}\"",                                           "long")]
    [TestCase("\"api/orders/{value:required}\"",                                              "long")]
    [TestCase("\"{value:guid}\"",                                                             "System.Guid")]
    [TestCase("\"api/orders/{value:minlength(1)}\"",                                          "string")]
    [TestCase("\"api/orders/{value:maxlength(1)}\"",                                          "string")]
    [TestCase("\"api/orders/{value:length(1)}\"",                                             "string")]
    [TestCase("\"api/orders/{value:length(1,3)}\"",                                           "string")]
    [TestCase("\"api/orders/{value:alpha}\"",                                                 "string")]
    [TestCase("\"api/orders/{value:regex(a-(0|1))}\"",                                        "string")]
    [TestCase("\"api/orders/{value:regex(^\\\\\\\\d{{3}}-\\\\\\\\d{{2}}-\\\\\\\\d{{4}}$)}\"", "string")]
    [TestCase("@\"api/orders/{value:regex(^\\\\\\\\d{{3}}-\\\\\\\\d{{2}}-\\\\\\\\d{{4}}$)}\"", "string")]
    [TestCase("@\"api/orders/{value:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{{4}}$)}\"", "string")]
    [TestCase("\"api/orders/{value:required}\"",                                              "string")]
    public static void WithParameter(string parameter, string type)
    {
        var code = @"
namespace AspBox
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class OrdersController : Controller
    {
        [HttpGet(""api/{value}"")]
        public IActionResult GetValue(string value)
        {
            return this.Ok(value);
        }
    }
}".AssertReplace("\"api/{value}\"", parameter)
.AssertReplace("string", type);
        RoslynAssert.Valid(Analyzer, code);
    }
}
