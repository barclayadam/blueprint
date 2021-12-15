using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Blueprint.Http;
using Blueprint.Middleware;
using Blueprint.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using XmlDocsExtensions = Blueprint.Utilities.XmlDocsExtensions;

namespace Blueprint.OpenApi
{
    internal class OpenApiDocumentBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiDataModel _apiDataModel;
        private readonly IEnumerable<IMessagePopulationSource> _messagePopulationSources;
        private readonly IOptions<OpenApiOptions> _options;

        /// <summary>
        /// Initialises a new instance of the <see cref="OpenApiDocumentBuilder" />.
        /// </summary>
        /// <param name="serviceProvider">Service provider used to create new <see cref="ISchemaProcessor" /> instances.</param>
        /// <param name="apiDataModel">The current data model.</param>
        /// <param name="messagePopulationSources">The registered message population sources.</param>
        /// <param name="options">The options to configure the OpenAPI document.</param>
        public OpenApiDocumentBuilder(
            IServiceProvider serviceProvider,
            ApiDataModel apiDataModel,
            IEnumerable<IMessagePopulationSource> messagePopulationSources,
            IOptions<OpenApiOptions> options)
        {
            this._serviceProvider = serviceProvider;
            this._apiDataModel = apiDataModel;
            this._messagePopulationSources = messagePopulationSources;
            this._options = options;
        }

        public OpenApiDocument Build()
        {
            var openApiOptions = this._options.Value;

            var document = new OpenApiDocument();

            var jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new BlueprintContractResolver(this._apiDataModel, this._messagePopulationSources),
                    Converters =
                    {
                        new StringEnumConverter(),
                    },
                },
                SchemaType = SchemaType.OpenApi3,
                FlattenInheritanceHierarchy = true,
                SchemaNameGenerator = new BlueprintSchemaNameGenerator(),
            };

            foreach (var processor in openApiOptions.SchemaProcessors)
            {
                jsonSchemaGeneratorSettings.SchemaProcessors.Add(
                    (ISchemaProcessor)ActivatorUtilities.CreateInstance(this._serviceProvider, processor, this._apiDataModel));
            }

            openApiOptions.ConfigureSettings?.Invoke(jsonSchemaGeneratorSettings);

            var generator = openApiOptions.CreateGenerator == null
                ? new BlueprintJsonSchemaGenerator(jsonSchemaGeneratorSettings)
                : openApiOptions.CreateGenerator(this._serviceProvider, this._apiDataModel, jsonSchemaGeneratorSettings);

            var openApiDocumentSchemaResolver = new OpenApiDocumentSchemaResolver(document, jsonSchemaGeneratorSettings);

            foreach (var operation in this._apiDataModel.Operations)
            {
                if (!operation.IsExposed)
                {
                    continue;
                }

                var httpData = operation.GetFeatureData<HttpOperationFeatureData>();

                foreach (var route in operation.Links)
                {
                    var pathUrl = "/" + route.RoutingUrl;

                    if (!document.Paths.TryGetValue(pathUrl, out var openApiPathItem))
                    {
                        openApiPathItem = new OpenApiPathItem();
                        document.Paths[pathUrl] = openApiPathItem;
                    }

                    var openApiOperation = new OpenApiOperation
                    {
                        OperationId = operation.Name,
                        Summary = XmlDocsExtensions.GetXmlDocsSummary(operation.OperationType),
                        Description = XmlDocsExtensions.GetXmlDocsRemarks(operation.OperationType),
                    };

                    // Use the last namespace segment as a tag of this operation, which provides a generally
                    // better structure when generating documentation or SDK clients
                    if (operation.OperationType.Namespace != null)
                    {
                        openApiOperation.Tags.Add(operation.OperationType.Namespace.Split('.').Last());
                    }

                    var httpMethod = operation.GetFeatureData<HttpOperationFeatureData>().HttpMethod;

                    var allOwned = this._messagePopulationSources
                        .SelectMany(s => s.GetOwnedProperties(this._apiDataModel, operation))
                        .ToList();

                    // First, add the explicit "owned" properties, those that we know come from a particular
                    // place and are therefore _not_ part of the body
                    foreach (var property in operation.Properties)
                    {
                        var ownedPropertyDescriptor = allOwned.SingleOrDefault(o => o.Property == property);

                        // We are only considering "owned" parameters here. All non-owned properties will
                        // be part of the "body" of this command, as handled below UNLESS this is a GET request,
                        // in which case we determine the parameter comes from the query (if not explicitly
                        // overriden)
                        if (ownedPropertyDescriptor == null && httpMethod != "GET")
                        {
                            continue;
                        }

                        // This owned property is internal and should never be exposed
                        if (ownedPropertyDescriptor?.IsInternal == true)
                        {
                            continue;
                        }

                        var isRoute = route.Placeholders.Any(p => p.Property == property);

                        openApiOperation.Parameters.Add(new OpenApiParameter
                        {
                            Kind = isRoute ? OpenApiParameterKind.Path : ToKind(property),
                            Name = ownedPropertyDescriptor?.PropertyName ?? property.Name,
                            IsRequired = isRoute ||
                                         property.ToContextualProperty().Nullability == Nullability.NotNullable ||
                                         property.GetCustomAttributes<RequiredAttribute>().Any(),
                            Schema = generator.Generate(property.PropertyType),
                            Description = XmlDocsExtensions.GetXmlDocsSummary(property),
                        });
                    }

                    // GETs will NOT have their body parsed, meaning we need not handle body parameter at all
                    if (httpMethod != "GET")
                    {
                        // The body schema would contain all non-owned properties (owned properties are
                        // handled above as coming from a specific part of the HTTP request).
                        var bodySchema = GetOrAddJsonSchema(operation.OperationType, document, generator, openApiDocumentSchemaResolver);

                        if (bodySchema != null)
                        {
                            openApiOperation.RequestBody = new OpenApiRequestBody
                            {
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = bodySchema,
                                    },
                                },
                            };
                        }
                    }

                    foreach (var response in operation.Responses)
                    {
                        var httpStatusCode = response.HttpStatus.ToString();

                        if (!openApiOperation.Responses.TryGetValue(httpStatusCode, out var oaResponse))
                        {
                            oaResponse = new OpenApiResponse
                            {
                                Description = response.Description,
                            };

                            openApiOperation.Responses[httpStatusCode] = oaResponse;
                        }
                        else
                        {
                            if (response.Description != null && oaResponse.Description != response.Description)
                            {
                                // If we have multiple responses for the same status code remove the description as
                                // it would be misleading, given it would have been only the first instance
                                oaResponse.Description = null;
                            }
                        }

                        // We do not always specify a response type, for example in command that simply respond with a
                        // 201 status code
                        if (response.Type != null)
                        {
                            // Note below assignments are once-only. We always return a ProblemResult from HTTP,
                            // so we can assume we only need to set the failure schema's once, and can
                            // only return a single type for success.
                            //
                            // If we override Content then Examples are removed.
                            if (response.Type == typeof(PlainTextResult))
                            {
                                if (!oaResponse.Content.ContainsKey("text/plain"))
                                {
                                    oaResponse.Content["text/plain"] = new OpenApiMediaType
                                    {
                                        Schema = new JsonSchema
                                        {
                                            Type = JsonObjectType.String,
                                        },
                                    };
                                }
                            }
                            else
                            {
                                if (!oaResponse.Content.ContainsKey("application/json"))
                                {
                                    // Assume for now we always return JSON
                                    oaResponse.Content["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = GetOrAddJsonSchema(
                                            GetResponseType(response),
                                            document,
                                            generator,
                                            openApiDocumentSchemaResolver),
                                    };
                                }
                            }
                        }

                        // When we have an ApiException that has additional metadata attached we try to find
                        // "type", which can be declared on the <exception /> tag to enable us to provide
                        // more details about the types of failures within a status code that could be expected
                        if (response.Metadata != null && response.Type == typeof(ApiException))
                        {
                            if (response.Metadata.TryGetValue("type", out var problemType))
                            {
                                var examples = oaResponse.Examples as Dictionary<string, object> ?? new Dictionary<string, object>();
                                oaResponse.Examples = examples;

                                examples[problemType.ToPascalCase()] = new
                                {
                                    value = new
                                    {
                                        status = response.HttpStatus,
                                        title = response.Description,
                                        type = problemType,
                                    },
                                };
                            }
                        }
                    }

                    this._options.Value.ConfigureOperation?.Invoke(openApiOperation, operation);

                    openApiPathItem[ToOpenApiOperationMethod(httpData.HttpMethod)] = openApiOperation;
                }
            }

            openApiOptions.PostConfigure?.Invoke(document);

            return document;
        }

        private static Type GetActualType(Type type)
        {
            // We do not output the individual event types, but instead consolidate down to only a concrete
            // implementation of the well-known subclasses (i.e. we would only have _one_ ResourceUpdated per
            // type instead of multiple for every subclass.
            if (type.IsOfGenericType(typeof(ResourceUpdated<>), out var concreteUpdatedGenericType) &&
                !type.IsGenericType)
            {
                return GetActualType(concreteUpdatedGenericType);
            }

            if (type.IsAbstract == false && type.IsOfGenericType(typeof(ResourceCreated<>), out var concreteCreatedType) &&
                !type.IsGenericType)
            {
                return GetActualType(concreteCreatedType);
            }

            if (type.IsAbstract == false && type.IsOfGenericType(typeof(ResourceDeleted<>), out var concreteDeletedType) &&
                !type.IsGenericType)
            {
                return GetActualType(concreteDeletedType);
            }

            return type;
        }

        private static Type GetResponseType(ResponseDescriptor response)
        {
            return response.HttpStatus switch
            {
                var x when x >= 200 && x <= 299 => response.Type,
                422 => typeof(ValidationProblemDetails),
                _ => typeof(ProblemDetails)
            };
        }

        private static OpenApiParameterKind ToKind(PropertyInfo property)
        {
            if (property.HasAttribute(typeof(FromHeaderAttribute), false))
            {
                return OpenApiParameterKind.Header;
            }

            if (property.HasAttribute(typeof(FromCookieAttribute), false))
            {
                return OpenApiParameterKind.Cookie;
            }

            if (property.HasAttribute(typeof(FromQueryAttribute), false))
            {
                return OpenApiParameterKind.Query;
            }

            return OpenApiParameterKind.Query;
        }

        private static JsonSchema GetOrAddJsonSchema(
            Type type,
            OpenApiDocument document,
            JsonSchemaGenerator generator,
            JsonSchemaResolver jsonSchemaResolver)
        {
            type = GetActualType(type);

            var jsonSchemaName = generator.Settings.SchemaNameGenerator.Generate(type);

            // We try to find in the "#/components/schemas" namespace an existing schema. If it does
            // not exist we will add one, set it's document path correctly
            if (!document.Components.Schemas.TryGetValue(jsonSchemaName, out var jsonSchema))
            {
                jsonSchema = generator.Generate(type, jsonSchemaResolver);

                // Having a separate referenced schema for an Array type seems unnecessary, so in this
                // case we will directly return the array schema, with the items having been placed into
                // the component dictionary during the generation process.
                if (jsonSchema.Type == JsonObjectType.Array)
                {
                    return jsonSchema;
                }

                if (jsonSchema.Properties.Any() == false && jsonSchema.Type == JsonObjectType.Object)
                {
                    return null;
                }

                jsonSchema.DocumentPath = "#/components/schemas/" + jsonSchemaName;
                jsonSchema.Id = jsonSchemaName;

                document.Components.Schemas[jsonSchemaName] = jsonSchema;
            }

            // The actual JsonSchema returned is only ever a reference to the common one
            // stored in "#/components/schemas"
            return new JsonSchema
            {
                Type = JsonObjectType.Object,
                Reference = jsonSchema,
            };
        }

        private static string ToOpenApiOperationMethod(string method)
        {
            if (method == "GET")
            {
                return OpenApiOperationMethod.Get;
            }

            if (method == "DELETE")
            {
                return OpenApiOperationMethod.Delete;
            }

            if (method == "HEAD")
            {
                return OpenApiOperationMethod.Head;
            }

            if (method == "OPTIONS")
            {
                return OpenApiOperationMethod.Options;
            }

            if (method == "POST")
            {
                return OpenApiOperationMethod.Post;
            }

            if (method == "PUT")
            {
                return OpenApiOperationMethod.Put;
            }

            throw new ArgumentOutOfRangeException(nameof(method));
        }

        private class BlueprintSchemaNameGenerator : DefaultSchemaNameGenerator
        {
            public override string Generate(Type type)
            {
                return base.Generate(type)
                    .Replace("ApiResource", string.Empty)
                    .Replace("PagedOf", "Paged");
            }
        }

        private class BlueprintContractResolver : CamelCasePropertyNamesContractResolver
        {
            private readonly ApiDataModel _apiDataModel;
            private readonly IEnumerable<IMessagePopulationSource> _messagePopulationSources;

            public BlueprintContractResolver(ApiDataModel apiDataModel, IEnumerable<IMessagePopulationSource> messagePopulationSources)
            {
                this._apiDataModel = apiDataModel;
                this._messagePopulationSources = messagePopulationSources;
            }

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                if (this._apiDataModel.TryFindOperation(objectType, out var descriptor) == false)
                {
                    return base.GetSerializableMembers(objectType);
                }

                var allOwned = this._messagePopulationSources
                    .SelectMany(s => s.GetOwnedProperties(this._apiDataModel, descriptor))
                    .ToList();

                return base.GetSerializableMembers(objectType)
                    .Where(p => allOwned.All(o => o.Property != p))
                    .ToList();
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var baseProperty = base.CreateProperty(member, memberSerialization);

                if (member.ToContextualMember().Nullability == Nullability.NotNullable)
                {
                    baseProperty.Required = Required.Always;
                }

                if (member.DeclaringType == typeof(ResourceEvent) && member.Name == nameof(ResourceEvent.Data))
                {
                    baseProperty.Ignored = true;
                }

                if (member.DeclaringType == typeof(ApiResource))
                {
                    if (member.Name == nameof(ApiResource.Object) ||
                        member.Name == nameof(ApiResource.Links))
                    {
                        baseProperty.Required = Required.Always;
                    }
                }

                if (member.DeclaringType == typeof(Link))
                {
                    if (member.Name == nameof(Link.Href) ||
                        member.Name == nameof(Link.Type))
                    {
                        baseProperty.Required = Required.Always;
                    }
                }

                if (member.DeclaringType.IsOfGenericType(typeof(PagedApiResource<>)))
                {
                    if (member.Name == nameof(PagedApiResource<object>.Values))
                    {
                        baseProperty.Required = Required.Always;
                    }
                }

                if (member.DeclaringType.IsOfGenericType(typeof(ListApiResource<>)))
                {
                    if (member.Name == nameof(ListApiResource<object>.Values))
                    {
                        baseProperty.Required = Required.Always;
                    }
                }

                if (member.DeclaringType == typeof(ResourceEvent))
                {
                    if (member.Name == nameof(ResourceEvent.Object) ||
                        member.Name == nameof(ResourceEvent.EventId) ||
                        member.Name == nameof(ResourceEvent.ResourceObject) ||
                        member.Name == nameof(ResourceEvent.Data))
                    {
                        baseProperty.Required = Required.Always;
                    }
                }

                if (member.DeclaringType.IsOfGenericType(typeof(ResourceEvent<>)))
                {
                    if (member.Name == nameof(ResourceEvent.Data))
                    {
                        baseProperty.Required = Required.Always;
                    }
                }

                return baseProperty;
            }
        }

        private class OpenApiDocumentSchemaResolver : JsonSchemaResolver
        {
            private readonly ITypeNameGenerator _typeNameGenerator;

            /// <summary>Initializes a new instance of the <see cref="OpenApiDocumentSchemaResolver" /> class.</summary>
            /// <param name="document">The Open API document.</param>
            /// <param name="settings">The settings.</param>
            public OpenApiDocumentSchemaResolver(OpenApiDocument document, JsonSchemaGeneratorSettings settings)
                : base(document, settings)
            {
                if (document == null)
                {
                    throw new ArgumentNullException(nameof(document));
                }

                this._typeNameGenerator = settings.TypeNameGenerator;
            }

            private OpenApiDocument Document => (OpenApiDocument)this.RootObject;

            /// <inheritdoc/>
            public override void AppendSchema(JsonSchema schema, string typeNameHint)
            {
                var schemas = this.Document.Components.Schemas;

                if (schemas.Values.Contains(schema))
                {
                    return;
                }

                schemas[this._typeNameGenerator.Generate(schema, typeNameHint, schemas.Keys)] = schema;
            }
        }
    }
}
