using Blueprint.Api.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Api.Configuration
{
    public class BlueprintValidationConfigurer
    {
        private readonly BlueprintMiddlewareConfigurer middlewareConfigurer;

        internal BlueprintValidationConfigurer(BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            this.middlewareConfigurer = middlewareConfigurer;

            middlewareConfigurer.Services.TryAddSingleton<IValidator, BlueprintValidator>();
        }

        /// <summary>
        /// Adds the <see cref="BlueprintValidationSourceBuilder"/> validation builder which will use any <see cref="BlueprintValidationAttribute"/>s applied
        /// to the properties of an <see cref="IApiOperation"/>.
        /// </summary>
        /// <remarks>
        /// Blueprint validation attributes are similar to DataAnnotations but have access to <see cref="ApiOperationContext" /> in addition to being
        /// async when validating.
        /// <returns>This configurer.</returns>
        public BlueprintValidationConfigurer UseBlueprintSource()
        {
            middlewareConfigurer.Services.AddSingleton<IValidationSource, BlueprintValidationSource>();
            middlewareConfigurer.Services.AddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();

            return this;
        }

        /// <summary>
        /// Adds a DataAnnotation validation builder which will use the standard DataAnnotations attributes, applied
        /// to the properties of an <see cref="IApiOperation"/>.
        /// </summary>
        /// <returns>This configurer.</returns>
        public BlueprintValidationConfigurer UseDataAnnotationSource()
        {
            middlewareConfigurer.Services.AddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            middlewareConfigurer.Services.AddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();

            return this;
        }
    }
}
