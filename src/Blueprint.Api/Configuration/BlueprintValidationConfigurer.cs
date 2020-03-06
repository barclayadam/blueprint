﻿using Blueprint.Api.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Api.Configuration
{
    public class BlueprintValidationConfigurer
    {
        private readonly BlueprintPipelineBuilder pipelineBuilder;

        internal BlueprintValidationConfigurer(BlueprintPipelineBuilder pipelineBuilder)
        {
            this.pipelineBuilder = pipelineBuilder;

            pipelineBuilder.Services.TryAddSingleton<IValidator, BlueprintValidator>();
        }

        /// <summary>
        /// Adds the <see cref="BlueprintValidationSourceBuilder"/> validation builder which will use any <see cref="BlueprintValidationAttribute"/>s applied
        /// to the properties of an <see cref="IApiOperation"/>.
        /// </summary>
        /// <remarks>
        /// Blueprint validation attributes are similar to DataAnnotations but have access to <see cref="ApiOperationContext" /> in addition to being
        /// async when validating.
        /// </remarks>
        /// <returns>This configurer.</returns>
        public BlueprintValidationConfigurer UseBlueprintSource()
        {
            pipelineBuilder.Services.AddSingleton<IValidationSource, BlueprintValidationSource>();
            pipelineBuilder.Services.AddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();

            return this;
        }

        /// <summary>
        /// Adds a DataAnnotation validation builder which will use the standard DataAnnotations attributes, applied
        /// to the properties of an <see cref="IApiOperation"/>.
        /// </summary>
        /// <returns>This configurer.</returns>
        public BlueprintValidationConfigurer UseDataAnnotationSource()
        {
            pipelineBuilder.Services.AddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            pipelineBuilder.Services.AddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();

            return this;
        }
    }
}
