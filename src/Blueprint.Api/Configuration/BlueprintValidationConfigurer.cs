using Blueprint.Api.Middleware;
using Blueprint.Api.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintValidationConfigurer
    {
        private readonly BlueprintApiConfigurer blueprintApiConfigurer;

        public BlueprintValidationConfigurer(BlueprintApiConfigurer blueprintApiConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;

            blueprintApiConfigurer.Services.AddSingleton<IValidator, BlueprintValidator>();
            blueprintApiConfigurer.AddMiddlewareBuilderToStage<ValidationMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.OperationChecks);
        }

        public BlueprintValidationConfigurer UseBlueprintSource()
        {
            blueprintApiConfigurer.Services.AddScoped<IValidationSource, BlueprintValidationSource>();
            blueprintApiConfigurer.Services.AddScoped<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();

            return this;
        }

        public BlueprintValidationConfigurer UseDataAnnotationSource()
        {
            blueprintApiConfigurer.Services.AddScoped<IValidationSource, DataAnnotationsValidationSource>();
            blueprintApiConfigurer.Services.AddScoped<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();

            return this;
        }
    }
}
