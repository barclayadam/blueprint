using System.Collections.Generic;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Http;
using NJsonSchema.Generation;
using NSwag;

namespace Blueprint.OpenApi
{
    /// <summary>
    /// A <see cref="ISchemaProcessor" /> that will add a vendor extension of <c>x-links</c> to
    /// a generated schema to represents <see cref="Link" />s that are registered for that
    /// API resource type.
    /// </summary>
    public class BlueprintLinkSchemaProcessor : ISchemaProcessor
    {
        private readonly ApiDataModel apiDataModel;
        private readonly OpenApiDocument openApiDocument;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintLinkSchemaProcessor" /> class.
        /// </summary>
        /// <param name="apiDataModel">The <see cref="ApiDataModel" /> being processed.</param>
        /// <param name="openApiDocument">The <see cref="OpenApiDocument" /> being generated.</param>
        public BlueprintLinkSchemaProcessor(ApiDataModel apiDataModel, OpenApiDocument openApiDocument)
        {
            this.apiDataModel = apiDataModel;
            this.openApiDocument = openApiDocument;
        }

        /// <inheritdoc />
        public void Process(SchemaProcessorContext context)
        {
            if (!typeof(ApiResource).IsAssignableFrom(context.Type))
            {
                return;
            }

            var resourceLinks = apiDataModel
                .GetLinksForResource(context.Type)
                .Where(l => l.Rel != "self")
                .ToList();

            if (resourceLinks.Any())
            {
                context.Schema.ExtensionData ??= new Dictionary<string, object>();

                context.Schema.ExtensionData["x-links"] = resourceLinks.Select(r => new
                {
                    rel = r.Rel,
                    operationId = r.OperationDescriptor.Name,
                    method = r.OperationDescriptor.GetFeatureData<HttpOperationFeatureData>().HttpMethod,
                    responseSchema = context.Settings.SchemaNameGenerator.Generate(OpenApiQuery.GetActualType(r.OperationDescriptor.Responses.Single(r => r.Category == ResponseDescriptorCategory.Success).Type)),
                    body = OpenApiQuery.GetCommandBodySchema(r.OperationDescriptor, r, this.openApiDocument, context.Generator, context.Resolver)?.Reference.Id,
                });
            }
        }
    }
}
