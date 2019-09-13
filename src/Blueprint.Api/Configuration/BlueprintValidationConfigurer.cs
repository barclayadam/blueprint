using Blueprint.Api.Middleware;
using Blueprint.Api.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintValidationConfigurer
    {
        private readonly BlueprintConfigurer blueprintConfigurer;

        public BlueprintValidationConfigurer(BlueprintConfigurer blueprintConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintConfigurer = blueprintConfigurer;

            blueprintConfigurer.Services.AddSingleton<IValidator, BlueprintValidator>();
            blueprintConfigurer.AddMiddlewareBuilderToStage<ValidationMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.OperationChecks);
        }

        public BlueprintValidationConfigurer UseBlueprintSource()
        {
            blueprintConfigurer.Services.AddScoped<IValidationSource, BlueprintValidationSource>();
            blueprintConfigurer.Services.AddScoped<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();

            return this;
        }

        public BlueprintValidationConfigurer UseDataAnnotationSource()
        {
            blueprintConfigurer.Services.AddScoped<IValidationSource, DataAnnotationsValidationSource>();
            blueprintConfigurer.Services.AddScoped<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();

            return this;
        }
    }
}
