using AspNetCoreAnalyzers.Tests;
using Gu.Roslyn.Asserts;

[assembly: TransitiveMetadataReferences(
    typeof(ValidWithAllAnalyzers),
    typeof(ValidCode.Program))]
