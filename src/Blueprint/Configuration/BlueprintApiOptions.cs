using System.Collections.Generic;
using System.Reflection;
using Blueprint.Compiler;
using Microsoft.CodeAnalysis;

namespace Blueprint.Configuration
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
            this.GenerationRules = new GenerationRules
            {
                OptimizationLevel = OptimizationLevel.Release,
            };
        }

        /// <summary>
        /// The data model that describes the structure of the API.
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
        /// The pipeline assembly, which is the entry assembly for this application and where precompiled
        /// pipeline types can be expected to be loaded from.
        /// </summary>
        public Assembly PipelineAssembly { get; set; }

        /// <summary>
        /// The folder into which generated pipeline code should be placed. This should be a folder that will
        /// be included in the <see cref="PipelineAssembly" /> on compilation to ensure the auto strategy can
        /// work.
        /// </summary>
        public string GeneratedCodeFolder { get; set; }
    }
}
