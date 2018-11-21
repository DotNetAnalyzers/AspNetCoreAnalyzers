namespace AspNetCoreAnalyzers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCodeWithAllAnalyzers
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(HelpLink)
                                                                                 .Assembly.GetTypes()
                                                                                 .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                 .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                 .ToArray();

        private static readonly Solution AnalyzersProjectSolution = CodeFactory.CreateSolution(
            ProjectFile.Find("AspNetCoreAnalyzers.csproj"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        [SetUp]
        public void Setup()
        {
            // The cache will be enabled when running in VS.
            // It speeds up the tests and makes them more realistic
            Cache<SyntaxTree, SemanticModel>.Begin();
        }

        [TearDown]
        public void TearDown()
        {
            Cache<SyntaxTree, SemanticModel>.End();
        }

        [Test]
        public void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void ValidCodeProject(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, ValidCodeProjectSln);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void AnalyzersSolution(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, AnalyzersProjectSolution);
        }
    }
}
