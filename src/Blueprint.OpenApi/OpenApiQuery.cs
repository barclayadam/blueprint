using System;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Http;
using NJsonSchema;
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
    public class OpenApiQuery : IQuery
    {
        /// <summary>
        /// Returns the OpenAPI representation of the given <see cref="ApiDataModel" />.
        /// </summary>
        /// <param name="apiDataModel">The current data model.</param>
        /// <returns>An OpenAPI representation.</returns>
        public async Task<object> InvokeAsync(ApiDataModel apiDataModel)
        {
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "OpenAPI Specification",
                },

                // Paths = new OpenApiPaths
                // {
                //     ["/pets"] = new OpenApiPathItem
                //     {
                //         Operations = new Dictionary<OpenApiOperationMethod, OpenApiOperation>
                //         {
                //             [OpenApiOperationMethod.Get] = new OpenApiOperation
                //             {
                //                 Description = "Returns all pets from the system that the user has access to",
                //
                //                 Responses = new OpenApiResponses
                //                 {
                //                     ["200"] = new OpenApiResponse
                //                     {
                //                         Description = "OK",
                //                     },
                //                 },
                //             },
                //         },
                //     },
                // },
            };

            foreach (var operation in apiDataModel.Operations)
            {
                if (!operation.IsExposed)
                {
                    continue;
                }

                var httpData = operation.GetFeatureData<HttpOperationFeatureData>();

                foreach (var route in operation.Links)
                {
                    if(!document.Paths.TryGetValue(route.RoutingUrl, out var openApiPathItem))
                    {
                        openApiPathItem = new OpenApiPathItem();
                        document.Paths[route.RoutingUrl] = openApiPathItem;
                    }

                    var openApiOperation = new OpenApiOperation
                    {
                        OperationId = operation.Name,
                    };

                    foreach (var routeProperty in route.Placeholders)
                    {
                        openApiOperation.Parameters.Add(new OpenApiParameter
                        {
                            Kind = OpenApiParameterKind.Path,
                            Name = routeProperty.Property.Name,
                            IsRequired = true,
                            Schema = JsonSchema.FromType(routeProperty.Property.PropertyType),
                        });
                    }

                    openApiPathItem[ToOpenApiOperationMethod(httpData.HttpMethod)] = openApiOperation;
                }
            }

            return document;
        }

        private string ToOpenApiOperationMethod(string method)
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
    }
}
