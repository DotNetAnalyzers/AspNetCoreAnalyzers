namespace AspNetCoreAnalyzers.Tests;

using System.Runtime.CompilerServices;
using Gu.Roslyn.Asserts;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Settings.Default = Settings.Default.WithMetadataReferences(
            MetadataReferences.Transitive(
                typeof(Microsoft.EntityFrameworkCore.DbContext),
                typeof(Microsoft.Extensions.Hosting.GenericHostBuilderExtensions),
                typeof(Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions),
                typeof(Microsoft.AspNetCore.Builder.HttpsPolicyBuilderExtensions)));
    }
}
