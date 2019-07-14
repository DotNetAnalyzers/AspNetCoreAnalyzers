using AspNetCoreAnalyzers.Tests;
using Gu.Roslyn.Asserts;

[assembly: TransitiveMetadataReferences(
    typeof(ValidWithAllAnalyzers),
    typeof(Microsoft.EntityFrameworkCore.DbContext),
    typeof(Microsoft.AspNetCore.Mvc.Controller))]
