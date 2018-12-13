namespace AspNetCoreAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new ObjectType();
        internal static readonly StringType String = new StringType();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly QualifiedType DateTime = Create("System.DateTime");
        internal static readonly QualifiedType Decimal = Create("System.Decimal", "decimal");
        internal static readonly QualifiedType Double = Create("System.Double", "double");
        internal static readonly QualifiedType Float = Create("System.Single", "float");
        internal static readonly QualifiedType Guid = Create("System.Guid");
        internal static readonly QualifiedType Int32 = Create("System.Int32", "int");
        internal static readonly QualifiedType Int64 = Create("System.Int64", "long");
        internal static readonly QualifiedType Attribute = Create("System.Attribute");

        internal static readonly QualifiedType AcceptVerbsAttribute = Create("Microsoft.AspNetCore.Mvc.AcceptVerbsAttribute");
        internal static readonly QualifiedType ActionContextAttribute = Create("Microsoft.AspNetCore.Mvc.ActionContextAttribute");
        internal static readonly QualifiedType ActionNameAttribute = Create("Microsoft.AspNetCore.Mvc.ActionNameAttribute");
        internal static readonly QualifiedType ApiControllerAttribute = Create("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
        internal static readonly QualifiedType ApiExplorerSettingsAttribute = Create("Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute");
        internal static readonly QualifiedType AreaAttribute = Create("Microsoft.AspNetCore.Mvc.AreaAttribute");
        internal static readonly QualifiedType BindAttribute = Create("Microsoft.AspNetCore.Mvc.BindAttribute");
        internal static readonly QualifiedType BindPropertiesAttribute = Create("Microsoft.AspNetCore.Mvc.BindPropertiesAttribute");
        internal static readonly QualifiedType BindPropertyAttribute = Create("Microsoft.AspNetCore.Mvc.BindPropertyAttribute");
        internal static readonly QualifiedType ConsumesAttribute = Create("Microsoft.AspNetCore.Mvc.ConsumesAttribute");
        internal static readonly QualifiedType ControllerAttribute = Create("Microsoft.AspNetCore.Mvc.ControllerAttribute");
        internal static readonly QualifiedType ControllerContextAttribute = Create("Microsoft.AspNetCore.Mvc.ControllerContextAttribute");
        internal static readonly QualifiedType DisableRequestSizeLimitAttribute = Create("Microsoft.AspNetCore.Mvc.DisableRequestSizeLimitAttribute");
        internal static readonly QualifiedType MiddlewareFilterAttribute = Create("Microsoft.AspNetCore.Mvc.MiddlewareFilterAttribute");
        internal static readonly QualifiedType FormatFilterAttribute = Create("Microsoft.AspNetCore.Mvc.FormatFilterAttribute");
        internal static readonly QualifiedType FromBodyAttribute = Create("Microsoft.AspNetCore.Mvc.FromBodyAttribute");
        internal static readonly QualifiedType FromFormAttribute = Create("Microsoft.AspNetCore.Mvc.FromFormAttribute");
        internal static readonly QualifiedType FromHeaderAttribute = Create("Microsoft.AspNetCore.Mvc.FromHeaderAttribute");
        internal static readonly QualifiedType FromQueryAttribute = Create("Microsoft.AspNetCore.Mvc.FromQueryAttribute");
        internal static readonly QualifiedType FromRouteAttribute = Create("Microsoft.AspNetCore.Mvc.FromRouteAttribute");
        internal static readonly QualifiedType FromServicesAttribute = Create("Microsoft.AspNetCore.Mvc.FromServicesAttribute");
        internal static readonly QualifiedType ModelBinderAttribute = Create("Microsoft.AspNetCore.Mvc.ModelBinderAttribute");
        internal static readonly QualifiedType ModelMetadataTypeAttribute = Create("Microsoft.AspNetCore.Mvc.ModelMetadataTypeAttribute");
        internal static readonly QualifiedType NonActionAttribute = Create("Microsoft.AspNetCore.Mvc.NonActionAttribute");
        internal static readonly QualifiedType NonControllerAttribute = Create("Microsoft.AspNetCore.Mvc.NonControllerAttribute");
        internal static readonly QualifiedType NonViewComponentAttribute = Create("Microsoft.AspNetCore.Mvc.NonViewComponentAttribute");
        internal static readonly QualifiedType ProducesAttribute = Create("Microsoft.AspNetCore.Mvc.ProducesAttribute");
        internal static readonly QualifiedType ProducesResponseTypeAttribute = Create("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
        internal static readonly QualifiedType RequestFormLimitsAttribute = Create("Microsoft.AspNetCore.Mvc.RequestFormLimitsAttribute");
        internal static readonly QualifiedType RequestSizeLimitAttribute = Create("Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute");
        internal static readonly QualifiedType RequireHttpsAttribute = Create("Microsoft.AspNetCore.Mvc.RequireHttpsAttribute");
        internal static readonly QualifiedType ResponseCacheAttribute = Create("Microsoft.AspNetCore.Mvc.ResponseCacheAttribute");
        internal static readonly QualifiedType RouteAttribute = Create("Microsoft.AspNetCore.Mvc.RouteAttribute");
        internal static readonly QualifiedType ServiceFilterAttribute = Create("Microsoft.AspNetCore.Mvc.ServiceFilterAttribute");
        internal static readonly QualifiedType TypeFilterAttribute = Create("Microsoft.AspNetCore.Mvc.TypeFilterAttribute");
        internal static readonly QualifiedType RouteValueAttribute = Create("Microsoft.AspNetCore.Mvc.Routing.RouteValueAttribute");
        internal static readonly QualifiedType BindingBehaviorAttribute = Create("Microsoft.AspNetCore.Mvc.ModelBinding.BindingBehaviorAttribute");
        internal static readonly QualifiedType BindNeverAttribute = Create("Microsoft.AspNetCore.Mvc.ModelBinding.BindNeverAttribute");
        internal static readonly QualifiedType BindRequiredAttribute = Create("Microsoft.AspNetCore.Mvc.ModelBinding.BindRequiredAttribute");
        internal static readonly QualifiedType ValidateNeverAttribute = Create("Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNeverAttribute");
        internal static readonly QualifiedType ActionFilterAttribute = Create("Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute");
        internal static readonly QualifiedType ExceptionFilterAttribute = Create("Microsoft.AspNetCore.Mvc.Filters.ExceptionFilterAttribute");
        internal static readonly QualifiedType ResultFilterAttribute = Create("Microsoft.AspNetCore.Mvc.Filters.ResultFilterAttribute");
        internal static readonly QualifiedType ProvideApplicationPartFactoryAttribute = Create("Microsoft.AspNetCore.Mvc.ApplicationParts.ProvideApplicationPartFactoryAttribute");
        internal static readonly QualifiedType RelatedAssemblyAttribute = Create("Microsoft.AspNetCore.Mvc.ApplicationParts.RelatedAssemblyAttribute");
        internal static readonly QualifiedType ActionMethodSelectorAttribute = Create("Microsoft.AspNetCore.Mvc.ActionConstraints.ActionMethodSelectorAttribute");

        internal static readonly QualifiedType HttpGetAttribute = Create("Microsoft.AspNetCore.Mvc.HttpGetAttribute");
        internal static readonly QualifiedType HttpPostAttribute = Create("Microsoft.AspNetCore.Mvc.HttpPostAttribute");
        internal static readonly QualifiedType HttpPutAttribute = Create("Microsoft.AspNetCore.Mvc.HttpPutAttribute");
        internal static readonly QualifiedType HttpDeleteAttribute = Create("Microsoft.AspNetCore.Mvc.HttpDeleteAttribute");
        internal static readonly QualifiedType HttpHeadAttribute = Create("Microsoft.AspNetCore.Mvc.HttpHeadAttribute");
        internal static readonly QualifiedType HttpOptionsAttribute = Create("Microsoft.AspNetCore.Mvc.HttpOptionsAttribute");
        internal static readonly QualifiedType HttpPatchAttribute = Create("Microsoft.AspNetCore.Mvc.HttpPatchAttribute");
        internal static readonly QualifiedType HttpMethodAttribute = Create("Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute");

        private static QualifiedType Create(string qualifiedName, string alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
