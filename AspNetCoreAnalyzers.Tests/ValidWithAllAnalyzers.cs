namespace AspNetCoreAnalyzers.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

public static class ValidWithAllAnalyzers
{
    private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
        typeof(Descriptors)
            .Assembly
            .GetTypes()
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

    private static readonly Solution AnalyzersProjectSolution = CodeFactory.CreateSolution(
        ProjectFile.Find("AspNetCoreAnalyzers.csproj"));

    private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
        ProjectFile.Find("ValidCode.csproj"));

    [Test]
    public static void NotEmpty()
    {
        CollectionAssert.IsNotEmpty(AllAnalyzers);
        Assert.Pass($"Count: {AllAnalyzers.Count}");
    }

    [TestCaseSource(nameof(AllAnalyzers))]
    public static void ValidCodeProject(DiagnosticAnalyzer analyzer)
    {
        RoslynAssert.Valid(analyzer, ValidCodeProjectSln);
    }

    [TestCaseSource(nameof(AllAnalyzers))]
    public static void AnalyzersSolution(DiagnosticAnalyzer analyzer)
    {
        Assert.Inconclusive("Does not figure out source package.");
        RoslynAssert.NoAnalyzerDiagnostics(analyzer, AnalyzersProjectSolution);
    }
}
