using System.Collections.Generic;
using Blueprint.Compiler;
using Microsoft.CodeAnalysis;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Serves as the central configuration point for the Blueprint API, providing the options for
    /// configuring behaviour as well as creating an <see cref="ApiDataModel" /> that represents the operations
    /// that can be executed.
    /// </summary>
    public class BlueprintApiOptions
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiOptions" /> class.
        /// </summary>
        public BlueprintApiOptions()
        {
            GenerationRules = new GenerationRules
            {
                OptimizationLevel = OptimizationLevel.Release,
            };

            BaseApiUrl = string.Empty;
        }

        /// <summary>
        /// The data model that describes the structure of the API (i.e. the <see cref="IApiOperation" />s, the
        /// <see cref="ApiResource" />s and associated <see cref="Link"/>s).
        /// </summary>
        public ApiDataModel Model { get; } = new ApiDataModel();

        /// <summary>
        /// The <see cref="Compiler.GenerationRules" /> that are used when compiling the pipelines of this API.
        /// </summary>
        public GenerationRules GenerationRules { get; }

        /// <summary>
        /// Gets the list of middleware builder that will be used by the pipeline generation, added in the
        /// order that they will be configured.
        /// </summary>
        public List<IMiddlewareBuilder> MiddlewareBuilders { get; } = new List<IMiddlewareBuilder>();

        /// <summary>
        /// Gets the name of the application, which is used when generating the DLL for the pipeline
        /// executors.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the Base URL for the API, the fully-qualified URL that represents where the
        /// root of the API can be accessed from (i.e. https://api.somewhere.com/api/).
        /// </summary>
        public string BaseApiUrl { get; set; }

        /// <summary>
        /// The "host" of the API, i.e. a HTTP host or a background task processor.
        /// </summary>
        public IBlueprintApiHost Host { get; set; }
    }
}
