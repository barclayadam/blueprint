using Blueprint.Api.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Api.Configuration
{
    public class BlueprintValidationConfigurer
    {
        private readonly BlueprintMiddlewareConfigurer middlewareConfigurer;

        public BlueprintValidationConfigurer(BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            this.middlewareConfigurer = middlewareConfigurer;

            middlewareConfigurer.Services.TryAddSingleton<IValidator, BlueprintValidator>();
        }

        public BlueprintValidationConfigurer UseBlueprintSource()
        {
            middlewareConfigurer.Services.TryAddSingleton<IValidationSource, BlueprintValidationSource>();
            middlewareConfigurer.Services.TryAddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();

            return this;
        }

        public BlueprintValidationConfigurer UseDataAnnotationSource()
        {
            middlewareConfigurer.Services.TryAddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            middlewareConfigurer.Services.TryAddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();

            return this;
        }
    }
}
