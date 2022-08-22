using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;

namespace Blueprint.OpenApi;

/// <summary>
/// The options for OpenAPI generation.
/// </summary>
[PublicAPI]
public class OpenApiOptions
{
    private readonly List<Type> _schemaProcessors = new List<Type>();

    /// <summary>
    /// Called when the OpenAPI document has been fully constructed. Allows for modification of
    /// existing resources and to, in particular, add app-specific settings such as authentication
    /// and generation information (i.e. <see cref="OpenApiDocument.Info" />.
    /// </summary>
    [CanBeNull]
    public Action<OpenApiDocument> PostConfigure { get; set; }

    /// <summary>
    /// Called for every <see cref="OpenApiOperation" /> that is generated, with the <see cref="ApiOperationDescriptor" />
    /// that it was generated from, allowing post-creation modifications.
    /// </summary>
    [CanBeNull]
    public Action<OpenApiOperation, ApiOperationDescriptor> ConfigureOperation { get; set; }

    /// <summary>
    /// Called to enable the modification of the <see cref="JsonSchemaGeneratorSettings" /> used during
    /// the creation of the OpenAPI document. Note that the settings have already been configured and
    /// you risk modifying / breaking functionality if you completely change parts of the settings.
    /// </summary>
    [CanBeNull]
    public Action<JsonSchemaGeneratorSettings> ConfigureSettings { get; set; }

    /// <summary>
    /// An optional function that can be used to create the <see cref="JsonSchemaGenerator" /> to be used, passed in
    /// the fully configured <see cref="JsonSchemaGeneratorSettings" />.
    /// </summary>
    [CanBeNull]
    public Func<IServiceProvider, ApiDataModel, JsonSchemaGeneratorSettings, JsonSchemaGenerator> CreateGenerator { get; set; }

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
    public IReadOnlyList<Type> SchemaProcessors => this._schemaProcessors;

    /// <summary>
    /// The version of Redoc (https://github.com/Redocly/redoc) to use for the OpenApi documentation
    /// endpoint.
    /// </summary>
    public string RedocVersion { get; set; } = "latest";

    /// <summary>
    /// Adds the given schema processor.
    /// </summary>
    /// <typeparam name="T">The schema processor type to add.</typeparam>
    public void AddSchemaProcessor<T>() where T : ISchemaProcessor
    {
        this._schemaProcessors.Add(typeof(T));
    }
}