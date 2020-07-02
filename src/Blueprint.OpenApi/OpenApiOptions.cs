using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;

namespace Blueprint.OpenApi
{
    /// <summary>
    /// The options for OpenAPI generation.
    /// </summary>
    [PublicAPI]
    public class OpenApiOptions
    {
        private readonly List<Type> schemaProcessors = new List<Type>();

        /// <summary>
        /// Called when the OpenAPI document has been fully constructed. Allows for modification of
        /// existing resources and to, in particular, add app-specific settings such as authentication
        /// and generation information (i.e. <see cref="OpenApiDocument.Info" />.
        /// </summary>
        public Action<OpenApiDocument> PostConfigure { get; set; }

        /// <summary>
        /// Called to enable the modification of the <see cref="JsonSchemaGeneratorSettings" /> used during
        /// the creation of the OpenAPI document. Note that the settings have already been configured and
        /// you risk modifying / breaking functionality if you completely change parts of the settings.
        /// </summary>
        public Action<JsonSchemaGeneratorSettings> ConfigureSettings { get; set; }

        /// <summary>
        /// An optional function that can be used to create the <see cref="JsonSchemaGenerator" /> to be used, passed in
        /// the fully configured <see cref="JsonSchemaGeneratorSettings" />.
        /// </summary>
        [CanBeNull]
        public Func<JsonSchemaGeneratorSettings, JsonSchemaGenerator> CreateGenerator { get; set; }

        /// <summary>
        /// The schema type to generate, defaults to <see cref="NJsonSchema.SchemaType.OpenApi3"/>
        /// </summary>
        public SchemaType SchemaType { get; set; } = SchemaType.OpenApi3;

        /// <summary>
        /// The formatting to apply to the output JSON, defaults to <see cref="Newtonsoft.Json.Formatting.Indented" />.
        /// </summary>
        public Formatting Formatting { get; set; } = Formatting.Indented;

        /// <summary>
        /// A list of <see cref="ISchemaProcessor" /> types to instantiate and add to the
        /// settings to enable some custom processing of generated schemas.
        /// </summary>
        public IReadOnlyList<Type> SchemaProcessors => schemaProcessors;

        /// <summary>
        /// Adds the given schema processor.
        /// </summary>
        /// <typeparam name="T">The schema processor type to add.</typeparam>
        public void AddSchemaProcessor<T>() where T : ISchemaProcessor
        {
            schemaProcessors.Add(typeof(T));
        }
    }
}
