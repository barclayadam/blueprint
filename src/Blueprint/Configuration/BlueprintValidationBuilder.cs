using Blueprint.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Configuration
{
    public class BlueprintValidationBuilder
    {
        private readonly BlueprintApiBuilder _apiBuilder;

        internal BlueprintValidationBuilder(BlueprintApiBuilder apiBuilder)
        {
            this._apiBuilder = apiBuilder;

            apiBuilder.Services.TryAddSingleton<IValidator, BlueprintValidator>();
        }

        /// <summary>
        /// Adds the <see cref="BlueprintValidationSourceBuilder"/> validation builder which will use any <see cref="BlueprintValidationAttribute"/>s applied
        /// to the properties of an operation.
        /// </summary>
        /// <remarks>
        /// Blueprint validation attributes are similar to DataAnnotations but have access to <see cref="ApiOperationContext" /> in addition to being
        /// async when validating.
        /// </remarks>
        /// <returns>This builder.</returns>
        public BlueprintValidationBuilder UseBlueprintSource()
        {
            this._apiBuilder.Services.AddSingleton<IValidationSource, BlueprintValidationSource>();
            this._apiBuilder.Services.AddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();

            return this;
        }

        /// <summary>
        /// Adds a DataAnnotation validation builder which will use the standard DataAnnotations attributes, applied
        /// to the properties of an operation.
        /// </summary>
        /// <returns>This builder.</returns>
        public BlueprintValidationBuilder UseDataAnnotationSource()
        {
            this._apiBuilder.Services.AddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            this._apiBuilder.Services.AddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();

            return this;
        }
    }
}
