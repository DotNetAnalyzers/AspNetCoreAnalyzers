using System;
using AspNetCoreAnalyzers.Tests;
using Gu.Roslyn.Asserts;

[assembly: CLSCompliant(false)]

[assembly: TransitiveMetadataReferences(
    typeof(ValidWithAllAnalyzers),
    typeof(ValidCode.Program))]
