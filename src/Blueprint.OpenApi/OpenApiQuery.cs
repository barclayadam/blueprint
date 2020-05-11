using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Http;
using Blueprint.Api.Middleware;
using Blueprint.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;

namespace Blueprint.OpenApi
{
    /// <summary>
    /// An <see cref="IQuery" /> that can will return an OpenAPI representation of the
    /// <see cref="ApiDataModel" /> of the current API.
    /// </summary>
    [AllowAnonymous]
    [RootLink("/openapi")]
    [UnexposedOperation]
    public class OpenApiQuery : IQuery<PlainTextResult>
    {
        /// <summary>
        /// Returns the OpenAPI representation of the given <see cref="ApiDataModel" />.
        /// </summary>
        /// <param name="httpFeatureContext">The <see cref="HttpFeatureContext" />.</param>
        /// <param name="serviceProvider">Service provider used to create new <see cref="ISchemaProcessor" /> instances.</param>
        /// <param name="apiDataModel">The current data model.</param>
        /// <param name="messagePopulationSources">The registered message population sources.</param>
        /// <param name="options">The options to configure the OpenAPI document</param>
        /// <returns>An OpenAPI representation.</returns>
        public PlainTextResult Invoke(
            HttpFeatureContext httpFeatureContext,
            IServiceProvider serviceProvider,
            ApiDataModel apiDataModel,
            IEnumerable<IMessagePopulationSource> messagePopulationSources,
            IOptions<OpenApiOptions> options)
        {
            var openApiOptions = options.Value;

            var document = new OpenApiDocument
            {
                // Special case a base path of just "/" to avoid "//". We prepend "/" to
                // indicate this is relative to the URL the document was accessed at
                BasePath = httpFeatureContext.BasePath == "/" ? "/" : "/" + httpFeatureContext.BasePath,
            };

            var jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new BlueprintContractResolver(apiDataModel, messagePopulationSources),

                    Converters =
                    {
                        new StringEnumConverter(),
                    },
                },

                SchemaType = SchemaType.JsonSchema,

                FlattenInheritanceHierarchy = true,

                ReflectionService = new BlueprintReflectionService(),

                SchemaNameGenerator = new BlueprintSchemaNameGenerator(),

                DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            };

            foreach (var processor in openApiOptions.SchemaProcessors)
            {
                jsonSchemaGeneratorSettings.SchemaProcessors.Add(
                    (ISchemaProcessor)ActivatorUtilities.CreateInstance(serviceProvider, processor, apiDataModel, document));
            }

            openApiOptions.ConfigureSettings?.Invoke(jsonSchemaGeneratorSettings);

            var generator = new JsonSchemaGenerator(jsonSchemaGeneratorSettings);

            var openApiDocumentSchemaResolver = new OpenApiDocumentSchemaResolver(document, jsonSchemaGeneratorSettings);

            foreach (var operation in apiDataModel.Operations)
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
                        Description = operation.OperationType.GetXmlDocsSummary(),
                    };

                    // Use the last namespace segment as a tag of this operation, which provides a generally
                    // better structure when generating documentation or SDK clients
                    if (operation.OperationType.Namespace != null)
                    {
                        openApiOperation.Tags.Add(operation.OperationType.Namespace.Split('.').Last());
                    }

                    foreach (var routeProperty in route.Placeholders)
                    {
                        openApiOperation.Parameters.Add(new OpenApiParameter
                        {
                            Kind = OpenApiParameterKind.Path,
                            Name = routeProperty.Property.Name,
                            IsRequired = true,
                            Schema = JsonSchema.FromType(routeProperty.Property.PropertyType),
                            Description = routeProperty.Property.GetXmlDocsSummary(),
                        });
                    }

                    foreach (var response in operation.Responses)
                    {
                        var jsonSchema = GetOrAddJsonSchema(
                            response.Type,
                            document,
                            generator,
                            openApiDocumentSchemaResolver);

                        openApiOperation.Responses["200"] = new OpenApiResponse
                        {
                            Schema = jsonSchema,
                        };
                    }

                    // If the operation is a command then it takes a command body if there are non-route properties
                    if (operation.IsCommand)
                    {
                        var bodySchema = GetCommandBodySchema(operation, route, document, generator, openApiDocumentSchemaResolver);

                        if (bodySchema != null)
                        {
                            openApiOperation.Parameters.Add(new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.Body,
                                Schema = bodySchema,
                            });
                        }
                    }
                    else
                    {
                        // If this is not a command we assume all parameters come in from the query string
                        foreach (var parameter in operation.Properties)
                        {
                            // We have already handled this property above when adding route
                            // parameters
                            if (route.Placeholders.Any(p => p.Property == parameter))
                            {
                                continue;
                            }

                            openApiOperation.Parameters.Add(new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.Query,

                                Name = parameter.Name,

                                IsRequired = parameter.ToContextualProperty().Nullability == Nullability.NotNullable ||
                                             parameter.GetCustomAttributes<RequiredAttribute>().Any(),

                                // Exclude "owned" properties?
                                Schema = generator.Generate(parameter.PropertyType),
                            });
                        }
                    }

                    openApiPathItem[ToOpenApiOperationMethod(httpData.HttpMethod)] = openApiOperation;
                }
            }

            openApiOptions.PostConfigure?.Invoke(document);

            return new PlainTextResult(document.ToJson(openApiOptions.SchemaType, openApiOptions.Formatting))
            {
                ContentType = "application/json",
            };
        }

        internal static JsonSchema? GetCommandBodySchema(
            ApiOperationDescriptor operation,
            ApiOperationLink route,
            OpenApiDocument document,
            JsonSchemaGenerator generator,
            JsonSchemaResolver openApiDocumentSchemaResolver)
        {
            var nonRouteProperties = operation
                .Properties
                .Where(p => route.Placeholders.All(ph => ph.Property != p));

            if (nonRouteProperties.Any())
            {
                // Exclude "owned" properties?
                return GetOrAddJsonSchema(
                    operation.OperationType,
                    document,
                    generator,
                    openApiDocumentSchemaResolver);
            }

            return null;
        }

        internal static Type GetActualType(Type type)
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
            private readonly ApiDataModel apiDataModel;
            private readonly IEnumerable<IMessagePopulationSource> messagePopulationSources;

            public BlueprintContractResolver(ApiDataModel apiDataModel, IEnumerable<IMessagePopulationSource> messagePopulationSources)
            {
                this.apiDataModel = apiDataModel;
                this.messagePopulationSources = messagePopulationSources;
            }

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var descriptor = apiDataModel.Operations.SingleOrDefault(o => o.OperationType == objectType);

                if (descriptor == null)
                {
                    return base.GetSerializableMembers(objectType);
                }

                var allOwned = messagePopulationSources
                    .SelectMany(s => s.GetOwnedProperties(apiDataModel, descriptor))
                    .ToList();

                return base.GetSerializableMembers(objectType)
                    .Where(p => !allOwned.Contains(p))
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

        private class BlueprintReflectionService : DefaultReflectionService
        {
            public override bool IsNullable(ContextualType contextualType, ReferenceTypeNullHandling defaultReferenceTypeNullHandling)
            {
                return false;
            }
        }

        private class OpenApiDocumentSchemaResolver : JsonSchemaResolver
        {
            private readonly ITypeNameGenerator typeNameGenerator;

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

                typeNameGenerator = settings.TypeNameGenerator;
            }

            private OpenApiDocument Document => (OpenApiDocument)RootObject;

            /// <inheritdoc/>
            public override void AppendSchema(JsonSchema schema, string typeNameHint)
            {
                var schemas = Document.Components.Schemas;

                if (schemas.Values.Contains(schema))
                {
                    return;
                }

                schemas[typeNameGenerator.Generate(schema, typeNameHint, schemas.Keys)] = schema;
            }
        }
    }
}
